using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Nodes;
using DogSab.Platform.Core.Abstractions.Logging;

namespace DogSab.Platform.Debugger;

/// <summary>
/// A minimal client for the Debug Adapter Protocol (DAP): launches a debug
/// adapter as a child process and exchanges JSON-RPC-style messages with it
/// over its standard input/output streams, following DAP's
/// <c>Content-Length</c>-prefixed framing.
/// This is a deliberately narrow implementation covering only
/// request/response and event message shapes needed to drive a basic debug
/// session (launch, set breakpoints, continue, step, stack trace,
/// variables) — DAP defines a much larger surface (e.g. exception
/// breakpoints, data breakpoints, multi-target debugging) that a real
/// production debugger integration would need to support, left
/// unimplemented here. Concrete <c>IDebugProcessProvider</c> plugins (e.g.
/// a future netcoredbg integration) are expected to build on top of this
/// client, not reimplement DAP framing themselves.
/// </summary>
public sealed class DapClient : IAsyncDisposable
{
    /// <summary>
    /// The underlying debug adapter process.
    /// </summary>
    private readonly Process _process;

    /// <summary>
    /// Logger used to report protocol-level errors.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Pending requests awaiting a response, keyed by their DAP sequence
    /// number, so an incoming response can be routed back to the caller
    /// that issued the matching request.
    /// </summary>
    private readonly ConcurrentDictionary<int, TaskCompletionSource<JsonNode>> _pendingRequests = new();

    /// <summary>
    /// The next sequence number to assign to an outgoing request, per the
    /// DAP specification's requirement that each message carry a
    /// monotonically increasing <c>seq</c> field.
    /// </summary>
    private int _nextSequenceNumber = 1;

    /// <summary>
    /// Raised for every DAP event message received (as opposed to a
    /// response to a request), e.g. <c>"stopped"</c> when a breakpoint is
    /// hit, or <c>"output"</c> for debuggee console output. Callers (a
    /// future concrete <see cref="Abstractions.IDebugProcessProvider"/>
    /// implementation) are expected to inspect the event's <c>"event"</c>
    /// field to determine its kind and dispatch accordingly — this client
    /// does not itself interpret event semantics.
    /// </summary>
    public event Action<JsonNode>? EventReceived;

    /// <summary>
    /// The background task reading and dispatching incoming messages from
    /// the adapter's standard output, started in <see cref="StartAsync"/>.
    /// </summary>
    private Task? _readLoopTask;

    /// <summary>
    /// Creates a new DAP client wrapping a not-yet-started debug adapter process.
    /// </summary>
    /// <param name="adapterExecutablePath">
    /// The path to the debug adapter executable to launch.
    /// </param>
    /// <param name="adapterArguments">
    /// Arguments to pass to the debug adapter.
    /// </param>
    /// <param name="loggerFactory">
    /// Factory used to obtain a logger scoped to this client.
    /// </param>
    public DapClient(string adapterExecutablePath, string[] adapterArguments, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.GetLogger(typeof(DapClient));

        var startInfo = new ProcessStartInfo
        {
            FileName = adapterExecutablePath,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var arg in adapterArguments)
        {
            startInfo.ArgumentList.Add(arg);
        }

        _process = new Process { StartInfo = startInfo };
    }

    /// <summary>
    /// Starts the debug adapter process and begins reading its output
    /// stream for incoming messages.
    /// </summary>
    public void Start()
    {
        _process.Start();
        _readLoopTask = Task.Run(RunReadLoopAsync);
    }

    /// <summary>
    /// Sends a DAP request and asynchronously awaits its matching response.
    /// </summary>
    /// <param name="command">
    /// The DAP command name (e.g. <c>"launch"</c>, <c>"continue"</c>,
    /// <c>"stackTrace"</c>).
    /// </param>
    /// <param name="arguments">
    /// The command's arguments object, or <c>null</c> for commands that
    /// take none.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel waiting for the response.
    /// </param>
    /// <returns>
    /// The response's <c>"body"</c> field, as a parsed JSON node.
    /// </returns>
    public async Task<JsonNode> SendRequestAsync(string command, JsonObject? arguments, CancellationToken cancellationToken)
    {
        var sequenceNumber = Interlocked.Increment(ref _nextSequenceNumber);

        var requestObject = new JsonObject
        {
            ["seq"] = sequenceNumber,
            ["type"] = "request",
            ["command"] = command,
            ["arguments"] = arguments
        };

        var completionSource = new TaskCompletionSource<JsonNode>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingRequests[sequenceNumber] = completionSource;

        await WriteMessageAsync(requestObject).ConfigureAwait(false);

        await using var registration = cancellationToken.Register(() => completionSource.TrySetCanceled(cancellationToken));

        return await completionSource.Task.ConfigureAwait(false);
    }

