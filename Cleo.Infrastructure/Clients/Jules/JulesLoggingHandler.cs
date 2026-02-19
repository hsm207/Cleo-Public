using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Cleo.Infrastructure.Clients.Jules;

/// <summary>
/// A specialized delegating handler for logging Jules API interactions.
/// </summary>
internal sealed partial class JulesLoggingHandler : DelegatingHandler
{
    private readonly ILogger<JulesLoggingHandler> _logger;

    public JulesLoggingHandler(ILogger<JulesLoggingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        LogRequest(request.Method, request.RequestUri);

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        stopwatch.Stop();

        if (response.IsSuccessStatusCode)
        {
            LogSuccess(response.StatusCode, request.RequestUri, stopwatch.ElapsedMilliseconds);
        }
        else
        {
            LogWarning(response.StatusCode, request.RequestUri, stopwatch.ElapsedMilliseconds);
        }

        return response;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "📡 Sending {Method} to {Url}")]
    private partial void LogRequest(HttpMethod method, Uri? url);

    [LoggerMessage(Level = LogLevel.Information, Message = "✅ Received {StatusCode} from {Url} in {Elapsed}ms")]
    private partial void LogSuccess(System.Net.HttpStatusCode statusCode, Uri? url, long elapsed);

    [LoggerMessage(Level = LogLevel.Warning, Message = "⚠️ Received {StatusCode} from {Url} in {Elapsed}ms")]
    private partial void LogWarning(System.Net.HttpStatusCode statusCode, Uri? url, long elapsed);
}
