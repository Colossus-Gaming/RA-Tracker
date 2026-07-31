namespace RATracker.WPF.Services;

/// <summary>
/// Provides feature flags for controlling API version selection and feature rollout.
/// </summary>
public interface IFeatureFlagService
{
    /// <summary>
    /// Gets whether V2 API should be used for metadata operations (systems, games, users).
    /// </summary>
    bool UseV2ForMetadata { get; }

    /// <summary>
    /// Gets whether V2 API should be used for user progress operations.
    /// </summary>
    /// <remarks>
    /// This is typically false until V2 supports all progress endpoints.
    /// </remarks>
    bool UseV2ForProgress { get; }

    /// <summary>
    /// Gets whether V2 API should be used for user summary/rank lookups.
    /// </summary>
    bool UseV2ForUserLookup { get; }

    /// <summary>
    /// Gets whether multi-set achievements are enabled.
    /// </summary>
    bool EnableMultiSet { get; }

    /// <summary>
    /// Gets whether V1 fallback is enabled when V2 calls fail.
    /// </summary>
    bool EnableV1Fallback { get; }

    /// <summary>
    /// Gets whether detailed API logging is enabled.
    /// </summary>
    bool EnableApiLogging { get; }
}

/// <summary>
/// Default implementation of IFeatureFlagService using application settings.
/// </summary>
public class FeatureFlagService : IFeatureFlagService
{
    /// <summary>
    /// Creates a new FeatureFlagService with default values.
    /// All operations use V2 API by default.
    /// </summary>
    public FeatureFlagService()
    {
        // Default settings - V2 API for all operations
        UseV2ForMetadata = true;       // V2 for metadata
        UseV2ForProgress = true;       // V2 for progress operations
        UseV2ForUserLookup = true;     // V2 for user data
        EnableMultiSet = true;         // Multi-set support enabled
        EnableV1Fallback = true;       // Always fall back to V1 on V2 errors
        EnableApiLogging = false;      // Off by default
    }

    /// <summary>
    /// Creates a new FeatureFlagService with custom settings.
    /// </summary>
    public FeatureFlagService(
        bool useV2ForMetadata = true,
        bool useV2ForProgress = true,
        bool useV2ForUserLookup = true,
        bool enableMultiSet = true,
        bool enableV1Fallback = true,
        bool enableApiLogging = false)
    {
        UseV2ForMetadata = useV2ForMetadata;
        UseV2ForProgress = useV2ForProgress;
        UseV2ForUserLookup = useV2ForUserLookup;
        EnableMultiSet = enableMultiSet;
        EnableV1Fallback = enableV1Fallback;
        EnableApiLogging = enableApiLogging;
    }

    /// <inheritdoc />
    public bool UseV2ForMetadata { get; set; }

    /// <inheritdoc />
    public bool UseV2ForProgress { get; set; }

    /// <inheritdoc />
    public bool UseV2ForUserLookup { get; set; }

    /// <inheritdoc />
    public bool EnableMultiSet { get; set; }

    /// <inheritdoc />
    public bool EnableV1Fallback { get; set; }

    /// <inheritdoc />
    public bool EnableApiLogging { get; set; }
}