    /// <summary>
    /// Serializes a message with DAP's required <c>Content-Length</c>
    /// header framing and writes it to the adapter's standard input.
    /// </summary>
    /// <param name="message">
    /// The message object to send.
    /// </param>
    private async Task WriteMessageAsync(JsonObject message)
    {
        var json = message.ToJsonString();
        var bodyBytes = Encoding.UTF8.GetBytes(json);
        var header = $"Content-Length: {bodyBytes.Length}\r\n\r\n";
        var headerBytes = Encoding.ASCII.GetBytes(header);

        await _process.StandardInput.BaseStream.WriteAsync(headerBytes).ConfigureAwait(false);
        await _process.StandardInput.BaseStream.WriteAsync(bodyBytes).ConfigureAwait(false);
        await _process.StandardInput.BaseStream.FlushAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Continuously reads DAP-framed messages from the adapter's standard
    /// output, dispatching each as either a response to a pending request
    /// or an <see cref="EventReceived"/> notification, until the process
    /// exits or the stream ends.
    /// A known simplification: this loop's header-parsing does not handle
    /// every edge case DAP's framing technically allows (e.g. additional
    /// headers beyond <c>Content-Length</c>), only the common single-header
    /// case every debug adapter implementation in practice sends.
    /// </summary>
    private async Task RunReadLoopAsync()
    {
        var stream = _process.StandardOutput.BaseStream;

        try
        {
            while (!_process.HasExited)
            {
                var contentLength = await ReadContentLengthHeaderAsync(stream).ConfigureAwait(false);

                if (contentLength is null)
                {
                    break;
                }

                var bodyBytes = new byte[contentLength.Value];
                var bytesRead = 0;

                while (bytesRead < bodyBytes.Length)
                {
                    var read = await stream.ReadAsync(bodyBytes.AsMemory(bytesRead)).ConfigureAwait(false);
                    if (read == 0)
                    {
                        return;
                    }
                    bytesRead += read;
                }

                var json = Encoding.UTF8.GetString(bodyBytes);
                DispatchMessage(json);
            }
        }
        catch (Exception ex)
        {
            _logger.Error("DAP read loop terminated unexpectedly", ex);
        }
    }

    /// <summary>
    /// Reads and parses a single <c>Content-Length</c> header line from
    /// the stream, per DAP's framing, up to and including the blank line
    /// terminating the header block.
    /// </summary>
    /// <param name="stream">
    /// The stream to read the header from.
    /// </param>
    /// <returns>
    /// The parsed content length, or <c>null</c> if the stream ended
    /// before a complete header was read.
    /// </returns>
    private static async Task<int?> ReadContentLengthHeaderAsync(System.IO.Stream stream)
    {
        var headerBuilder = new StringBuilder();
        var buffer = new byte[1];

        while (true)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(0, 1)).ConfigureAwait(false);
            if (read == 0)
            {
                return null;
            }

            headerBuilder.Append((char)buffer[0]);

            if (headerBuilder.Length >= 4 &&
                headerBuilder[^4] == '\r' && headerBuilder[^3] == '\n' &&
                headerBuilder[^2] == '\r' && headerBuilder[^1] == '\n')
            {
                break;
            }
        }

        var headerText = headerBuilder.ToString();
        var contentLengthLine = headerText.Split("\r\n", StringSplitOptions.RemoveEmptyEntries)[0];
        var lengthValue = contentLengthLine.Split(':')[1].Trim();

        return int.Parse(lengthValue);
    }

    /// <summary>
    /// Parses a received message body and routes it either to a pending
    /// request's completion source (if it is a <c>"response"</c> matching
    /// a known <c>request_seq</c>) or to <see cref="EventReceived"/> (if it
    /// is an <c>"event"</c>).
    /// </summary>
    /// <param name="json">
    /// The raw JSON text of the received message.
    /// </param>
    private void DispatchMessage(string json)
    {
        var node = JsonNode.Parse(json);

        if (node is not JsonObject messageObject)
        {
            return;
        }

        var messageType = messageObject["type"]?.GetValue<string>();

        if (messageType == "response")
        {
            var requestSeq = messageObject["request_seq"]?.GetValue<int>();

            if (requestSeq is { } seq && _pendingRequests.TryRemove(seq, out var completionSource))
            {
                var body = messageObject["body"] ?? new JsonObject();
                completionSource.TrySetResult(body);
            }
        }
        else if (messageType == "event")
        {
            EventReceived?.Invoke(messageObject);
        }
    }

    /// <summary>
    /// Stops the debug adapter process and releases resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        try
        {
            if (!_process.HasExited)
            {
                _process.Kill();
            }
        }
        catch (InvalidOperationException)
        {
            // Process already exited between the check and the kill attempt.
        }

        if (_readLoopTask is not null)
        {
            try
            {
                await _readLoopTask.ConfigureAwait(false);
            }
            catch
            {
                // Read loop's own exceptions are already logged internally.
            }
        }

        _process.Dispose();
    }
}