using System.Diagnostics;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Core.Abstractions.Messaging;
using DogSab.Platform.Extensibility.Abstractions.ExtensionPoints;
using DogSab.Platform.RunConfigurations.Abstractions;
using DogSab.Platform.RunConfigurations.Abstractions.Events;

namespace DogSab.Platform.RunConfigurations;

/// <summary>
/// Launches an <see cref="IRunConfiguration"/> as an actual OS process,
/// resolving its launch specification via the matching registered
/// <see cref="IRunConfigurationType"/> and wrapping the result as an
/// <see cref="IRunProcessHandle"/>.
/// The single place in the platform that touches
/// <see cref="System.Diagnostics.Process"/> directly.
/// </summary>
public sealed class ProcessRunner
{
    /// <summary>
    /// The platform's extension point registry, used to look up the
    /// configuration type matching a configuration's declared type ID.
    /// </summary>
    private readonly IExtensionPointRegistry _extensionPointRegistry;

    /// <summary>
    /// Used to publish run state changes to the platform-wide topic.
    /// </summary>
    private readonly IMessageBus _messageBus;

    /// <summary>
    /// Logger used to report launch failures.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a new process runner.
    /// </summary>
    /// <param name="extensionPointRegistry">
    /// The registry to resolve configuration types from.
    /// </param>
    /// <param name="messageBus">
    /// The message bus to publish run state changes on.
    /// </param>
    /// <param name="loggerFactory">
    /// Factory used to obtain a logger scoped to this runner.
    /// </param>
    public ProcessRunner(IExtensionPointRegistry extensionPointRegistry, IMessageBus messageBus, ILoggerFactory loggerFactory)
    {
        _extensionPointRegistry = extensionPointRegistry;
        _messageBus = messageBus;
        _logger = loggerFactory.GetLogger(typeof(ProcessRunner));
    }

    /// <summary>
    /// Launches a run configuration.
    /// </summary>
    /// <param name="configuration">
    /// The configuration to launch.
    /// </param>
    /// <param name="configurationTypeId">
    /// The type ID the configuration was created from, used to resolve
    /// which registered <see cref="IRunConfigurationType"/> can derive its
    /// launch specification. Passed separately rather than being a
    /// property of <see cref="IRunConfiguration"/> itself, since which
    /// type created a configuration is bookkeeping the platform needs
    /// for dispatch, not something meaningful to the configuration's own
    /// public contract.
    /// </param>
    /// <returns>
    /// A handle to the launched process.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no registered configuration type matches
    /// <paramref name="configurationTypeId"/>.
    /// </exception>
    public IRunProcessHandle Run(IRunConfiguration configuration, string configurationTypeId)
    {
        var configurationType = _extensionPointRegistry
            .GetExtensions(RunExtensionPoints.RUN_CONFIGURATION_TYPE)
            .FirstOrDefault(t => t.TypeId == configurationTypeId);

        if (configurationType is null)
        {
            throw new InvalidOperationException(
                $"No registered run configuration type found for id '{configurationTypeId}'.");
        }

        var launchSpec = configurationType.CreateLaunchSpecification(configuration);

        return StartProcess(configuration, launchSpec);
    }

    /// <summary>
    /// Starts the actual OS process for a resolved launch specification,
    /// combining its base arguments with the configuration's own
    /// user-supplied arguments and environment variables.
    /// </summary>
    /// <param name="configuration">
    /// The configuration being launched, for its own arguments/environment
    /// variables and for attributing the resulting handle back to it.
    /// </param>
    /// <param name="launchSpec">
    /// The resolved executable path, base arguments, and working directory.
    /// </param>
    /// <returns>
    /// A handle to the launched (or failed-to-launch) process.
    /// </returns>
    private IRunProcessHandle StartProcess(IRunConfiguration configuration, LaunchSpecification launchSpec)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = launchSpec.ExecutablePath,
            WorkingDirectory = launchSpec.WorkingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var arg in launchSpec.BaseArguments.Concat(configuration.Arguments))
        {
            startInfo.ArgumentList.Add(arg);
        }

        foreach (var (key, value) in configuration.EnvironmentVariables)
        {
            startInfo.EnvironmentVariables[key] = value;
        }

        var handle = new RunProcessHandleImpl(configuration.Id);

        try
        {
            var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

            process.OutputDataReceived += (_, e) => { if (e.Data is not null) handle.ReportOutput(e.Data); };
            process.ErrorDataReceived += (_, e) => { if (e.Data is not null) handle.ReportOutput(e.Data); };
            process.Exited += (_, _) => handle.ReportExited(process.ExitCode);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            handle.AttachStopAction(process.Kill);
            handle.TransitionTo(RunState.Running);
        }
        catch (Exception ex)
        {
            _logger.Error(
                "Failed to launch run configuration '{0}' ('{1}')",
                ex,
                configuration.DisplayName,
                launchSpec.ExecutablePath);

            handle.TransitionTo(RunState.Failed);
        }

        handle.StateChanged += _ => _messageBus.Publisher(RunExtensionPoints.RUN_STATE_CHANGED).RunStateChanged(handle);

        return handle;
    }

    /// <summary>
    /// Default implementation of <see cref="IRunProcessHandle"/>, mutable
    /// internally by <see cref="ProcessRunner"/> as the underlying process
    /// progresses through its lifecycle.
    /// </summary>
    private sealed class RunProcessHandleImpl : IRunProcessHandle
    {
        private Action? _stopAction;

        /// <inheritdoc />
        public RunConfigurationId ConfigurationId { get; }

        /// <inheritdoc />
        public RunState State { get; private set; } = RunState.NotStarted;

        /// <inheritdoc />
        public int? ExitCode { get; private set; }

        /// <inheritdoc />
        public event Action<string>? OutputReceived;

        /// <inheritdoc />
        public event Action<RunState>? StateChanged;

        /// <summary>
        /// Creates a new process handle for a given configuration.
        /// </summary>
        /// <param name="configurationId">
        /// The configuration this process was launched from.
        /// </param>
        public RunProcessHandleImpl(RunConfigurationId configurationId)
        {
            ConfigurationId = configurationId;
        }

        /// <inheritdoc />
        public void Stop()
        {
            if (State != RunState.Running)
            {
                return;
            }

            _stopAction?.Invoke();
        }

        /// <summary>
        /// Records the action used to actually terminate the underlying
        /// process, called from <see cref="Stop"/>.
        /// </summary>
        /// <param name="stopAction">
        /// The action that terminates the process.
        /// </param>
        public void AttachStopAction(Action stopAction)
        {
            _stopAction = stopAction;
        }

        /// <summary>
        /// Reports a new line of output from the process.
        /// </summary>
        /// <param name="line">
        /// The output line received.
        /// </param>
        public void ReportOutput(string line)
        {
            OutputReceived?.Invoke(line);
        }

        /// <summary>
        /// Reports that the process has exited, transitioning to
        /// <see cref="RunState.Stopped"/> and recording its exit code.
        /// </summary>
        /// <param name="exitCode">
        /// The process's exit code.
        /// </param>
        public void ReportExited(int exitCode)
        {
            ExitCode = exitCode;
            TransitionTo(RunState.Stopped);
        }

        /// <summary>
        /// Transitions this handle to a new state, raising
        /// <see cref="StateChanged"/>.
        /// </summary>
        /// <param name="newState">
        /// The state to transition to.
        /// </param>
        public void TransitionTo(RunState newState)
        {
            State = newState;
            StateChanged?.Invoke(newState);
        }
    }
}