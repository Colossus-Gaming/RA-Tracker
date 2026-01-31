# Settings Migration Prompt: JSON-Based Settings with Secure API Key Storage

## Objective

Migrate the Retro Achievements Layout Manager from `Properties.Settings.Default` (WinForms app.config-based) to a JSON file-based settings system with the following requirements:

1. **JSON File Storage**: Store all user settings in `settings.json` in the application directory
2. **Secure API Key**: Encrypt the Web API key using Windows DPAPI (Data Protection API)
3. **Backwards Compatibility**: Migrate existing settings from `Properties.Settings.Default` on first run
4. **Primary Source**: Use `settings.json` as the primary source when it exists

---

## Implementation Requirements

### 1. Create Settings Model Classes

Create a `UserSettings` class that mirrors all settings from `Properties.Settings.Default`:

```csharp
// File: Retro Achievement Tracker\Models\UserSettings.cs

using System;

namespace Retro_Achievement_Tracker.Models
{
    /// <summary>
    /// Represents all user-configurable settings for the application.
    /// This class is serialized to/from settings.json.
    /// </summary>
    public class UserSettings
    {
        // Schema version for future migrations
        public int SchemaVersion { get; set; } = 1;
        
        // Credentials (API key is stored encrypted)
        public CredentialsSettings Credentials { get; set; } = new CredentialsSettings();
        
        // Auto-launch settings
        public AutoLaunchSettings AutoLaunch { get; set; } = new AutoLaunchSettings();
        
        // Focus window settings
        public FocusSettings Focus { get; set; } = new FocusSettings();
        
        // Alerts window settings
        public AlertsSettings Alerts { get; set; } = new AlertsSettings();
        
        // User info window settings
        public UserInfoSettings UserInfo { get; set; } = new UserInfoSettings();
        
        // Game info window settings
        public GameInfoSettings GameInfo { get; set; } = new GameInfoSettings();
        
        // Game progress/stats settings
        public GameProgressSettings GameProgress { get; set; } = new GameProgressSettings();
        
        // Recent achievements settings
        public RecentAchievementsSettings RecentAchievements { get; set; } = new RecentAchievementsSettings();
        
        // Achievement list settings
        public AchievementListSettings AchievementList { get; set; } = new AchievementListSettings();
        
        // Related media settings
        public RelatedMediaSettings RelatedMedia { get; set; } = new RelatedMediaSettings();
        
        // Update settings
        public UpdateSettings Updates { get; set; } = new UpdateSettings();
    }

    public class CredentialsSettings
    {
        public string Username { get; set; } = "";
        
        /// <summary>
        /// Base64-encoded DPAPI-encrypted API key.
        /// Never store the raw key in this field.
        /// </summary>
        public string EncryptedApiKey { get; set; } = "";
        
        public int PreviouslyPlayedGameId { get; set; } = 0;
    }

    public class AutoLaunchSettings
    {
        public bool AutoStart { get; set; } = false;
        public bool Focus { get; set; } = false;
        public bool Alerts { get; set; } = false;
        public bool UserInfo { get; set; } = false;
        public bool GameInfo { get; set; } = false;
        public bool GameProgress { get; set; } = false;
        public bool RecentAchievements { get; set; } = false;
        public bool AchievementList { get; set; } = false;
        public bool RelatedMedia { get; set; } = false;
    }

    public class FontSettings
    {
        public string FontFamily { get; set; } = "Calibri";
        public string Color { get; set; } = "#000000";
        public bool OutlineEnabled { get; set; } = false;
        public string OutlineColor { get; set; } = "#FFFFFF";
        public int OutlineSize { get; set; } = 2;
    }

    public class AdvancedFontSettings
    {
        public FontSettings Title { get; set; } = new FontSettings();
        public FontSettings Description { get; set; } = new FontSettings();
        public FontSettings Points { get; set; } = new FontSettings();
        public FontSettings Line { get; set; } = new FontSettings();
    }

    public class WindowAppearanceSettings
    {
        public string WindowBackgroundColor { get; set; } = "#FF00FF";
        public string BorderBackgroundColor { get; set; } = "#FF00FF";
        public bool BorderEnabled { get; set; } = true;
        public bool AdvancedOptionsEnabled { get; set; } = false;
        public FontSettings SimpleFont { get; set; } = new FontSettings();
        public AdvancedFontSettings AdvancedFont { get; set; } = new AdvancedFontSettings();
    }

    public class FocusSettings : WindowAppearanceSettings
    {
        public string RefocusBehavior { get; set; } = "GO_TO_FIRST";
    }

    public class CustomNotificationSettings
    {
        public bool Enabled { get; set; } = false;
        public string FilePath { get; set; } = "";
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public decimal Scale { get; set; } = 1.00m;
        public int FadeInTime { get; set; } = 300;
        public int FadeOutTime { get; set; } = 300;
        public int InSpeed { get; set; } = 500;
        public int OutSpeed { get; set; } = 500;
        public string AnimationIn { get; set; } = "STATIC";
        public string AnimationOut { get; set; } = "UP";
    }

    public class AlertsSettings : WindowAppearanceSettings
    {
        public bool AchievementAlertEnabled { get; set; } = true;
        public bool MasteryAlertEnabled { get; set; } = true;
        public CustomNotificationSettings CustomAchievement { get; set; } = new CustomNotificationSettings();
        public CustomNotificationSettings CustomMastery { get; set; } = new CustomNotificationSettings();
    }

    public class NameValueFontSettings
    {
        public FontSettings Names { get; set; } = new FontSettings();
        public FontSettings Values { get; set; } = new FontSettings();
    }

    public class UserInfoSettings
    {
        public string WindowBackgroundColor { get; set; } = "#FF00FF";
        public bool AdvancedOptionsEnabled { get; set; } = false;
        public FontSettings SimpleFont { get; set; } = new FontSettings();
        public NameValueFontSettings AdvancedFont { get; set; } = new NameValueFontSettings();
        
        // Field visibility
        public bool RankEnabled { get; set; } = true;
        public bool PointsEnabled { get; set; } = true;
        public bool TruePointsEnabled { get; set; } = true;
        public bool RatioEnabled { get; set; } = true;
        
        // Custom field names
        public string RankName { get; set; } = "Rank";
        public string PointsName { get; set; } = "Points";
        public string TruePointsName { get; set; } = "True Points";
        public string RatioName { get; set; } = "Ratio";
    }

    public class GameInfoSettings
    {
        public string WindowBackgroundColor { get; set; } = "#FF00FF";
        public bool AdvancedOptionsEnabled { get; set; } = false;
        public FontSettings SimpleFont { get; set; } = new FontSettings();
        public NameValueFontSettings AdvancedFont { get; set; } = new NameValueFontSettings();
        
        // Field visibility
        public bool TitleEnabled { get; set; } = true;
        public bool ConsoleEnabled { get; set; } = true;
        public bool DeveloperEnabled { get; set; } = true;
        public bool PublisherEnabled { get; set; } = true;
        public bool GenreEnabled { get; set; } = true;
        public bool ReleaseDateEnabled { get; set; } = true;
        
        // Custom field names
        public string TitleName { get; set; } = "Title";
        public string ConsoleName { get; set; } = "Console";
        public string DeveloperName { get; set; } = "Developer";
        public string PublisherName { get; set; } = "Publisher";
        public string GenreName { get; set; } = "Genre";
        public string ReleaseDateName { get; set; } = "Released";
    }

    public class GameProgressSettings
    {
        public string WindowBackgroundColor { get; set; } = "#FF00FF";
        public bool AdvancedOptionsEnabled { get; set; } = false;
        public FontSettings SimpleFont { get; set; } = new FontSettings();
        public NameValueFontSettings AdvancedFont { get; set; } = new NameValueFontSettings();
        
        // Field visibility
        public bool AchievementsEnabled { get; set; } = true;
        public bool PointsEnabled { get; set; } = true;
        public bool TruePointsEnabled { get; set; } = true;
        public bool RatioEnabled { get; set; } = true;
        public bool CompletedEnabled { get; set; } = true;
        
        // Custom field names
        public string AchievementsName { get; set; } = "Achievements";
        public string PointsName { get; set; } = "Points";
        public string TruePointsName { get; set; } = "True Points";
        public string RatioName { get; set; } = "Ratio";
        public string CompletedName { get; set; } = "Completed";
        
        public string DividerCharacter { get; set; } = "/";
    }

    public class RecentAchievementsSettings : WindowAppearanceSettings
    {
        public int MaxListSize { get; set; } = 5;
        public bool AutoScroll { get; set; } = true;
    }

    public class AchievementListSettings
    {
        public string WindowBackgroundColor { get; set; } = "#FF00FF";
        public string BorderColor { get; set; } = "#CC9900";
        public int WindowSizeX { get; set; } = 748;
        public int WindowSizeY { get; set; } = 612;
        public bool AutoScroll { get; set; } = true;
    }

    public class RelatedMediaSettings
    {
        public string WindowBackgroundColor { get; set; } = "#FF00FF";
        public string LaunchBoxFilePath { get; set; } = "";
        public string MediaSelection { get; set; } = "BADGE_ICON";
    }

    public class UpdateSettings
    {
        public bool CheckForUpdateOnVersion { get; set; } = true;
        public string LastCheckedVersion { get; set; } = "1.0.0.0";
    }
}
```

