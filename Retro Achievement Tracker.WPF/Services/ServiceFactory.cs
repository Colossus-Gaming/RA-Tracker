using RATracker.WPF.Http.V2;

namespace RATracker.WPF.Services;

/// <summary>
/// Factory for creating service instances based on feature flags.
/// </summary>
public class ServiceFactory : IDisposable
{
    private readonly string _apiKey;
    private readonly string _username;
    private readonly IFeatureFlagService _featureFlags;
    private readonly IV2ApiLogger? _logger;
    private readonly V2ApiMetrics _metrics;

    private IMetadataService? _metadataService;
    private IAchievementProgressProvider? _progressProvider;
    private IProgressService? _progressService;
    private AchievementTrackingService? _trackingService;
    private bool _disposed;

    /// <summary>
    /// Creates a new ServiceFactory.
    /// </summary>
    /// <param name="username">The authenticated username.</param>
    /// <param name="apiKey">The RetroAchievements API key.</param>
    /// <param name="featureFlags">Optional feature flag service (uses defaults if not provided).</param>
    public ServiceFactory(string username, string apiKey, IFeatureFlagService? featureFlags = null)
        : this(username, apiKey, featureFlags, null)
    {
    }

    /// <summary>
    /// Creates a new ServiceFactory with logging support.
    /// </summary>
    /// <param name="username">The authenticated username.</param>
    /// <param name="apiKey">The RetroAchievements API key.</param>
    /// <param name="featureFlags">Optional feature flag service (uses defaults if not provided).</param>
    /// <param name="enableLogging">Whether to enable API request logging.</param>
    public ServiceFactory(string username, string apiKey, IFeatureFlagService? featureFlags, bool? enableLogging)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required", nameof(username));
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key is required", nameof(apiKey));

        _username = username;
        _apiKey = apiKey;
        _featureFlags = featureFlags ?? new FeatureFlagService();
        _metrics = new V2ApiMetrics();

        // Create logger based on feature flag or explicit setting
        var loggingEnabled = enableLogging ?? _featureFlags.EnableApiLogging;
        _logger = loggingEnabled ? new DebugApiLogger(_metrics, true) : null;
    }

    /// <summary>
    /// Gets the feature flag service.
    /// </summary>
    public IFeatureFlagService FeatureFlags => _featureFlags;

    /// <summary>
    /// Gets the API metrics for observability.
    /// </summary>
    public V2ApiMetrics Metrics => _metrics;

    /// <summary>
    /// Gets the metadata service based on current feature flags.
    /// </summary>
    /// <remarks>
    /// Uses V2 API when UseV2ForMetadata is enabled, otherwise throws NotSupportedException
    /// as there's no V1 metadata service implementation yet.
    /// </remarks>
    public IMetadataService GetMetadataService()
    {
        ThrowIfDisposed();

        if (_metadataService == null)
        {
            if (_featureFlags.UseV2ForMetadata)
            {
                _metadataService = new V2MetadataService(_apiKey, _logger);
            }
            else
            {
                // V1 metadata service could be implemented if needed
                throw new NotSupportedException("V1 metadata service is not implemented. Enable UseV2ForMetadata.");
            }
        }

        return _metadataService;
    }

    /// <summary>
    /// Gets the achievement progress provider based on current feature flags.
    /// </summary>
    public IAchievementProgressProvider GetProgressProvider()
    {
        ThrowIfDisposed();

        if (_progressProvider == null)
        {
            if (_featureFlags.UseV2ForProgress)
            {
                // Use V2 with V1 fallback
                _progressProvider = new V2AchievementProgressProvider(
                    _apiKey, 
                    _username, 
                    _featureFlags.EnableV1Fallback);
            }
            else
            {
                // Use V1 directly
                _progressProvider = new V1AchievementProgressProvider(_username, _apiKey);
            }
        }

        return _progressProvider;
    }

    /// <summary>
    /// Gets the progress service based on current feature flags.
    /// </summary>
    /// <remarks>
    /// Uses HybridProgressService which handles V1/V2 selection internally based on feature flags.
    /// </remarks>
    public IProgressService GetProgressService()
    {
        ThrowIfDisposed();

        if (_progressService == null)
        {
            IProgressServiceLogger progressLogger = _featureFlags.EnableApiLogging 
                ? DebugProgressServiceLogger.Instance 
                : NullProgressServiceLogger.Instance;

            _progressService = new HybridProgressService(
                _username, 
                _apiKey, 
                _featureFlags, 
                progressLogger,
                _logger);
        }

        return _progressService;
    }

    /// <summary>
    /// Gets the achievement tracking service for polling and unlock detection.
    /// </summary>
    /// <remarks>
    /// Creates a new AchievementTrackingService instance that uses the progress service
    /// for API calls and ProgressStateTracker for unlock detection.
    /// </remarks>
    public AchievementTrackingService GetTrackingService()
    {
        ThrowIfDisposed();

        if (_trackingService == null)
        {
            _trackingService = new AchievementTrackingService(GetProgressService(), _username);
        }

        return _trackingService;
    }

    /// <summary>
    /// Resets cached service instances, forcing recreation on next access.
    /// Useful when feature flags change at runtime.
    /// </summary>
    public void ResetServices()
    {
        ThrowIfDisposed();

        if (_metadataService is IDisposable metadataDisposable)
        {
            metadataDisposable.Dispose();
        }
        _metadataService = null;

        if (_progressProvider is IDisposable progressDisposable)
        {
            progressDisposable.Dispose();
        }
        _progressProvider = null;

        if (_progressService is IDisposable progressServiceDisposable)
        {
            progressServiceDisposable.Dispose();
        }
        _progressService = null;

        _trackingService?.Dispose();
        _trackingService = null;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ServiceFactory));
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            ResetServices();
            _disposed = true;
        }
    }
}
