using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using RATracker.WPF.Models;

namespace RATracker.WPF.Services;

/// <summary>
/// Service for persisting and loading application settings.
/// Uses JSON file storage in %AppData%\RATracker\settings.json.
/// Provides basic encryption for sensitive data like API keys.
/// </summary>
public class SettingsService
{
    private static readonly Lazy<SettingsService> _instance = new(() => new SettingsService());
    
    private readonly string _settingsDirectory;
    private readonly string _settingsFilePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private AppSettings _currentSettings;
    private Timer? _saveDebounceTimer;
    private readonly object _saveLock = new();
    private bool _isDirty;

    /// <summary>
    /// Gets the singleton instance of the SettingsService.
    /// </summary>
    public static SettingsService Instance => _instance.Value;

    /// <summary>
    /// Creates a new SettingsService instance.
    /// </summary>
    private SettingsService()
    {
        _settingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RATracker");
        _settingsFilePath = Path.Combine(_settingsDirectory, "settings.json");

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        _currentSettings = LoadSettings();
    }

    /// <summary>
    /// Gets the current settings.
    /// </summary>
    public AppSettings Settings => _currentSettings;

    /// <summary>
    /// Loads settings from the JSON file, creating defaults if the file doesn't exist.
    /// </summary>
    /// <returns>The loaded or default settings.</returns>
    private AppSettings LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = File.ReadAllText(_settingsFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions);
                if (settings != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[Settings] Loaded settings from {_settingsFilePath}");
                    return settings;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Settings] Error loading settings: {ex.Message}");
        }

        System.Diagnostics.Debug.WriteLine("[Settings] Using default settings");
        return new AppSettings();
    }

    /// <summary>
    /// Saves settings to the JSON file.
    /// </summary>
    public void SaveSettings()
    {
        lock (_saveLock)
        {
            try
            {
                // Ensure directory exists
                if (!Directory.Exists(_settingsDirectory))
                {
                    Directory.CreateDirectory(_settingsDirectory);
                }

                var json = JsonSerializer.Serialize(_currentSettings, _jsonOptions);
                File.WriteAllText(_settingsFilePath, json);
                _isDirty = false;
                System.Diagnostics.Debug.WriteLine($"[Settings] Saved settings to {_settingsFilePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Settings] Error saving settings: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Marks settings as dirty and schedules a debounced save.
    /// This prevents excessive disk writes when multiple properties change rapidly.
    /// </summary>
    public void ScheduleSave()
    {
        _isDirty = true;

        // Cancel previous timer
        _saveDebounceTimer?.Dispose();

        // Schedule save after 500ms of inactivity
        _saveDebounceTimer = new Timer(
            _ => SaveSettings(),
            null,
            TimeSpan.FromMilliseconds(500),
            Timeout.InfiniteTimeSpan);
    }

    /// <summary>
    /// Forces an immediate save if there are pending changes.
    /// Call this on application shutdown.
    /// </summary>
    public void Flush()
    {
        _saveDebounceTimer?.Dispose();
        _saveDebounceTimer = null;

        if (_isDirty)
        {
            SaveSettings();
        }
    }

    #region API Key Encryption

    // Secret protection is delegated to CredentialProtection.Current: DPAPI on Windows (unchanged
    // behaviour and entropy, so keys saved by earlier releases still decrypt), and an encoding-only
    // fallback elsewhere so the shared core can run on macOS and Linux. Check
    // CredentialProtection.Current.IsEncrypted before implying to a user that a secret is secure.

    /// <summary>
    /// Protects an API key for storage.
    /// </summary>
    /// <param name="apiKey">The plain text API key.</param>
    /// <returns>A Base64-encoded string, or empty if input is empty.</returns>
    public static string EncryptApiKey(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
            return string.Empty;

        try
        {
            return CredentialProtection.Current.Protect(apiKey);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Settings] Error encrypting API key: {ex.Message}");
            // Fall back to plain storage (not ideal but functional)
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(apiKey));
        }
    }

    /// <summary>
    /// Reads a stored API key back.
    /// </summary>
    /// <param name="encryptedKey">The Base64-encoded stored string.</param>
    /// <returns>The plain text API key, or empty if input is empty or unreadable.</returns>
    public static string DecryptApiKey(string encryptedKey)
    {
        if (string.IsNullOrEmpty(encryptedKey))
            return string.Empty;

        try
        {
            return CredentialProtection.Current.Unprotect(encryptedKey);
        }
        catch (CryptographicException)
        {
            // May be plain text from a fallback path, or an older format.
            //
            // A DPAPI blob is itself valid Base64, so this path also catches genuinely
            // undecryptable data - e.g. settings.json copied to another PC, or a Windows
            // password reset that invalidated the master key. Decoding that yields ~60
            // characters of mojibake, and returning it would leave the caller unable to
            // tell a corrupt key from a real one: the app would look signed in and fail
            // every API call. Only hand back something that could actually be a key.
            try
            {
                var bytes = Convert.FromBase64String(encryptedKey);
                var text = Encoding.UTF8.GetString(bytes);
                return LooksLikeApiKey(text) ? text : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Settings] Error decrypting API key: {ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// Whether a decoded string is plausibly a RetroAchievements API key rather than the
    /// mojibake you get from Base64-decoding an undecryptable DPAPI blob.
    /// </summary>
    /// <remarks>
    /// RA keys are short printable ASCII. Deliberately permissive about which characters
    /// are allowed - the point is only to reject binary garbage, not to validate the key,
    /// which the API itself does.
    /// </remarks>
    private static bool LooksLikeApiKey(string text) =>
        text.Length is > 0 and <= 128 && text.All(c => c is >= ' ' and <= '~');

    #endregion

    #region Convenience Methods

    /// <summary>
    /// Gets the decrypted API key from settings.
    /// </summary>
    public string GetApiKey()
    {
        return DecryptApiKey(_currentSettings.EncryptedApiKey);
    }

    /// <summary>
    /// Sets and encrypts the API key in settings.
    /// </summary>
    /// <param name="apiKey">The plain text API key.</param>
    public void SetApiKey(string apiKey)
    {
        _currentSettings.EncryptedApiKey = EncryptApiKey(apiKey);
        ScheduleSave();
    }

    /// <summary>
    /// Gets the decrypted password from settings.
    /// Returns empty string if password is not saved.
    /// </summary>
    public string GetPassword()
    {
        if (!_currentSettings.RememberPassword || string.IsNullOrEmpty(_currentSettings.EncryptedPassword))
            return string.Empty;
        return DecryptApiKey(_currentSettings.EncryptedPassword);
    }

    /// <summary>
    /// Sets and encrypts the password in settings.
    /// </summary>
    /// <param name="password">The plain text password.</param>
    /// <param name="remember">Whether to persist the encrypted password.</param>
    public void SetPassword(string password, bool remember)
    {
        _currentSettings.RememberPassword = remember;
        _currentSettings.EncryptedPassword = remember && !string.IsNullOrEmpty(password)
            ? EncryptApiKey(password)
            : string.Empty;
        ScheduleSave();
    }

    /// <summary>
    /// Clears the stored password.
    /// </summary>
    public void ClearPassword()
    {
        _currentSettings.EncryptedPassword = string.Empty;
        _currentSettings.RememberPassword = false;
        ScheduleSave();
    }

    /// <summary>
    /// Updates a setting value and schedules a save.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="setter">Action to set the property.</param>
    /// <param name="value">The new value.</param>
    public void UpdateSetting<T>(Action<AppSettings, T> setter, T value)
    {
        setter(_currentSettings, value);
        ScheduleSave();
    }

    #endregion
}
