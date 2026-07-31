using System.Diagnostics;

namespace RATracker.WPF.Http.V2;

/// <summary>
/// Provides logging and observability for V2 API requests.
/// </summary>
public interface IV2ApiLogger
{
    /// <summary>
    /// Logs an API request being sent.
    /// </summary>
    /// <param name="method">HTTP method.</param>
    /// <param name="url">The request URL (API key will be redacted).</param>
    void LogRequest(string method, string url);

    /// <summary>
    /// Logs an API response received.
    /// </summary>
    /// <param name="method">HTTP method.</param>
    /// <param name="url">The request URL (API key will be redacted).</param>
    /// <param name="statusCode">HTTP status code.</param>
    /// <param name="latencyMs">Request latency in milliseconds.</param>
    /// <param name="success">Whether the request was successful.</param>
    void LogResponse(string method, string url, int statusCode, long latencyMs, bool success);

    /// <summary>
    /// Logs an API error.
    /// </summary>
    /// <param name="method">HTTP method.</param>
    /// <param name="url">The request URL (API key will be redacted).</param>
    /// <param name="error">The error message.</param>
    /// <param name="statusCode">HTTP status code if available.</param>
    void LogError(string method, string url, string error, int? statusCode = null);
}

/// <summary>
/// Tracks API metrics for observability.
/// </summary>
public class V2ApiMetrics
{
    private long _totalRequests;
    private long _successfulRequests;
    private long _failedRequests;
    private long _totalLatencyMs;
    private readonly object _lock = new();

    /// <summary>
    /// Gets the total number of requests made.
    /// </summary>
    public long TotalRequests => Interlocked.Read(ref _totalRequests);

    /// <summary>
    /// Gets the number of successful requests.
    /// </summary>
    public long SuccessfulRequests => Interlocked.Read(ref _successfulRequests);

    /// <summary>
    /// Gets the number of failed requests.
    /// </summary>
    public long FailedRequests => Interlocked.Read(ref _failedRequests);

    /// <summary>
    /// Gets the success rate as a percentage.
    /// </summary>
    public double SuccessRate
    {
        get
        {
            var total = TotalRequests;
            return total > 0 ? (double)SuccessfulRequests / total * 100 : 0;
        }
    }

    /// <summary>
    /// Gets the error rate as a percentage.
    /// </summary>
    public double ErrorRate
    {
        get
        {
            var total = TotalRequests;
            return total > 0 ? (double)FailedRequests / total * 100 : 0;
        }
    }

    /// <summary>
    /// Gets the average latency in milliseconds.
    /// </summary>
    public double AverageLatencyMs
    {
        get
        {
            var total = TotalRequests;
            return total > 0 ? (double)Interlocked.Read(ref _totalLatencyMs) / total : 0;
        }
    }

    /// <summary>
    /// Records a successful request.
    /// </summary>
    public void RecordSuccess(long latencyMs)
    {
        Interlocked.Increment(ref _totalRequests);
        Interlocked.Increment(ref _successfulRequests);
        Interlocked.Add(ref _totalLatencyMs, latencyMs);
    }

    /// <summary>
    /// Records a failed request.
    /// </summary>
    public void RecordFailure(long latencyMs)
    {
        Interlocked.Increment(ref _totalRequests);
        Interlocked.Increment(ref _failedRequests);
        Interlocked.Add(ref _totalLatencyMs, latencyMs);
    }

    /// <summary>
    /// Resets all metrics.
    /// </summary>
    public void Reset()
    {
        Interlocked.Exchange(ref _totalRequests, 0);
        Interlocked.Exchange(ref _successfulRequests, 0);
        Interlocked.Exchange(ref _failedRequests, 0);
        Interlocked.Exchange(ref _totalLatencyMs, 0);
    }
}

/// <summary>
/// Default implementation of IV2ApiLogger that logs to Debug output.
/// </summary>
public class DebugApiLogger : IV2ApiLogger
{
    private readonly V2ApiMetrics _metrics;
    private readonly bool _enabled;

    /// <summary>
    /// Creates a new DebugApiLogger.
    /// </summary>
    /// <param name="metrics">Optional metrics tracker.</param>
    /// <param name="enabled">Whether logging is enabled.</param>
    public DebugApiLogger(V2ApiMetrics? metrics = null, bool enabled = true)
    {
        _metrics = metrics ?? new V2ApiMetrics();
        _enabled = enabled;
    }

    /// <summary>
    /// Gets the metrics tracker.
    /// </summary>
    public V2ApiMetrics Metrics => _metrics;

    /// <inheritdoc />
    public void LogRequest(string method, string url)
    {
        if (!_enabled) return;

        var redactedUrl = RedactApiKey(url);
        Debug.WriteLine($"[V2API] {method} {redactedUrl}");
    }

    /// <inheritdoc />
    public void LogResponse(string method, string url, int statusCode, long latencyMs, bool success)
    {
        if (success)
        {
            _metrics.RecordSuccess(latencyMs);
        }
        else
        {
            _metrics.RecordFailure(latencyMs);
        }

        if (!_enabled) return;

        var redactedUrl = RedactApiKey(url);
        var statusText = success ? "OK" : "FAIL";
        Debug.WriteLine($"[V2API] {method} {redactedUrl} -> {statusCode} {statusText} ({latencyMs}ms)");
    }

    /// <inheritdoc />
    public void LogError(string method, string url, string error, int? statusCode = null)
    {
        if (!_enabled) return;

        var redactedUrl = RedactApiKey(url);
        var statusText = statusCode.HasValue ? $"[{statusCode}] " : "";
        Debug.WriteLine($"[V2API] ERROR {method} {redactedUrl} -> {statusText}{error}");
    }

    /// <summary>
    /// Redacts API key from URL for safe logging.
    /// </summary>
    private static string RedactApiKey(string url)
    {
        // Redact X-API-Key from query params if present (shouldn't be, but just in case)
        // The API key is in the header, not URL, but redact any sensitive patterns
        return System.Text.RegularExpressions.Regex.Replace(
            url,
            @"(api[-_]?key|token|secret|password|key)=([^&\s]+)",
            "$1=***REDACTED***",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
}

/// <summary>
/// A null logger that does nothing (for testing or when logging is disabled).
/// </summary>
public class NullApiLogger : IV2ApiLogger
{
    /// <summary>
    /// Singleton instance.
    /// </summary>
    public static readonly NullApiLogger Instance = new();

    private NullApiLogger() { }

    public void LogRequest(string method, string url) { }
    public void LogResponse(string method, string url, int statusCode, long latencyMs, bool success) { }
    public void LogError(string method, string url, string error, int? statusCode = null) { }
}
