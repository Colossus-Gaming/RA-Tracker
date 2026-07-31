using System.Globalization;
using System.IO;
using System.Text.Json;
using RATracker.Models;

namespace RATracker.WPF.Services;

/// <summary>
/// Service for generating stream label text files for OBS integration.
/// Writes achievement, game, and user data to text files that can be
/// read by OBS as text sources for stream overlays.
/// </summary>
public class StreamLabelService
{
    private static readonly Lazy<StreamLabelService> _instance = new(() => new StreamLabelService());

    private readonly string _basePath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly object _writeLock = new();
    private bool _isEnabled;

    /// <summary>
    /// Gets the singleton instance of the StreamLabelService.
    /// </summary>
    public static StreamLabelService Instance => _instance.Value;

    /// <summary>
    /// Gets or sets whether stream label generation is enabled.
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            if (value)
            {
                EnsureDirectoriesExist();
            }
        }
    }

    private StreamLabelService()
    {
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), "stream-labels");
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    #region Directory Management

    /// <summary>
    /// Ensures all required stream label directories exist.
    /// </summary>
    public void EnsureDirectoriesExist()
    {
        try
        {
            var directories = new[]
            {
                Path.Combine(_basePath, "focus"),
                Path.Combine(_basePath, "alerts"),
                Path.Combine(_basePath, "game-info"),
                Path.Combine(_basePath, "user-info"),
                Path.Combine(_basePath, "last-five")
            };

            foreach (var dir in directories)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    System.Diagnostics.Debug.WriteLine($"[StreamLabels] Created directory: {dir}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StreamLabels] Error creating directories: {ex.Message}");
        }
    }

    #endregion

    #region Focus Labels

    /// <summary>
    /// Writes focus achievement stream labels including set name for multi-set games.
    /// </summary>
    /// <param name="achievement">The focused achievement.</param>
    /// <param name="setName">Optional achievement set name for multi-set games.</param>
    public void WriteFocusLabels(Achievement? achievement, string? setName = null)
    {
        if (!IsEnabled) return;

        lock (_writeLock)
        {
            try
            {
                var focusPath = Path.Combine(_basePath, "focus");

                if (achievement != null)
                {
                    WriteFile(Path.Combine(focusPath, "title.txt"), achievement.Title);
                    WriteFile(Path.Combine(focusPath, "description.txt"), achievement.Description);
                    WriteFile(Path.Combine(focusPath, "points.txt"), achievement.Points.ToString());
                    WriteFile(Path.Combine(focusPath, "set-name.txt"), setName ?? string.Empty);
                    WriteFile(Path.Combine(focusPath, "data.json"), SerializeToJson(achievement));

                    System.Diagnostics.Debug.WriteLine($"[StreamLabels] Wrote focus labels for: {achievement.Title}");
                }
                else
                {
                    ClearFocusLabels();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StreamLabels] Error writing focus labels: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Clears all focus stream labels.
    /// </summary>
    public void ClearFocusLabels()
    {
        if (!IsEnabled) return;

        lock (_writeLock)
        {
            try
            {
                var focusPath = Path.Combine(_basePath, "focus");

                WriteFile(Path.Combine(focusPath, "title.txt"), string.Empty);
                WriteFile(Path.Combine(focusPath, "description.txt"), string.Empty);
                WriteFile(Path.Combine(focusPath, "points.txt"), string.Empty);
                WriteFile(Path.Combine(focusPath, "set-name.txt"), string.Empty);
                WriteFile(Path.Combine(focusPath, "data.json"), "{}");

                System.Diagnostics.Debug.WriteLine("[StreamLabels] Cleared focus labels");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StreamLabels] Error clearing focus labels: {ex.Message}");
            }
        }
    }

    #endregion

    #region Alert Labels

    /// <summary>
    /// Writes alert stream labels for an achievement unlock.
    /// </summary>
    /// <param name="achievement">The unlocked achievement.</param>
    /// <param name="setName">Optional achievement set name for multi-set games.</param>
    public void WriteAlertLabels(Achievement? achievement, string? setName = null)
    {
        if (!IsEnabled || achievement == null) return;

        lock (_writeLock)
        {
            try
            {
                var alertPath = Path.Combine(_basePath, "alerts");

                WriteFile(Path.Combine(alertPath, "title.txt"), achievement.Title);
                WriteFile(Path.Combine(alertPath, "description.txt"), achievement.Description);
                WriteFile(Path.Combine(alertPath, "points.txt"), achievement.Points.ToString());
                WriteFile(Path.Combine(alertPath, "set-name.txt"), setName ?? string.Empty);
                WriteFile(Path.Combine(alertPath, "data.json"), SerializeToJson(achievement));

                System.Diagnostics.Debug.WriteLine($"[StreamLabels] Wrote alert labels for: {achievement.Title}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StreamLabels] Error writing alert labels: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Writes alert stream labels for a game mastery.
    /// </summary>
    /// <param name="gameInfo">The mastered game info.</param>
    /// <param name="setName">Optional achievement set name for multi-set mastery.</param>
    public void WriteAlertLabels(GameInfo? gameInfo, string? setName = null)
    {
        if (!IsEnabled || gameInfo == null) return;

        lock (_writeLock)
        {
            try
            {
                var alertPath = Path.Combine(_basePath, "alerts");

                WriteFile(Path.Combine(alertPath, "title.txt"), gameInfo.Title);
                WriteFile(Path.Combine(alertPath, "description.txt"), "MASTERED!");
                WriteFile(Path.Combine(alertPath, "points.txt"), gameInfo.GamePointsPossible.ToString());
                WriteFile(Path.Combine(alertPath, "set-name.txt"), setName ?? string.Empty);
                WriteFile(Path.Combine(alertPath, "data.json"), SerializeToJson(gameInfo));

                System.Diagnostics.Debug.WriteLine($"[StreamLabels] Wrote mastery alert labels for: {gameInfo.Title}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StreamLabels] Error writing mastery alert labels: {ex.Message}");
            }
        }
    }

    #endregion

    #region User Info Labels

    /// <summary>
    /// Writes user info stream labels.
    /// </summary>
    /// <param name="userSummary">The user summary data.</param>
    public void WriteUserInfoLabels(UserSummary? userSummary)
    {
        if (!IsEnabled || userSummary == null) return;

        lock (_writeLock)
        {
            try
            {
                var userPath = Path.Combine(_basePath, "user-info");

                WriteFile(Path.Combine(userPath, "rank.txt"), 
                    userSummary.Rank == 0 ? "No Rank" : userSummary.Rank.ToString());
                WriteFile(Path.Combine(userPath, "ratio.txt"), userSummary.RetroRatio);
                WriteFile(Path.Combine(userPath, "points.txt"), userSummary.TotalPoints.ToString());
                WriteFile(Path.Combine(userPath, "true-points.txt"), userSummary.TotalTruePoints.ToString());
                WriteFile(Path.Combine(userPath, "data.json"), SerializeToJson(userSummary));

                System.Diagnostics.Debug.WriteLine($"[StreamLabels] Wrote user info labels for: {userSummary.UserName}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StreamLabels] Error writing user info labels: {ex.Message}");
            }
        }
    }

    #endregion

    #region Game Info Labels

    /// <summary>
    /// Writes game info stream labels including current set name for multi-set games.
    /// </summary>
    /// <param name="gameInfo">The game info data.</param>
    /// <param name="currentSetName">Optional current achievement set name for multi-set games.</param>
    public void WriteGameInfoLabels(GameInfo? gameInfo, string? currentSetName = null)
    {
        if (!IsEnabled || gameInfo == null) return;

        lock (_writeLock)
        {
            try
            {
                var gamePath = Path.Combine(_basePath, "game-info");

                WriteFile(Path.Combine(gamePath, "title.txt"), gameInfo.Title);
                WriteFile(Path.Combine(gamePath, "console.txt"), gameInfo.ConsoleName);
                WriteFile(Path.Combine(gamePath, "developer.txt"), gameInfo.Developer ?? string.Empty);
                WriteFile(Path.Combine(gamePath, "publisher.txt"), gameInfo.Publisher ?? string.Empty);
                WriteFile(Path.Combine(gamePath, "genre.txt"), gameInfo.Genre ?? string.Empty);
                WriteFile(Path.Combine(gamePath, "released.txt"), gameInfo.Released ?? string.Empty);
                WriteFile(Path.Combine(gamePath, "current-set.txt"), currentSetName ?? string.Empty);
                WriteFile(Path.Combine(gamePath, "data.json"), SerializeToJson(gameInfo));

                System.Diagnostics.Debug.WriteLine($"[StreamLabels] Wrote game info labels for: {gameInfo.Title}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StreamLabels] Error writing game info labels: {ex.Message}");
            }
        }
    }

    #endregion

    #region Game Progress Labels

    /// <summary>
    /// Writes game progress stream labels.
    /// </summary>
    /// <param name="gameInfo">The game info with progress data.</param>
    public void WriteGameProgressLabels(GameInfo? gameInfo)
    {
        if (!IsEnabled || gameInfo == null) return;

        lock (_writeLock)
        {
            try
            {
                var gamePath = Path.Combine(_basePath, "game-info");

                // Calculate ratio
                var ratio = gameInfo.GamePointsPossible == 0 
                    ? "0.00" 
                    : (Convert.ToDecimal(gameInfo.GameTruePointsPossible) / 
                       Convert.ToDecimal(gameInfo.GamePointsPossible)).ToString("0.00", CultureInfo.InvariantCulture);

                // Calculate completion percentage
                var achievementCount = gameInfo.Achievements?.Count ?? 0;
                var achievementsEarned = gameInfo.Achievements?.Count(a => a.DateEarned.HasValue) ?? 0;
                var completionPercent = achievementCount == 0 
                    ? "0.00 %" 
                    : ((decimal)achievementsEarned / achievementCount * 100).ToString("0.00", CultureInfo.InvariantCulture) + " %";

                WriteFile(Path.Combine(gamePath, "ratio.txt"), ratio);
                WriteFile(Path.Combine(gamePath, "points.txt"), 
                    $"{gameInfo.GamePointsEarned} / {gameInfo.GamePointsPossible}");
                WriteFile(Path.Combine(gamePath, "true-points.txt"), 
                    $"{gameInfo.GameTruePointsEarned} / {gameInfo.GameTruePointsPossible}");
                WriteFile(Path.Combine(gamePath, "achievements.txt"), 
                    $"{achievementsEarned} / {achievementCount}");
                WriteFile(Path.Combine(gamePath, "completed.txt"), completionPercent);

                System.Diagnostics.Debug.WriteLine($"[StreamLabels] Wrote game progress labels: {completionPercent}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StreamLabels] Error writing game progress labels: {ex.Message}");
            }
        }
    }

    #endregion

    #region Recent Unlocks Labels

    /// <summary>
    /// Writes recent unlocks (last five) stream labels.
    /// </summary>
    /// <param name="achievements">The list of recently unlocked achievements.</param>
    public void WriteRecentUnlocksLabels(List<Achievement>? achievements)
    {
        if (!IsEnabled) return;

        lock (_writeLock)
        {
            try
            {
                var lastFivePath = Path.Combine(_basePath, "last-five");

                if (achievements != null && achievements.Count > 0)
                {
                    // Sort by date earned, most recent first
                    var sorted = achievements
                        .Where(a => a.DateEarned.HasValue)
                        .OrderByDescending(a => a.DateEarned)
                        .Take(5)
                        .ToList();

                    for (int i = 0; i < 5; i++)
                    {
                        var index = i + 1;
                        if (i < sorted.Count)
                        {
                            var achievement = sorted[i];
                            WriteFile(Path.Combine(lastFivePath, $"last-{index}-title.txt"), achievement.Title);
                            WriteFile(Path.Combine(lastFivePath, $"last-{index}-description.txt"), achievement.Description);
                            WriteFile(Path.Combine(lastFivePath, $"last-{index}-points.txt"), achievement.Points.ToString());
                            WriteFile(Path.Combine(lastFivePath, $"last-{index}-data.json"), SerializeToJson(achievement));
                        }
                        else
                        {
                            // Clear remaining slots
                            WriteFile(Path.Combine(lastFivePath, $"last-{index}-title.txt"), string.Empty);
                            WriteFile(Path.Combine(lastFivePath, $"last-{index}-description.txt"), string.Empty);
                            WriteFile(Path.Combine(lastFivePath, $"last-{index}-points.txt"), string.Empty);
                            WriteFile(Path.Combine(lastFivePath, $"last-{index}-data.json"), "{}");
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"[StreamLabels] Wrote {sorted.Count} recent unlock labels");
                }
                else
                {
                    ClearRecentUnlocksLabels();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StreamLabels] Error writing recent unlocks labels: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Clears all recent unlocks stream labels.
    /// </summary>
    public void ClearRecentUnlocksLabels()
    {
        if (!IsEnabled) return;

        lock (_writeLock)
        {
            try
            {
                var lastFivePath = Path.Combine(_basePath, "last-five");

                for (int i = 1; i <= 5; i++)
                {
                    WriteFile(Path.Combine(lastFivePath, $"last-{i}-title.txt"), string.Empty);
                    WriteFile(Path.Combine(lastFivePath, $"last-{i}-description.txt"), string.Empty);
                    WriteFile(Path.Combine(lastFivePath, $"last-{i}-points.txt"), string.Empty);
                    WriteFile(Path.Combine(lastFivePath, $"last-{i}-data.json"), "{}");
                }

                System.Diagnostics.Debug.WriteLine("[StreamLabels] Cleared recent unlocks labels");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StreamLabels] Error clearing recent unlocks labels: {ex.Message}");
            }
        }
    }

    #endregion

    #region Clear All Labels

    /// <summary>
    /// Clears all stream label files.
    /// Call this when polling stops.
    /// </summary>
    public void ClearAllLabels()
    {
        if (!IsEnabled) return;

        lock (_writeLock)
        {
            try
            {
                // Focus labels
                var focusPath = Path.Combine(_basePath, "focus");
                WriteFile(Path.Combine(focusPath, "title.txt"), string.Empty);
                WriteFile(Path.Combine(focusPath, "description.txt"), string.Empty);
                WriteFile(Path.Combine(focusPath, "points.txt"), string.Empty);
                WriteFile(Path.Combine(focusPath, "set-name.txt"), string.Empty);
                WriteFile(Path.Combine(focusPath, "data.json"), "{}");

                // Alert labels
                var alertPath = Path.Combine(_basePath, "alerts");
                WriteFile(Path.Combine(alertPath, "title.txt"), string.Empty);
                WriteFile(Path.Combine(alertPath, "description.txt"), string.Empty);
                WriteFile(Path.Combine(alertPath, "points.txt"), string.Empty);
                WriteFile(Path.Combine(alertPath, "set-name.txt"), string.Empty);
                WriteFile(Path.Combine(alertPath, "data.json"), "{}");

                // Game info labels
                var gamePath = Path.Combine(_basePath, "game-info");
                WriteFile(Path.Combine(gamePath, "title.txt"), string.Empty);
                WriteFile(Path.Combine(gamePath, "console.txt"), string.Empty);
                WriteFile(Path.Combine(gamePath, "developer.txt"), string.Empty);
                WriteFile(Path.Combine(gamePath, "publisher.txt"), string.Empty);
                WriteFile(Path.Combine(gamePath, "genre.txt"), string.Empty);
                WriteFile(Path.Combine(gamePath, "released.txt"), string.Empty);
                WriteFile(Path.Combine(gamePath, "current-set.txt"), string.Empty);
                WriteFile(Path.Combine(gamePath, "ratio.txt"), string.Empty);
                WriteFile(Path.Combine(gamePath, "points.txt"), string.Empty);
                WriteFile(Path.Combine(gamePath, "true-points.txt"), string.Empty);
                WriteFile(Path.Combine(gamePath, "achievements.txt"), string.Empty);
                WriteFile(Path.Combine(gamePath, "completed.txt"), string.Empty);
                WriteFile(Path.Combine(gamePath, "data.json"), "{}");

                // User info labels
                var userPath = Path.Combine(_basePath, "user-info");
                WriteFile(Path.Combine(userPath, "rank.txt"), string.Empty);
                WriteFile(Path.Combine(userPath, "ratio.txt"), string.Empty);
                WriteFile(Path.Combine(userPath, "points.txt"), string.Empty);
                WriteFile(Path.Combine(userPath, "true-points.txt"), string.Empty);
                WriteFile(Path.Combine(userPath, "data.json"), "{}");

                // Recent unlocks labels
                ClearRecentUnlocksLabels();

                System.Diagnostics.Debug.WriteLine("[StreamLabels] Cleared all stream labels");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StreamLabels] Error clearing all labels: {ex.Message}");
            }
        }
    }

    #endregion

    #region Helper Methods

    private void WriteFile(string path, string content)
    {
        try
        {
            // Ensure directory exists
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(path, content);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StreamLabels] Error writing file {path}: {ex.Message}");
        }
    }

    private string SerializeToJson(object obj)
    {
        try
        {
            return JsonSerializer.Serialize(obj, _jsonOptions);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[StreamLabels] Error serializing to JSON: {ex.Message}");
            return "{}";
        }
    }

    #endregion
}
