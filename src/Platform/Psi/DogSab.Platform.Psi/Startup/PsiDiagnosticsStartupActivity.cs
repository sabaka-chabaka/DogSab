using DogSab.Platform.Core.Abstractions.Lifecycle;
using DogSab.Platform.Core.Abstractions.Logging;
using DogSab.Platform.Extensibility.Abstractions.Attributes;
using DogSab.Platform.Psi.Abstractions.Registry;

namespace DogSab.Platform.Psi.Startup;

/// <summary>
/// Platform startup activity that logs which languages are registered,
/// mirroring the diagnostics pattern used throughout the platform.
/// </summary>
[Extension("core.startupActivity")]
public sealed class PsiDiagnosticsStartupActivity : IStartupActivity
{
    private readonly ILanguageRegistry _languageRegistry;
    private readonly ILoggerFactory _loggerFactory;

    public PsiDiagnosticsStartupActivity(ILanguageRegistry languageRegistry, ILoggerFactory loggerFactory)
    {
        _languageRegistry = languageRegistry;
        _loggerFactory = loggerFactory;
    }

    public int Order => 2000;

    public Task RunActivityAsync(CancellationToken cancellationToken)
    {
        var logger = _loggerFactory.GetLogger(typeof(PsiDiagnosticsStartupActivity));
        var languages = _languageRegistry.AllLanguages;

        logger.Info("PSI diagnostics: {0} language(s) registered.", languages.Count);

        foreach (var language in languages)
        {
            logger.Debug(
                "Language '{0}' ({1}): extensions [{2}]",
                language.Id,
                language.DisplayName,
                string.Join(", ", language.FileExtensions));
        }

        return Task.CompletedTask;
    }
}