### 2. Create Secure Credential Helper

```csharp
// File: Retro Achievement Tracker\Services\CredentialProtector.cs

using System;
using System.Security.Cryptography;
using System.Text;

namespace Retro_Achievement_Tracker.Services
{
    /// <summary>
    /// Provides secure encryption/decryption of sensitive data using Windows DPAPI.
    /// Data encrypted with this class can only be decrypted by the same Windows user.
    /// </summary>
    public static class CredentialProtector
    {
        /// <summary>
        /// Encrypts a string using Windows DPAPI with user-scope protection.
        /// </summary>
        /// <param name="plainText">The plain text to encrypt.</param>
        /// <returns>Base64-encoded encrypted data, or empty string if input is null/empty.</returns>
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            try
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedBytes = ProtectedData.Protect(
                    plainBytes, 
                    null, 
                    DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encryptedBytes);
            }
            catch (CryptographicException)
            {
                // If encryption fails, return empty string
                // This can happen in rare cases with Windows configuration issues
                return string.Empty;
            }
        }

        /// <summary>
        /// Decrypts a Base64-encoded DPAPI-encrypted string.
        /// </summary>
        /// <param name="encryptedText">Base64-encoded encrypted data.</param>
        /// <returns>Decrypted plain text, or empty string if decryption fails.</returns>
        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;

            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                byte[] plainBytes = ProtectedData.Unprotect(
                    encryptedBytes, 
                    null, 
                    DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch (CryptographicException)
            {
                // Decryption failed - could be different user or corrupted data
                return string.Empty;
            }
            catch (FormatException)
            {
                // Invalid Base64 - data is corrupted
                return string.Empty;
            }
        }

        /// <summary>
        /// Checks if a string appears to be an encrypted API key (Base64 format).
        /// </summary>
        public static bool IsEncrypted(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            try
            {
                // Try to decode as Base64 - encrypted values are always Base64
                Convert.FromBase64String(value);
                // API keys are typically short (32 chars), encrypted versions are longer
                return value.Length > 50;
            }
            catch
            {
                return false;
            }
        }
    }
}
```

