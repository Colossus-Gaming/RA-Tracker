using Newtonsoft.Json;
using RATracker.Models;
using RATracker.Properties;
using System.IO;

namespace RATracker.Services
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
