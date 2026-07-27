using System.Text.Json.Serialization;

namespace RATracker.WPF.Models;

/// <summary>
/// Application settings model for JSON-based persistence.
/// Stores user preferences, auto-launch flags, and feature settings.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Gets or sets the RetroAchievements username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the encrypted API key.
    /// The key is stored in an encrypted/encoded format for basic security.
    /// </summary>
    public string EncryptedApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the encrypted password for session-based login.
    /// Encrypted with DPAPI, same as API key.
    /// </summary>
    public string EncryptedPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether to remember the password between sessions.
    /// </summary>
    public bool RememberPassword { get; set; }

    #region Auto-Start and Auto-Launch Settings

    /// <summary>
    /// Gets or sets whether polling should start automatically when the application opens.
    /// </summary>
    public bool AutoStart { get; set; }

    /// <summary>
    /// Gets or sets whether the Focus overlay should auto-launch when polling starts.
    /// </summary>
    public bool AutoLaunchFocus { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the Alerts overlay should auto-launch when polling starts.
    /// </summary>
    public bool AutoLaunchAlerts { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the User Info overlay should auto-launch when polling starts.
    /// </summary>
    public bool AutoLaunchUserInfo { get; set; }

    /// <summary>
    /// Gets or sets whether the Game Info overlay should auto-launch when polling starts.
    /// </summary>
    public bool AutoLaunchGameInfo { get; set; }

    /// <summary>
    /// Gets or sets whether the Game Progress overlay should auto-launch when polling starts.
    /// </summary>
    public bool AutoLaunchGameProgress { get; set; }

    /// <summary>
    /// Gets or sets whether the Recent Unlocks overlay should auto-launch when polling starts.
    /// </summary>
    public bool AutoLaunchRecentUnlocks { get; set; }

    /// <summary>
    /// Gets or sets whether the Achievement List overlay should auto-launch when polling starts.
    /// </summary>
    public bool AutoLaunchAchievementList { get; set; }

    /// <summary>
    /// Gets or sets whether the Related Media overlay should auto-launch when polling starts.
    /// </summary>
    public bool AutoLaunchRelatedMedia { get; set; }

    #endregion

    #region API Feature Flags

    /// <summary>
    /// Gets or sets whether to enable API request logging.
    /// </summary>
    public bool EnableApiLogging { get; set; }

    #endregion

    #region Stream Labels

    /// <summary>
    /// Gets or sets whether stream label file generation is enabled.
    /// When enabled, text files are written to the stream-labels/ directory for OBS integration.
    /// </summary>
    public bool EnableStreamLabels { get; set; }

    #endregion

    #region Refocus Behavior

    /// <summary>
    /// Gets or sets the refocus behavior when the current focus achievement is unlocked.
    /// Stored as string representation of RefocusBehaviorEnum.
    /// </summary>
    public string RefocusBehavior { get; set; } = "GO_TO_NEXT";

    #endregion

    #region Recent Unlocks Settings

    /// <summary>
    /// Gets or sets whether to auto-detect the timezone for the Recent Unlocks overlay.
    /// When true, uses the system's local timezone.
    /// </summary>
    public bool RecentUnlocksAutoDetectTimezone { get; set; } = true;

    /// <summary>
    /// Gets or sets the timezone ID for the Recent Unlocks overlay.
    /// Only used when AutoDetectTimezone is false.
    /// Stored as a TimeZoneInfo.Id string.
    /// </summary>
    public string RecentUnlocksTimezoneId { get; set; } = string.Empty;

    #endregion

    #region Focus Overlay Settings

    /// <summary>
    /// Gets or sets the Focus overlay window width.
    /// </summary>
    public double FocusWindowWidth { get; set; } = 700;

    /// <summary>
    /// Gets or sets the Focus overlay window height.
    /// </summary>
    public double FocusWindowHeight { get; set; } = 175;

    /// <summary>
    /// Gets or sets the badge size in the Focus overlay.
    /// </summary>
    public double FocusBadgeSize { get; set; } = 140;

    /// <summary>
    /// Gets or sets the badge corner radius in the Focus overlay.
    /// </summary>
    public double FocusBadgeCornerRadius { get; set; } = 5;

    /// <summary>
    /// Gets or sets the container corner radius in the Focus overlay.
    /// </summary>
    public double FocusContainerCornerRadius { get; set; } = 8;

    /// <summary>
    /// Gets or sets the container margin in the Focus overlay.
    /// </summary>
    public double FocusContainerMargin { get; set; } = 5;

    /// <summary>
    /// Gets or sets the content spacing in the Focus overlay.
    /// </summary>
    public double FocusContentSpacing { get; set; } = 15;

    /// <summary>
    /// Gets or sets whether to show the badge in the Focus overlay.
    /// </summary>
    public bool FocusShowBadge { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show the title in the Focus overlay.
    /// </summary>
    public bool FocusShowTitle { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show the separator line in the Focus overlay.
    /// </summary>
    public bool FocusShowLine { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show the points in the Focus overlay.
    /// </summary>
    public bool FocusShowPoints { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show the description in the Focus overlay.
    /// </summary>
    public bool FocusShowDescription { get; set; } = true;

    /// <summary>
    /// Gets or sets the title font size in the Focus overlay.
    /// </summary>
    public double FocusTitleFontSize { get; set; } = 26;

    /// <summary>
    /// Gets or sets the points font size in the Focus overlay.
    /// </summary>
    public double FocusPointsFontSize { get; set; } = 36;

    /// <summary>
    /// Gets or sets the description font size in the Focus overlay.
    /// </summary>
    public double FocusDescriptionFontSize { get; set; } = 16;

    /// <summary>
    /// Gets or sets the mastery info font size in the Focus overlay.
    /// </summary>
    public double FocusMasteryFontSize { get; set; } = 20;

    /// <summary>
    /// Gets or sets the separator line height in the Focus overlay.
    /// </summary>
    public double FocusLineHeight { get; set; } = 4;

    /// <summary>
    /// Gets or sets the separator line margin in the Focus overlay.
    /// </summary>
    public double FocusLineMargin { get; set; } = 8;

    #endregion

    #region Game and Set Selection

    /// <summary>
    /// Gets or sets the last played game ID for restoring state on restart.
    /// </summary>
    public long LastPlayedGameId { get; set; }

    /// <summary>
    /// Gets or sets the last selected achievement set ID.
    /// Used to restore the selected set when returning to a previously played game.
    /// </summary>
    public long LastSelectedSetId { get; set; }

    /// <summary>
    /// Gets or sets the last selected achievement set name.
    /// Used as a fallback when set ID is not available.
    /// </summary>
    public string LastSelectedSetName { get; set; } = string.Empty;

    #endregion

    #region Position Mode


    #endregion

    #region Window Positions

    /// <summary>
    /// Gets or sets the main window X position.
    /// </summary>
    public double? MainWindowX { get; set; }

    /// <summary>
    /// Gets or sets the main window Y position.
    /// </summary>
    public double? MainWindowY { get; set; }

    /// <summary>
    /// Gets or sets the Focus overlay X position.
    /// </summary>
    public double? FocusOverlayX { get; set; }

    /// <summary>
    /// Gets or sets the Focus overlay Y position.
    /// </summary>
    public double? FocusOverlayY { get; set; }

    /// <summary>
    /// Gets or sets the Alerts overlay X position.
    /// </summary>
    public double? AlertsOverlayX { get; set; }

    /// <summary>
    /// Gets or sets the Alerts overlay Y position.
    /// </summary>
    public double? AlertsOverlayY { get; set; }

    #region Updates

    /// <summary>
    /// Gets or sets whether the app checks GitHub Releases for updates on startup. Updates are only
    /// ever staged and applied on exit — never mid-session.
    /// </summary>
    public bool AutoUpdateEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets whether pre-release builds are offered. Testers turn this on to receive test
    /// builds; everyone else stays on stable releases.
    /// </summary>
    public bool UpdateToPrereleases { get; set; }

    #endregion

    #region Alert Container Shape

    /// <summary>
    /// Gets or sets the Alerts overlay window width. Persisted like the Focus overlay's, so a window
    /// sized to fit a scaled custom video survives a restart instead of resetting to the default.
    /// </summary>
    public double AlertsWindowWidth { get; set; } = 550;

    /// <summary>Gets or sets the Alerts overlay window height.</summary>
    public double AlertsWindowHeight { get; set; } = 250;

    /// <summary>Gets or sets the alert panel's X position inside the overlay window.</summary>
    public double AlertAchievementLeft { get; set; } = 20;

    /// <summary>Gets or sets the alert panel's Y position inside the overlay window.</summary>
    public double AlertAchievementTop { get; set; } = 20;

    /// <summary>Gets or sets the alert container width in px.</summary>
    public double AlertNotificationWidth { get; set; } = 500;

    /// <summary>Gets or sets the alert container height in px. 0 sizes the container to its content.</summary>
    public double AlertNotificationHeight { get; set; }

    /// <summary>Gets or sets the padding between the container edge and its contents.</summary>
    public double AlertContainerPadding { get; set; } = 15;

    /// <summary>Gets or sets the container corner radius.</summary>
    public double AlertContainerCornerRadius { get; set; } = 10;

    /// <summary>Gets or sets where the badge sits: Left, Right, Top, Bottom or Hidden.</summary>
    public string AlertBadgePlacement { get; set; } = "Left";

    /// <summary>Gets or sets the badge edge length in px.</summary>
    public double AlertBadgeSize { get; set; } = 96;

    /// <summary>Gets or sets the badge corner radius.</summary>
    public double AlertBadgeCornerRadius { get; set; } = 5;

    /// <summary>Gets or sets the gap between the badge and the text.</summary>
    public double AlertContentSpacing { get; set; } = 15;

    #endregion

    #region Custom Alert Videos

    /// <summary>Gets or sets whether achievement unlocks play a custom video alert.</summary>
    public bool CustomAchievementEnabled { get; set; }

    /// <summary>Gets or sets the path to the custom achievement alert video (.webm).</summary>
    public string CustomAchievementVideoPath { get; set; } = string.Empty;

    /// <summary>Gets or sets the custom achievement video's X offset in px (may be negative).</summary>
    public double CustomAchievementX { get; set; }

    /// <summary>Gets or sets the custom achievement video's Y offset in px (may be negative).</summary>
    public double CustomAchievementY { get; set; }

    /// <summary>Gets or sets the scale multiplier applied to the video's native width.</summary>
    public double CustomAchievementScale { get; set; } = 1.0;

    /// <summary>Gets or sets the video position (ms) at which the panel animates in.</summary>
    public int CustomAchievementInTime { get; set; }

    /// <summary>Gets or sets the video position (ms) at which the panel animates out.</summary>
    public int CustomAchievementOutTime { get; set; } = 5200;

    /// <summary>Gets or sets the in-animation duration (ms). 0 snaps into place.</summary>
    public int CustomAchievementInSpeed { get; set; }

    /// <summary>Gets or sets the out-animation duration (ms).</summary>
    public int CustomAchievementOutSpeed { get; set; } = 700;

    /// <summary>Gets or sets whether masteries play a custom video alert.</summary>
    public bool CustomMasteryEnabled { get; set; }

    /// <summary>Gets or sets the path to the custom mastery alert video (.webm).</summary>
    public string CustomMasteryVideoPath { get; set; } = string.Empty;

    /// <summary>Gets or sets the custom mastery video's X offset in px (may be negative).</summary>
    public double CustomMasteryX { get; set; }

    /// <summary>Gets or sets the custom mastery video's Y offset in px (may be negative).</summary>
    public double CustomMasteryY { get; set; }

    /// <summary>Gets or sets the scale multiplier applied to the video's native width.</summary>
    public double CustomMasteryScale { get; set; } = 1.0;

    /// <summary>Gets or sets the video position (ms) at which the panel animates in.</summary>
    public int CustomMasteryInTime { get; set; }

    /// <summary>Gets or sets the video position (ms) at which the panel animates out.</summary>
    public int CustomMasteryOutTime { get; set; } = 5200;

    /// <summary>Gets or sets the in-animation duration (ms). 0 snaps into place.</summary>
    public int CustomMasteryInSpeed { get; set; }

    /// <summary>Gets or sets the out-animation duration (ms).</summary>
    public int CustomMasteryOutSpeed { get; set; } = 700;

    /// <summary>Gets or sets the custom achievement in-animation direction.</summary>
    public string CustomAchievementInDirection { get; set; } = "Static";

    /// <summary>Gets or sets the custom achievement out-animation direction.</summary>
    public string CustomAchievementOutDirection { get; set; } = "Up";

    /// <summary>Gets or sets the custom mastery in-animation direction.</summary>
    public string CustomMasteryInDirection { get; set; } = "Static";

    /// <summary>Gets or sets the custom mastery out-animation direction.</summary>
    public string CustomMasteryOutDirection { get; set; } = "Up";

    #endregion

    /// <summary>
    /// Gets or sets the User Info overlay X position.
    /// </summary>
    public double? UserInfoOverlayX { get; set; }

    /// <summary>
    /// Gets or sets the User Info overlay Y position.
    /// </summary>
    public double? UserInfoOverlayY { get; set; }

    /// <summary>
    /// Gets or sets the Game Info overlay X position.
    /// </summary>
    public double? GameInfoOverlayX { get; set; }

    /// <summary>
    /// Gets or sets the Game Info overlay Y position.
    /// </summary>
    public double? GameInfoOverlayY { get; set; }

    /// <summary>
    /// Gets or sets the Game Progress overlay X position.
    /// </summary>
    public double? GameProgressOverlayX { get; set; }

    /// <summary>
    /// Gets or sets the Game Progress overlay Y position.
    /// </summary>
    public double? GameProgressOverlayY { get; set; }

    /// <summary>
    /// Gets or sets the Recent Unlocks overlay X position.
    /// </summary>
    public double? RecentUnlocksOverlayX { get; set; }

    /// <summary>
    /// Gets or sets the Recent Unlocks overlay Y position.
    /// </summary>
    public double? RecentUnlocksOverlayY { get; set; }

    /// <summary>
    /// Gets or sets the Achievement List overlay X position.
    /// </summary>
    public double? AchievementListOverlayX { get; set; }

    /// <summary>
    /// Gets or sets the Achievement List overlay Y position.
    /// </summary>
    public double? AchievementListOverlayY { get; set; }

    /// <summary>
    /// Gets or sets the Related Media overlay X position.
    /// </summary>
    public double? RelatedMediaOverlayX { get; set; }

    /// <summary>
    /// Gets or sets the Related Media overlay Y position.
    /// </summary>
    public double? RelatedMediaOverlayY { get; set; }

    #endregion

    /// <summary>
    /// Gets the settings file version for migration purposes.
    /// </summary>
    [JsonPropertyName("version")]
    public int Version { get; set; } = 1;
}
