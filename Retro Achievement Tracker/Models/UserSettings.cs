namespace RATracker.Models
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
