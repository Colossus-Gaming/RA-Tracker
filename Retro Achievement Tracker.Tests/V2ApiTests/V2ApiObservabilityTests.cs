using RATracker.WPF.Http.V2;

namespace RATracker.Tests.V2ApiTests;

/// <summary>
/// Tests for V2 API logging and observability.
/// </summary>
[TestFixture]
public class V2ApiObservabilityTests
{
    #region V2ApiMetrics Tests

    [Test]
    public void Metrics_InitialState_AllZeros()
    {
        // Arrange
        var metrics = new V2ApiMetrics();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(metrics.TotalRequests, Is.EqualTo(0));
            Assert.That(metrics.SuccessfulRequests, Is.EqualTo(0));
            Assert.That(metrics.FailedRequests, Is.EqualTo(0));
            Assert.That(metrics.SuccessRate, Is.EqualTo(0));
            Assert.That(metrics.ErrorRate, Is.EqualTo(0));
            Assert.That(metrics.AverageLatencyMs, Is.EqualTo(0));
        });
    }

    [Test]
    public void Metrics_RecordSuccess_IncrementsCorrectly()
    {
        // Arrange
        var metrics = new V2ApiMetrics();

        // Act
        metrics.RecordSuccess(100);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(metrics.TotalRequests, Is.EqualTo(1));
            Assert.That(metrics.SuccessfulRequests, Is.EqualTo(1));
            Assert.That(metrics.FailedRequests, Is.EqualTo(0));
            Assert.That(metrics.SuccessRate, Is.EqualTo(100));
            Assert.That(metrics.AverageLatencyMs, Is.EqualTo(100));
        });
    }

    [Test]
    public void Metrics_RecordFailure_IncrementsCorrectly()
    {
        // Arrange
        var metrics = new V2ApiMetrics();

        // Act
        metrics.RecordFailure(200);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(metrics.TotalRequests, Is.EqualTo(1));
            Assert.That(metrics.SuccessfulRequests, Is.EqualTo(0));
            Assert.That(metrics.FailedRequests, Is.EqualTo(1));
            Assert.That(metrics.ErrorRate, Is.EqualTo(100));
            Assert.That(metrics.AverageLatencyMs, Is.EqualTo(200));
        });
    }

    [Test]
    public void Metrics_MixedResults_CalculatesRatesCorrectly()
    {
        // Arrange
        var metrics = new V2ApiMetrics();

        // Act - 7 successes, 3 failures
        metrics.RecordSuccess(100);
        metrics.RecordSuccess(150);
        metrics.RecordSuccess(200);
        metrics.RecordSuccess(120);
        metrics.RecordSuccess(180);
        metrics.RecordSuccess(90);
        metrics.RecordSuccess(110);
        metrics.RecordFailure(500);
        metrics.RecordFailure(600);
        metrics.RecordFailure(400);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(metrics.TotalRequests, Is.EqualTo(10));
            Assert.That(metrics.SuccessfulRequests, Is.EqualTo(7));
            Assert.That(metrics.FailedRequests, Is.EqualTo(3));
            Assert.That(metrics.SuccessRate, Is.EqualTo(70));
            Assert.That(metrics.ErrorRate, Is.EqualTo(30));

            // Total latency: 100+150+200+120+180+90+110+500+600+400 = 2450
            // Average: 2450 / 10 = 245
            Assert.That(metrics.AverageLatencyMs, Is.EqualTo(245));
        });
    }

    [Test]
    public void Metrics_Reset_ClearsAllValues()
    {
        // Arrange
        var metrics = new V2ApiMetrics();
        metrics.RecordSuccess(100);
        metrics.RecordFailure(200);

        // Act
        metrics.Reset();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(metrics.TotalRequests, Is.EqualTo(0));
            Assert.That(metrics.SuccessfulRequests, Is.EqualTo(0));
            Assert.That(metrics.FailedRequests, Is.EqualTo(0));
        });
    }

    [Test]
    public void Metrics_ThreadSafety_ConcurrentUpdates()
    {
        // Arrange
        var metrics = new V2ApiMetrics();
        var tasks = new List<Task>();
        const int iterations = 1000;

        // Act - simulate concurrent requests
        for (int i = 0; i < iterations; i++)
        {
            tasks.Add(Task.Run(() => metrics.RecordSuccess(10)));
            tasks.Add(Task.Run(() => metrics.RecordFailure(20)));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(metrics.TotalRequests, Is.EqualTo(iterations * 2));
            Assert.That(metrics.SuccessfulRequests, Is.EqualTo(iterations));
            Assert.That(metrics.FailedRequests, Is.EqualTo(iterations));
            Assert.That(metrics.SuccessRate, Is.EqualTo(50));
        });
    }

    #endregion

    #region DebugApiLogger Tests

    [Test]
    public void DebugApiLogger_LogResponse_UpdatesMetrics()
    {
        // Arrange
        var metrics = new V2ApiMetrics();
        var logger = new DebugApiLogger(metrics, enabled: true);

        // Act
        logger.LogResponse("GET", "https://api.example.com/test", 200, 150, true);
        logger.LogResponse("GET", "https://api.example.com/test", 500, 250, false);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(logger.Metrics.TotalRequests, Is.EqualTo(2));
            Assert.That(logger.Metrics.SuccessfulRequests, Is.EqualTo(1));
            Assert.That(logger.Metrics.FailedRequests, Is.EqualTo(1));
        });
    }

    [Test]
    public void DebugApiLogger_WhenDisabled_StillUpdatesMetrics()
    {
        // Arrange
        var metrics = new V2ApiMetrics();
        var logger = new DebugApiLogger(metrics, enabled: false);

        // Act
        logger.LogResponse("GET", "https://api.example.com/test", 200, 100, true);

        // Assert - metrics should still be updated even when logging is disabled
        Assert.That(logger.Metrics.TotalRequests, Is.EqualTo(1));
    }

    [Test]
    public void DebugApiLogger_DefaultConstructor_CreatesOwnMetrics()
    {
        // Act
        var logger = new DebugApiLogger();

        // Assert
        Assert.That(logger.Metrics, Is.Not.Null);
    }

    #endregion

    #region NullApiLogger Tests

    [Test]
    public void NullApiLogger_Singleton_IsSameInstance()
    {
        // Act
        var logger1 = NullApiLogger.Instance;
        var logger2 = NullApiLogger.Instance;

        // Assert
        Assert.That(logger2, Is.SameAs(logger1));
    }

    [Test]
    public void NullApiLogger_Methods_DoNotThrow()
    {
        // Arrange
        var logger = NullApiLogger.Instance;

        // Act & Assert - should not throw
        Assert.DoesNotThrow(() =>
        {
            logger.LogRequest("GET", "https://api.example.com/test");
            logger.LogResponse("GET", "https://api.example.com/test", 200, 100, true);
            logger.LogError("GET", "https://api.example.com/test", "Error message", 500);
        });
    }

    #endregion

    #region API Key Redaction Tests

    [Test]
    public void ApiKeyRedaction_InQueryString_IsRedacted()
    {
        // Arrange
        var metrics = new V2ApiMetrics();
        var outputCapture = new List<string>();
        var logger = new TestableDebugApiLogger(metrics, outputCapture);

        // Act
        logger.LogRequest("GET", "https://api.example.com/test?api_key=secret123&other=value");

        // Assert
        Assert.That(outputCapture[0], Does.Contain("***REDACTED***"));
        Assert.That(outputCapture[0], Does.Not.Contain("secret123"));
    }

    [Test]
    public void ApiKeyRedaction_WithTokenParameter_IsRedacted()
    {
        // Arrange
        var metrics = new V2ApiMetrics();
        var outputCapture = new List<string>();
        var logger = new TestableDebugApiLogger(metrics, outputCapture);

        // Act
        logger.LogRequest("GET", "https://api.example.com/test?token=mysecrettoken");

        // Assert
        Assert.That(outputCapture[0], Does.Contain("***REDACTED***"));
        Assert.That(outputCapture[0], Does.Not.Contain("mysecrettoken"));
    }

    [Test]
    public void ApiKeyRedaction_NoSensitiveData_UrlUnchanged()
    {
        // Arrange
        var metrics = new V2ApiMetrics();
        var outputCapture = new List<string>();
        var logger = new TestableDebugApiLogger(metrics, outputCapture);

        // Act
        logger.LogRequest("GET", "https://api.example.com/games/1234?include=system");

        // Assert
        Assert.That(outputCapture[0], Does.Contain("include=system"));
        Assert.That(outputCapture[0], Does.Not.Contain("REDACTED"));
    }

    #endregion

    /// <summary>
    /// Test helper that captures log output instead of writing to Debug.
    /// </summary>
    private class TestableDebugApiLogger : IV2ApiLogger
    {
        private readonly V2ApiMetrics _metrics;
        private readonly List<string> _output;

        public TestableDebugApiLogger(V2ApiMetrics metrics, List<string> output)
        {
            _metrics = metrics;
            _output = output;
        }

        public void LogRequest(string method, string url)
        {
            var redactedUrl = RedactApiKey(url);
            _output.Add($"[V2API] {method} {redactedUrl}");
        }

        public void LogResponse(string method, string url, int statusCode, long latencyMs, bool success)
        {
            if (success) _metrics.RecordSuccess(latencyMs);
            else _metrics.RecordFailure(latencyMs);

            var redactedUrl = RedactApiKey(url);
            _output.Add($"[V2API] {method} {redactedUrl} -> {statusCode} ({latencyMs}ms)");
        }

        public void LogError(string method, string url, string error, int? statusCode = null)
        {
            var redactedUrl = RedactApiKey(url);
            _output.Add($"[V2API] ERROR {method} {redactedUrl} -> {error}");
        }

        private static string RedactApiKey(string url)
        {
            return System.Text.RegularExpressions.Regex.Replace(
                url,
                @"(api[-_]?key|token|secret|password|key)=([^&\s]+)",
                "$1=***REDACTED***",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
    }
}