### 3. Create Settings Service

```csharp
// File: Retro Achievement Tracker\Services\SettingsService.cs

using Newtonsoft.Json;
using Retro_Achievement_Tracker.Models;
using Retro_Achievement_Tracker.Properties;
using System;
using System.IO;

namespace Retro_Achievement_Tracker.Services
{
    /// <summary>
    /// Manages loading, saving, and migrating user settings.
    /// Provides backwards compatibility with Properties.Settings.Default.
    /// </summary>
    public class SettingsService
    {
        private static readonly Lazy<SettingsService> _instance = 
            new Lazy<SettingsService>(() => new SettingsService());
        
        public static SettingsService Instance => _instance.Value;

        private const string SETTINGS_FILENAME = "settings.json";
        private readonly string _settingsPath;
        private UserSettings _settings;
        private bool _isDirty;

        public UserSettings Current => _settings;

        private SettingsService()
        {
            _settingsPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, 
                SETTINGS_FILENAME);
            
            LoadOrMigrate();
        }

        /// <summary>
        /// Loads settings from JSON file, or migrates from Properties.Settings if no JSON exists.
        /// </summary>
        private void LoadOrMigrate()
        {
            if (File.Exists(_settingsPath))
            {
                // Primary path: Load from JSON file
                LoadFromJson();
            }
            else
            {
                // Backwards compatibility: Migrate from Properties.Settings
                MigrateFromLegacySettings();
            }
        }

        /// <summary>
        /// Loads settings from the JSON file.
        /// </summary>
        private void LoadFromJson()
        {
            try
            {
                string json = File.ReadAllText(_settingsPath);
                _settings = JsonConvert.DeserializeObject<UserSettings>(json) ?? new UserSettings();
                
                // Handle schema migrations for future versions
                if (_settings.SchemaVersion < 1)
                {
                    // Future: Add migration logic for schema upgrades
                    _settings.SchemaVersion = 1;
                    _isDirty = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
                _settings = new UserSettings();
                _isDirty = true;
            }
        }

        /// <summary>
        /// Migrates all settings from Properties.Settings.Default to the new JSON format.
        /// </summary>
        private void MigrateFromLegacySettings()
        {
            _settings = new UserSettings();
            
            try
            {
                var legacy = Settings.Default;

                // Credentials (encrypt the API key)
                _settings.Credentials.Username = legacy.ra_username ?? "";
                if (!string.IsNullOrEmpty(legacy.ra_key))
                {
                    _settings.Credentials.EncryptedApiKey = CredentialProtector.Encrypt(legacy.ra_key);
                }
                _settings.Credentials.PreviouslyPlayedGameId = legacy.previously_played_game;

                // Auto-launch settings
                _settings.AutoLaunch.AutoStart = legacy.auto_start_checked;
                _settings.AutoLaunch.Focus = legacy.auto_focus;
                _settings.AutoLaunch.Alerts = legacy.auto_notifications;
                _settings.AutoLaunch.UserInfo = legacy.auto_stats;
                _settings.AutoLaunch.GameInfo = legacy.auto_game_info;
                _settings.AutoLaunch.GameProgress = legacy.auto_game_stats;
                _settings.AutoLaunch.RecentAchievements = legacy.auto_last_five;
                _settings.AutoLaunch.AchievementList = legacy.auto_achievement_list;
                _settings.AutoLaunch.RelatedMedia = legacy.auto_related_media;

                // Focus settings
                MigrateFocusSettings(legacy);

                // Alerts settings
                MigrateAlertsSettings(legacy);

                // User info settings
                MigrateUserInfoSettings(legacy);

                // Game info settings
                MigrateGameInfoSettings(legacy);

                // Game progress settings
                MigrateGameProgressSettings(legacy);

                // Recent achievements settings
                MigrateRecentAchievementsSettings(legacy);

                // Achievement list settings
                MigrateAchievementListSettings(legacy);

                // Related media settings
                MigrateRelatedMediaSettings(legacy);

                // Update settings
                _settings.Updates.CheckForUpdateOnVersion = legacy.check_for_update_on_version;
                _settings.Updates.LastCheckedVersion = legacy.check_for_update_version ?? "1.0.0.0";

                // Save the migrated settings
                Save();

                Console.WriteLine("Successfully migrated settings from legacy format.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error migrating settings: {ex.Message}");
                _settings = new UserSettings();
            }
        }

        private void MigrateFocusSettings(Settings legacy)
        {
            _settings.Focus.WindowBackgroundColor = legacy.focus_window_background_color ?? "#FF00FF";
            _settings.Focus.BorderBackgroundColor = legacy.focus_background_color ?? "#FF00FF";
            _settings.Focus.BorderEnabled = legacy.focus_border_enable;
            _settings.Focus.AdvancedOptionsEnabled = legacy.focus_advanced_options_enabled;
            _settings.Focus.RefocusBehavior = legacy.focus_refocus_behavior ?? "GO_TO_FIRST";

            // Simple font
            _settings.Focus.SimpleFont.FontFamily = legacy.focus_font_family_name ?? "Calibri";
            _settings.Focus.SimpleFont.Color = legacy.focus_font_color_hex_code ?? "#000000";
            _settings.Focus.SimpleFont.OutlineEnabled = legacy.focus_font_outline_enabled;
            _settings.Focus.SimpleFont.OutlineColor = legacy.focus_font_outline_color_hex ?? "#FFFFFF";
            _settings.Focus.SimpleFont.OutlineSize = legacy.focus_font_outline_size;

            // Advanced fonts
            _settings.Focus.AdvancedFont.Title.FontFamily = legacy.focus_title_font_family ?? "Calibri";
            _settings.Focus.AdvancedFont.Title.Color = legacy.focus_title_color ?? "#FFFFFF";
            _settings.Focus.AdvancedFont.Title.OutlineEnabled = legacy.focus_title_outline_enabled;
            _settings.Focus.AdvancedFont.Title.OutlineColor = legacy.focus_title_outline_color ?? "#FFFFFF";
            _settings.Focus.AdvancedFont.Title.OutlineSize = legacy.focus_title_outline_size;

            _settings.Focus.AdvancedFont.Description.FontFamily = legacy.focus_description_font_family ?? "Calibri";
            _settings.Focus.AdvancedFont.Description.Color = legacy.focus_description_color ?? "#FFFFFF";
            _settings.Focus.AdvancedFont.Description.OutlineEnabled = legacy.focus_description_outline_enabled;
            _settings.Focus.AdvancedFont.Description.OutlineColor = legacy.focus_description_outline_color ?? "#FF00FF";
            _settings.Focus.AdvancedFont.Description.OutlineSize = legacy.focus_description_outline_size;

            _settings.Focus.AdvancedFont.Points.FontFamily = legacy.focus_points_font_family ?? "Calibri";
            _settings.Focus.AdvancedFont.Points.Color = legacy.focus_points_color ?? "#FFFFFF";
            _settings.Focus.AdvancedFont.Points.OutlineEnabled = legacy.focus_points_outline_enabled;
            _settings.Focus.AdvancedFont.Points.OutlineColor = legacy.focus_points_outline_color ?? "#000000";
            _settings.Focus.AdvancedFont.Points.OutlineSize = legacy.focus_points_outline_size;

            _settings.Focus.AdvancedFont.Line.Color = legacy.focus_line_color ?? "#FFFFFF";
            _settings.Focus.AdvancedFont.Line.OutlineEnabled = legacy.focus_line_outline_enabled;
            _settings.Focus.AdvancedFont.Line.OutlineColor = legacy.focus_line_outline_color ?? "#FF00FF";
            _settings.Focus.AdvancedFont.Line.OutlineSize = legacy.focus_line_outline_size;
        }

        private void MigrateAlertsSettings(Settings legacy)
        {
            _settings.Alerts.WindowBackgroundColor = legacy.alerts_window_background_color ?? "#FF00FF";
            _settings.Alerts.BorderBackgroundColor = legacy.notifications_background_color ?? "#FF00FF";
            _settings.Alerts.BorderEnabled = legacy.notifications_border_enable;
            _settings.Alerts.AdvancedOptionsEnabled = legacy.alerts_advanced_options_enabled;
            _settings.Alerts.AchievementAlertEnabled = legacy.alerts_achievement_enable;
            _settings.Alerts.MasteryAlertEnabled = legacy.alerts_mastery_enable;

            // Simple font
            _settings.Alerts.SimpleFont.FontFamily = legacy.notification_font_family_name ?? "Calibri";
            _settings.Alerts.SimpleFont.Color = legacy.notification_font_color_hex_code ?? "#000000";
            _settings.Alerts.SimpleFont.OutlineEnabled = legacy.notification_font_outline_enabled;
            _settings.Alerts.SimpleFont.OutlineColor = legacy.notification_font_outline_color_hex ?? "#FFFFFF";
            _settings.Alerts.SimpleFont.OutlineSize = legacy.notification_font_outline_size;

            // Advanced fonts
            _settings.Alerts.AdvancedFont.Title.FontFamily = legacy.alerts_title_font_family ?? "Calibri";
            _settings.Alerts.AdvancedFont.Title.Color = legacy.alerts_title_color ?? "#FF00FF";
            _settings.Alerts.AdvancedFont.Title.OutlineEnabled = legacy.alerts_title_outline_enabled;
            _settings.Alerts.AdvancedFont.Title.OutlineColor = legacy.alerts_title_outline_color ?? "#FF00FF";
            _settings.Alerts.AdvancedFont.Title.OutlineSize = legacy.alerts_title_outline_size;

            _settings.Alerts.AdvancedFont.Description.FontFamily = legacy.alerts_description_font_family ?? "Calibri";
            _settings.Alerts.AdvancedFont.Description.Color = legacy.alerts_description_color ?? "#FF00FF";
            _settings.Alerts.AdvancedFont.Description.OutlineEnabled = legacy.alerts_description_outline_enabled;
            _settings.Alerts.AdvancedFont.Description.OutlineColor = legacy.alerts_description_outline_color ?? "#FF00FF";
            _settings.Alerts.AdvancedFont.Description.OutlineSize = legacy.alerts_description_outline_size;

            _settings.Alerts.AdvancedFont.Points.FontFamily = legacy.alerts_points_font_family ?? "Calibri";
            _settings.Alerts.AdvancedFont.Points.Color = legacy.alerts_points_color ?? "#FF00FF";
            _settings.Alerts.AdvancedFont.Points.OutlineEnabled = legacy.alerts_points_outline_enabled;
            _settings.Alerts.AdvancedFont.Points.OutlineColor = legacy.alerts_points_outline_color ?? "#FF00FF";
            _settings.Alerts.AdvancedFont.Points.OutlineSize = legacy.alerts_points_outline_size;

            _settings.Alerts.AdvancedFont.Line.Color = legacy.alerts_line_color ?? "#FF00FF";
            _settings.Alerts.AdvancedFont.Line.OutlineEnabled = legacy.alerts_line_outline_enabled;
            _settings.Alerts.AdvancedFont.Line.OutlineColor = legacy.alerts_line_outline_color ?? "#FF00FF";
            _settings.Alerts.AdvancedFont.Line.OutlineSize = legacy.alerts_line_outline_size;

            // Custom achievement notification
            _settings.Alerts.CustomAchievement.Enabled = legacy.notification_custom_achievement_enable;
            _settings.Alerts.CustomAchievement.FilePath = legacy.notification_custom_achievement_file ?? "";
            _settings.Alerts.CustomAchievement.X = legacy.notification_custom_achievement_x;
            _settings.Alerts.CustomAchievement.Y = legacy.notification_custom_achievement_y;
            _settings.Alerts.CustomAchievement.Scale = legacy.notification_custom_achievement_scale;
            _settings.Alerts.CustomAchievement.FadeInTime = legacy.notification_custom_achievement_fade_in;
            _settings.Alerts.CustomAchievement.FadeOutTime = legacy.notification_custom_achievement_fade_out;
            _settings.Alerts.CustomAchievement.InSpeed = legacy.alerts_custom_achievement_in_speed;
            _settings.Alerts.CustomAchievement.OutSpeed = legacy.alerts_custom_achievement_out_speed;
            _settings.Alerts.CustomAchievement.AnimationIn = legacy.notifications_achievement_in_animation ?? "STATIC";
            _settings.Alerts.CustomAchievement.AnimationOut = legacy.notifications_achievement_out_animation ?? "UP";

            // Custom mastery notification
            _settings.Alerts.CustomMastery.Enabled = legacy.notification_custom_mastery_enable;
            _settings.Alerts.CustomMastery.FilePath = legacy.notification_custom_mastery_file ?? "";
            _settings.Alerts.CustomMastery.X = legacy.notification_custom_mastery_x;
            _settings.Alerts.CustomMastery.Y = legacy.notification_custom_mastery_y;
            _settings.Alerts.CustomMastery.Scale = legacy.notification_custom_mastery_scale;
            _settings.Alerts.CustomMastery.FadeInTime = legacy.notification_custom_mastery_fade_in;
            _settings.Alerts.CustomMastery.FadeOutTime = legacy.notification_custom_mastery_fade_out;
            _settings.Alerts.CustomMastery.InSpeed = legacy.alerts_custom_mastery_in_speed;
            _settings.Alerts.CustomMastery.OutSpeed = legacy.alerts_custom_mastery_out_speed;
            _settings.Alerts.CustomMastery.AnimationIn = legacy.notifications_mastery_in_animation ?? "STATIC";
            _settings.Alerts.CustomMastery.AnimationOut = legacy.notifications_mastery_out_animation ?? "UP";
        }

        private void MigrateUserInfoSettings(Settings legacy)
        {
            _settings.UserInfo.WindowBackgroundColor = legacy.stats_window_background_color ?? "#FF00FF";
            _settings.UserInfo.AdvancedOptionsEnabled = legacy.stats_advanced_options_enabled;

            _settings.UserInfo.SimpleFont.FontFamily = legacy.stats_font_family_name ?? "Calibri";
            _settings.UserInfo.SimpleFont.Color = legacy.stats_font_color_hex_code ?? "#000000";
            _settings.UserInfo.SimpleFont.OutlineEnabled = legacy.stats_font_outline_enabled;
            _settings.UserInfo.SimpleFont.OutlineColor = legacy.stats_font_outline_color_hex ?? "#FFFFFF";
            _settings.UserInfo.SimpleFont.OutlineSize = legacy.stats_font_outline_size;

            _settings.UserInfo.AdvancedFont.Names.FontFamily = legacy.stats_name_font_family ?? "Calibri";
            _settings.UserInfo.AdvancedFont.Names.Color = legacy.stats_name_color ?? "#FFFFFF";
            _settings.UserInfo.AdvancedFont.Names.OutlineEnabled = legacy.stats_name_outline_enabled;
            _settings.UserInfo.AdvancedFont.Names.OutlineColor = legacy.stats_name_outline_color ?? "#000000";
            _settings.UserInfo.AdvancedFont.Names.OutlineSize = legacy.stats_name_outline_size;

            _settings.UserInfo.AdvancedFont.Values.FontFamily = legacy.stats_value_font_family ?? "Calibri";
            _settings.UserInfo.AdvancedFont.Values.Color = legacy.stats_value_color ?? "#FFFFFF";
            _settings.UserInfo.AdvancedFont.Values.OutlineEnabled = legacy.stats_value_outline_enabled;
            _settings.UserInfo.AdvancedFont.Values.OutlineColor = legacy.stats_value_outline_color ?? "#000000";
            _settings.UserInfo.AdvancedFont.Values.OutlineSize = legacy.stats_value_outline_size;

            _settings.UserInfo.RankEnabled = legacy.stats_rank_enabled;
            _settings.UserInfo.PointsEnabled = legacy.stats_points_enabled;
            _settings.UserInfo.TruePointsEnabled = legacy.stats_true_points_enabled;
            _settings.UserInfo.RatioEnabled = legacy.stats_ratio_enabled;

            _settings.UserInfo.RankName = legacy.stats_rank_name ?? "Rank";
            _settings.UserInfo.PointsName = legacy.stats_points_name ?? "Points";
            _settings.UserInfo.TruePointsName = legacy.stats_true_points_name ?? "True Points";
            _settings.UserInfo.RatioName = legacy.stats_ratio_name ?? "Ratio";
        }

        private void MigrateGameInfoSettings(Settings legacy)
        {
            _settings.GameInfo.WindowBackgroundColor = legacy.game_info_window_background_color ?? "#FF00FF";
            _settings.GameInfo.AdvancedOptionsEnabled = legacy.game_info_advanced_options_enabled;

            _settings.GameInfo.SimpleFont.FontFamily = legacy.game_info_font_family_name ?? "Calibri";
            _settings.GameInfo.SimpleFont.Color = legacy.game_info_font_color_hex_code ?? "#000000";
            _settings.GameInfo.SimpleFont.OutlineEnabled = legacy.game_info_font_outline_enabled;
            _settings.GameInfo.SimpleFont.OutlineColor = legacy.game_info_font_outline_color_hex ?? "#FFFFFF";
            _settings.GameInfo.SimpleFont.OutlineSize = legacy.game_info_font_outline_size;

            _settings.GameInfo.AdvancedFont.Names.FontFamily = legacy.game_info_name_font_family ?? "Calibri";
            _settings.GameInfo.AdvancedFont.Names.Color = legacy.game_info_name_color ?? "#FFFFFF";
            _settings.GameInfo.AdvancedFont.Names.OutlineEnabled = legacy.game_info_name_outline_enabled;
            _settings.GameInfo.AdvancedFont.Names.OutlineColor = legacy.game_info_name_outline_color ?? "#000000";
            _settings.GameInfo.AdvancedFont.Names.OutlineSize = legacy.game_info_name_outline_size;

            _settings.GameInfo.AdvancedFont.Values.FontFamily = legacy.game_info_value_font_family ?? "Calibri";
            _settings.GameInfo.AdvancedFont.Values.Color = legacy.game_info_value_color ?? "#FFFFFF";
            _settings.GameInfo.AdvancedFont.Values.OutlineEnabled = legacy.game_info_value_outline_enabled;
            _settings.GameInfo.AdvancedFont.Values.OutlineColor = legacy.game_info_value_outline_color ?? "#000000";
            _settings.GameInfo.AdvancedFont.Values.OutlineSize = legacy.game_info_value_outline_size;

            _settings.GameInfo.TitleEnabled = legacy.game_info_title_enabled;
            _settings.GameInfo.ConsoleEnabled = legacy.game_info_console_enabled;
            _settings.GameInfo.DeveloperEnabled = legacy.game_info_developer_enabled;
            _settings.GameInfo.PublisherEnabled = legacy.game_info_publisher_enabled;
            _settings.GameInfo.GenreEnabled = legacy.game_info_genre_enabled;
            _settings.GameInfo.ReleaseDateEnabled = legacy.game_info_release_date_enabled;

            _settings.GameInfo.TitleName = legacy.game_info_title_name ?? "Title";
            _settings.GameInfo.ConsoleName = legacy.game_info_console_name ?? "Console";
            _settings.GameInfo.DeveloperName = legacy.game_info_developer_name ?? "Developer";
            _settings.GameInfo.PublisherName = legacy.game_info_publisher_name ?? "Publisher";
            _settings.GameInfo.GenreName = legacy.game_info_genre_name ?? "Genre";
            _settings.GameInfo.ReleaseDateName = legacy.game_info_release_date_name ?? "Released";
        }

        private void MigrateGameProgressSettings(Settings legacy)
        {
            _settings.GameProgress.WindowBackgroundColor = legacy.game_stats_window_background_color ?? "#FF00FF";
            _settings.GameProgress.AdvancedOptionsEnabled = legacy.game_stats_advanced_options_enabled;

            _settings.GameProgress.SimpleFont.FontFamily = legacy.game_stats_font_family_name ?? "Calibri";
            _settings.GameProgress.SimpleFont.Color = legacy.game_stats_font_color_hex_code ?? "#000000";
            _settings.GameProgress.SimpleFont.OutlineEnabled = legacy.game_stats_font_outline_enabled;
            _settings.GameProgress.SimpleFont.OutlineColor = legacy.game_stats_font_outline_color_hex ?? "#000000";
            _settings.GameProgress.SimpleFont.OutlineSize = legacy.game_stats_font_outline_size;

            _settings.GameProgress.AdvancedFont.Names.FontFamily = legacy.game_stats_name_font_family ?? "Calibri";
            _settings.GameProgress.AdvancedFont.Names.Color = legacy.game_stats_name_color ?? "#000000";
            _settings.GameProgress.AdvancedFont.Names.OutlineEnabled = legacy.game_stats_name_outline_enabled;
            _settings.GameProgress.AdvancedFont.Names.OutlineColor = legacy.game_stats_name_outline_color ?? "#000000";
            _settings.GameProgress.AdvancedFont.Names.OutlineSize = legacy.game_stats_name_outline_size;

            _settings.GameProgress.AdvancedFont.Values.FontFamily = legacy.game_stats_value_font_family ?? "Calibri";
            _settings.GameProgress.AdvancedFont.Values.Color = legacy.game_stats_value_color ?? "#000000";
            _settings.GameProgress.AdvancedFont.Values.OutlineEnabled = legacy.game_stats_value_outline_enabled;
            _settings.GameProgress.AdvancedFont.Values.OutlineColor = legacy.game_stats_value_outline_color ?? "#000000";

            _settings.GameProgress.AchievementsEnabled = legacy.stats_game_achievements_enabled;
            _settings.GameProgress.PointsEnabled = legacy.stats_game_points_enabled;
            _settings.GameProgress.TruePointsEnabled = legacy.stats_game_true_points_enabled;
            _settings.GameProgress.RatioEnabled = legacy.stats_game_ratio_enabled;
            _settings.GameProgress.CompletedEnabled = legacy.stats_completed_enabled;

            _settings.GameProgress.AchievementsName = legacy.stats_game_achievements_name ?? "Achievements";
            _settings.GameProgress.PointsName = legacy.stats_game_points_name ?? "Points";
            _settings.GameProgress.TruePointsName = legacy.stats_game_true_points_name ?? "True Points";
            _settings.GameProgress.RatioName = legacy.stats_game_ratio_name ?? "Ratio";
            _settings.GameProgress.CompletedName = legacy.stats_completed_name ?? "Completed";

            _settings.GameProgress.DividerCharacter = legacy.game_stats_divider_character_selection ?? "/";
        }

        private void MigrateRecentAchievementsSettings(Settings legacy)
        {
            _settings.RecentAchievements.WindowBackgroundColor = legacy.last_five_window_background_color ?? "#FF00FF";
            _settings.RecentAchievements.BorderBackgroundColor = legacy.last_five_background_color ?? "#FF00FF";
            _settings.RecentAchievements.BorderEnabled = legacy.last_five_border_enable;
            _settings.RecentAchievements.AdvancedOptionsEnabled = legacy.last_five_advanced_options_enabled;
            _settings.RecentAchievements.MaxListSize = legacy.recent_achievements_max_list_size;
            _settings.RecentAchievements.AutoScroll = legacy.recent_achievements_auto_scroll;

            _settings.RecentAchievements.SimpleFont.FontFamily = legacy.last_five_font_family_name ?? "Calibri";
            _settings.RecentAchievements.SimpleFont.Color = legacy.last_five_font_color_hex_code ?? "#000000";
            _settings.RecentAchievements.SimpleFont.OutlineEnabled = legacy.last_five_font_outline_enabled;
            _settings.RecentAchievements.SimpleFont.OutlineColor = legacy.last_five_font_outline_color_hex ?? "#FFFFFF";
            _settings.RecentAchievements.SimpleFont.OutlineSize = legacy.last_five_font_outline_size;

            _settings.RecentAchievements.AdvancedFont.Title.FontFamily = legacy.last_five_title_font_family ?? "Calibri";
            _settings.RecentAchievements.AdvancedFont.Title.Color = legacy.last_five_title_color ?? "#FFFFFF";
            _settings.RecentAchievements.AdvancedFont.Title.OutlineEnabled = legacy.last_five_title_outline_enabled;
            _settings.RecentAchievements.AdvancedFont.Title.OutlineColor = legacy.last_five_title_outline_color ?? "#000000";
            _settings.RecentAchievements.AdvancedFont.Title.OutlineSize = legacy.last_five_title_outline_size;

            _settings.RecentAchievements.AdvancedFont.Description.FontFamily = legacy.last_five_date_font_family ?? "Calibri";
            _settings.RecentAchievements.AdvancedFont.Description.Color = legacy.last_five_date_color ?? "#FFFFFF";
            _settings.RecentAchievements.AdvancedFont.Description.OutlineEnabled = legacy.last_five_date_outline_enabled;
            _settings.RecentAchievements.AdvancedFont.Description.OutlineColor = legacy.last_five_date_outline_color ?? "#000000";
            _settings.RecentAchievements.AdvancedFont.Description.OutlineSize = legacy.last_five_date_outline_size;

            _settings.RecentAchievements.AdvancedFont.Points.FontFamily = legacy.last_five_points_font_family ?? "Calibri";
            _settings.RecentAchievements.AdvancedFont.Points.Color = legacy.last_five_points_color ?? "#FFFFFF";
            _settings.RecentAchievements.AdvancedFont.Points.OutlineEnabled = legacy.last_five_points_outline_enabled;
            _settings.RecentAchievements.AdvancedFont.Points.OutlineColor = legacy.last_five_points_outline_color ?? "#000000";
            _settings.RecentAchievements.AdvancedFont.Points.OutlineSize = legacy.last_five_points_outline_size;

            _settings.RecentAchievements.AdvancedFont.Line.Color = legacy.last_five_line_color ?? "#FFFFFF";
            _settings.RecentAchievements.AdvancedFont.Line.OutlineEnabled = legacy.last_five_line_outline_enabled;
            _settings.RecentAchievements.AdvancedFont.Line.OutlineColor = legacy.last_five_line_outline_color ?? "#000000";
            _settings.RecentAchievements.AdvancedFont.Line.OutlineSize = legacy.last_five_line_outline_size;
        }

        private void MigrateAchievementListSettings(Settings legacy)
        {
            _settings.AchievementList.WindowBackgroundColor = legacy.achievement_list_window_background_color ?? "#FF00FF";
            _settings.AchievementList.BorderColor = legacy.achievement_list_border_color ?? "#CC9900";
            _settings.AchievementList.WindowSizeX = legacy.achievement_list_window_size_x;
            _settings.AchievementList.WindowSizeY = legacy.achievement_list_window_size_y;
            _settings.AchievementList.AutoScroll = legacy.achievement_list_auto_scroll;
        }

        private void MigrateRelatedMediaSettings(Settings legacy)
        {
            _settings.RelatedMedia.WindowBackgroundColor = legacy.related_media_window_background_color ?? "#FF00FF";
            _settings.RelatedMedia.LaunchBoxFilePath = legacy.related_media_launchbox_filepath ?? "";
            _settings.RelatedMedia.MediaSelection = legacy.related_media_selection ?? "BADGE_ICON";
        }

        /// <summary>
        /// Saves the current settings to the JSON file.
        /// </summary>
        public void Save()
        {
            try
            {
                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include
                };

                string json = JsonConvert.SerializeObject(_settings, jsonSettings);
                File.WriteAllText(_settingsPath, json);
                _isDirty = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the decrypted API key.
        /// </summary>
        public string GetApiKey()
        {
            return CredentialProtector.Decrypt(_settings.Credentials.EncryptedApiKey);
        }

        /// <summary>
        /// Sets and encrypts the API key.
        /// </summary>
        public void SetApiKey(string apiKey)
        {
            _settings.Credentials.EncryptedApiKey = CredentialProtector.Encrypt(apiKey);
            _isDirty = true;
        }

        /// <summary>
        /// Marks settings as changed (will save on next Save() call).
        /// </summary>
        public void MarkDirty()
        {
            _isDirty = true;
        }

        /// <summary>
        /// Returns true if settings have unsaved changes.
        /// </summary>
        public bool HasUnsavedChanges => _isDirty;

        /// <summary>
        /// Reloads settings from disk, discarding any unsaved changes.
        /// </summary>
        public void Reload()
        {
            LoadOrMigrate();
        }
    }
}
```

