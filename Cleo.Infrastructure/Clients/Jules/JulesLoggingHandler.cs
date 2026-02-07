using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Cleo.Infrastructure.Clients.Jules;

/// <summary>
/// A diagnostic handler that logs HTTP request and response details for the Jules client.
/// </summary>
internal sealed partial class JulesLoggingHandler : DelegatingHandler
{
    private readonly ILogger<JulesLoggingHandler> _logger;

    public JulesLoggingHandler(ILogger<JulesLoggingHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        LogSendingRequest(_logger, request.Method, request.RequestUri);

        try
        {
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            stopwatch.Stop();

            if (response.IsSuccessStatusCode)
            {
                LogResponseSuccess(_logger, (int)response.StatusCode, request.RequestUri, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                LogResponseWarning(_logger, (int)response.StatusCode, request.RequestUri, stopwatch.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogRequestFailed(_logger, ex, request.RequestUri, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "📡 Sending {Method} to {Url}")]
    private static partial void LogSendingRequest(ILogger logger, HttpMethod method, Uri? url);

    [LoggerMessage(Level = LogLevel.Information, Message = "✅ Received {StatusCode} from {Url} in {Elapsed}ms")]
    private static partial void LogResponseSuccess(ILogger logger, int statusCode, Uri? url, long elapsed);

    [LoggerMessage(Level = LogLevel.Warning, Message = "⚠️ Received {StatusCode} from {Url} in {Elapsed}ms")]
    private static partial void LogResponseWarning(ILogger logger, int statusCode, Uri? url, long elapsed);

    [LoggerMessage(Level = LogLevel.Error, Message = "❌ Request to {Url} failed after {Elapsed}ms")]
    private static partial void LogRequestFailed(ILogger logger, Exception ex, Uri? url, long elapsed);
}
