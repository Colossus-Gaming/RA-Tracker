namespace RATracker.WPF.Services;

/// <summary>
/// Reads RetroAchievements credentials from environment variables.
/// Intended for development and automated testing so credentials can be supplied
/// without going through the settings UI. When a variable is present (non-empty),
/// it takes precedence over the value stored in the settings file.
/// </summary>
public static class EnvironmentCredentials
{
    /// <summary>Environment variable holding the RetroAchievements username.</summary>
    public const string UsernameVariable = "RA_USERNAME";

    /// <summary>Environment variable holding the RetroAchievements Web API key.</summary>
    public const string ApiKeyVariable = "RA_API_KEY";

    /// <summary>Environment variable holding the RetroAchievements account password (for session login).</summary>
    public const string PasswordVariable = "RA_PASSWORD";

    /// <summary>Gets the username from the environment, or null if unset/blank.</summary>
    public static string? GetUsername() => Normalize(Environment.GetEnvironmentVariable(UsernameVariable));

    /// <summary>Gets the Web API key from the environment, or null if unset/blank.</summary>
    public static string? GetApiKey() => Normalize(Environment.GetEnvironmentVariable(ApiKeyVariable));

    /// <summary>Gets the password from the environment, or null if unset/blank.</summary>
    public static string? GetPassword() => Normalize(Environment.GetEnvironmentVariable(PasswordVariable));

    /// <summary>Whether any credential environment variable is set.</summary>
    public static bool HasAny => GetUsername() != null || GetApiKey() != null || GetPassword() != null;

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