### 4. Integration Steps

After creating the above files, integrate the new settings service:

1. **Update Program.cs** to initialize SettingsService early:
```csharp
// In Program.cs, before Application.Run()
var settings = SettingsService.Instance; // Triggers load/migration
```

2. **Update MainWindow.cs** credential access:
```csharp
// Replace:
private string Username
{
    get => Settings.Default.ra_username;
    set => Settings.Default.ra_username = value;
}

// With:
private string Username
{
    get => SettingsService.Instance.Current.Credentials.Username;
    set
    {
        SettingsService.Instance.Current.Credentials.Username = value;
        SettingsService.Instance.MarkDirty();
    }
}

private string WebAPIKey
{
    get => SettingsService.Instance.GetApiKey();
    set => SettingsService.Instance.SetApiKey(value);
}
```

3. **Update OnClosed** to save settings:
```csharp
protected override void OnClosed(EventArgs e)
{
    // Save to new JSON format
    SettingsService.Instance.Save();
    
    // ... rest of cleanup
}
```

4. **Gradually migrate controllers** to use `SettingsService.Instance.Current` instead of `Settings.Default`

---

## File Structure After Migration

```
Retro Achievement Tracker/
??? Models/
?   ??? UserSettings.cs          # New settings model classes
??? Services/
?   ??? AchievementTrackingService.cs
?   ??? CredentialProtector.cs   # New DPAPI encryption helper
?   ??? SettingsService.cs       # New settings management service
??? settings.json                # Created on first run (in app directory)
??? ... existing files
```

---

## Sample settings.json Output

```json
{
  "SchemaVersion": 1,
  "Credentials": {
    "Username": "MyRAUsername",
    "EncryptedApiKey": "AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAA...",
    "PreviouslyPlayedGameId": 12345
  },
  "AutoLaunch": {
    "AutoStart": true,
    "Focus": true,
    "Alerts": true,
    "UserInfo": false,
    "GameInfo": false,
    "GameProgress": false,
    "RecentAchievements": false,
    "AchievementList": false,
    "RelatedMedia": false
  },
  "Focus": {
    "WindowBackgroundColor": "#FF00FF",
    "BorderBackgroundColor": "#FF00FF",
    "BorderEnabled": true,
    "AdvancedOptionsEnabled": false,
    "RefocusBehavior": "GO_TO_FIRST",
    "SimpleFont": {
      "FontFamily": "Calibri",
      "Color": "#FFFFFF",
      "OutlineEnabled": true,
      "OutlineColor": "#000000",
      "OutlineSize": 2
    }
  }
  // ... other settings sections
}
```

---

## Security Considerations

1. **DPAPI Protection**: The API key is encrypted using Windows Data Protection API with `CurrentUser` scope, meaning:
   - Only the Windows user who encrypted it can decrypt it
   - The key is tied to the user's Windows credentials
   - If the user's profile is deleted/corrupted, the key cannot be recovered

2. **settings.json Location**: The file is in the application directory, which is typically protected by standard Windows file permissions

3. **Backup Recommendation**: Users should be advised to keep their API key noted elsewhere, as the encrypted version cannot be recovered if Windows profile issues occur

---

## Testing Checklist

- [ ] Fresh install creates `settings.json` with defaults
- [ ] Existing users with `Properties.Settings` data get migrated automatically
- [ ] API key is encrypted in `settings.json` (not plain text)
- [ ] API key decrypts correctly for API calls
- [ ] Settings changes persist across app restarts
- [ ] Deleting `settings.json` triggers re-migration from legacy settings
- [ ] Schema version is set correctly
- [ ] All controller settings are preserved during migration
