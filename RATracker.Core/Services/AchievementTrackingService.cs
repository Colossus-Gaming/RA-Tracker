using RATracker.Models;

namespace RATracker.WPF.Services;

/// <summary>
/// Service responsible for tracking achievement progress and detecting changes.
/// This is the WPF version that uses IProgressService for data fetching and
/// ProgressStateTracker for unlock detection.
/// </summary>
public class AchievementTrackingService : IDisposable
{
    /// <summary>
    /// DEBUG / TESTING override. When &gt; 0, the tracker pins itself to this RetroAchievements game
    /// ID every poll, ignoring whatever game the user is actually playing.
    /// <para>Always 0 in normal operation — nothing assigns it — so the tracker follows the most
    /// recently played game. Set it from <c>App.OnStartup</c> inside the <c>DEV_TOOLS</c> block to
    /// exercise overlays against a specific game without booting it, and remove that again before
    /// committing. Release builds cannot set it at all.</para>
    /// </summary>
    public static long DebugForceGameId;

    private readonly IProgressService _progressService;
    private readonly ProgressStateTracker _stateTracker;
    private readonly string _username;
    private bool _disposed;

    /// <summary>
    /// Gets the current user summary.
    /// </summary>
    public UserSummary? CurrentUser { get; private set; }

    /// <summary>
    /// Gets the current game progress.
    /// </summary>
    public UserGameProgress? CurrentProgress { get; private set; }

    /// <summary>
    /// Gets the current game info (for backwards compatibility with controllers).
    /// </summary>
    public GameInfo? CurrentGame => CurrentProgress != null 
        ? CreateGameInfoFromProgress(CurrentProgress) 
        : null;

    #region Events

    /// <summary>
    /// Fired when new achievements are unlocked.
    /// </summary>
    public event EventHandler<AchievementsUnlockedEventArgs>? AchievementsUnlocked;

    /// <summary>
    /// Fired when the player switches to a different game.
    /// </summary>
    public event EventHandler<GameChangedEventArgs>? GameChanged;

    /// <summary>
    /// Fired when all achievements in a game are unlocked.
    /// </summary>
    public event EventHandler<GameMasteredEventArgs>? GameMastered;

    /// <summary>
    /// Fired when user rank/points change.
    /// </summary>
    public event EventHandler<UserInfoUpdatedEventArgs>? UserInfoUpdated;

    /// <summary>
    /// Fired with status messages for UI display.
    /// </summary>
    public event EventHandler<PollingStatusEventArgs>? PollingStatusChanged;

    #endregion

    /// <summary>
    /// Creates a new AchievementTrackingService with the specified progress service.
    /// </summary>
    /// <param name="progressService">The progress service for API calls.</param>
    /// <param name="username">The username being tracked.</param>
    public AchievementTrackingService(IProgressService progressService, string username)
    {
        _progressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
        _username = username ?? throw new ArgumentNullException(nameof(username));
        _stateTracker = new ProgressStateTracker();
    }

    /// <summary>
    /// Gets the list of locked achievements for the current game.
    /// </summary>
    public List<Achievement> LockedAchievements
    {
        get
        {
            if (CurrentProgress?.Achievements != null)
            {
                return CurrentProgress.Achievements.FindAll(x => !x.DateEarned.HasValue);
            }
            return new List<Achievement>();
        }
    }

    /// <summary>
    /// Gets the list of unlocked achievements for the current game.
    /// </summary>
    public List<Achievement> UnlockedAchievements
    {
        get
        {
            if (CurrentProgress?.Achievements != null)
            {
                return CurrentProgress.Achievements.FindAll(x => x.DateEarned.HasValue);
            }
            return new List<Achievement>();
        }
    }

    /// <summary>
    /// Performs a full poll cycle to check for user and game updates.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The polling result indicating what was updated.</returns>
    public async Task<PollingResult> PollAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        var result = new PollingResult();

        try
        {
            // Fetch user summary if not yet loaded
            if (CurrentUser == null)
            {
                OnPollingStatusChanged("Updating user info...");
                Log("Fetching user summary...");
                CurrentUser = await _progressService.GetUserSummaryAsync(_username, cancellationToken);

                if (CurrentUser != null)
                {
                    result.UserUpdated = true;
                    OnUserInfoUpdated(CurrentUser);
                    Log($"User info loaded: {CurrentUser.UserName}, Rank #{CurrentUser.Rank}, LastGameID={CurrentUser.LastGameID}");
                }
                else
                {
                    Log("GetUserSummaryAsync returned null");
                }
            }

            if (CurrentUser == null)
            {
                Log("Cannot poll: user summary unavailable");
                result.Success = false;
                return result;
            }

            // Determine the current game from recently played games. (V2's user resource has no
            // lastGameId, so we rely on the most-recently-played entry rather than a guard on it.)
            var recentlyPlayed = await _progressService.GetUserRecentlyPlayedGamesAsync(_username, 10, cancellationToken);
            if (recentlyPlayed.Count == 0)
            {
                Log("No recently played games returned");
                result.Success = true;
                return result;
            }

            // The most recently played entry IS the current game — take it as-is.
            //
            // This used to skip entries reporting TotalAchievements=0, on the theory that a game
            // whose set has been demoted/unpublished has "no metadata to show". That premise is
            // wrong: such a game still returns its title, console, developer, publisher, release
            // date and all its artwork — only the achievement list is empty, which is accurate.
            // Skipping it meant the app silently tracked a DIFFERENT game than the one being
            // played, mis-attributing the dashboard, the overlays and any unlock alerts. Showing
            // the real game at 0/0 is both truthful and what the user expects.
            var pickedGame = recentlyPlayed[0];
            var currentGameId = pickedGame.GameId;

            // DEBUG / TESTING override (see DebugForceGameId): pin the tracked game for subset
            // testing, ignoring whatever the user is actually playing.
            if (DebugForceGameId > 0)
            {
                Log($"[DEBUG] Game selection hardcoded to {DebugForceGameId}; ignoring actual current game {currentGameId} ({pickedGame.Title})");
                currentGameId = DebugForceGameId;
            }

            bool isNewGame = CurrentProgress == null || currentGameId != CurrentProgress.GameId;
            if (DebugForceGameId <= 0)
            {
                Log($"Currently playing game ID {currentGameId} ({pickedGame.Title}), " +
                    $"{pickedGame.TotalAchievements} published achievement(s), isNewGame={isNewGame}");
            }

            // Check for recent achievements to detect if we need a full refresh
            var recentAchievements = await _progressService.GetUserRecentAchievementsAsync(_username, 10, cancellationToken);
            bool hasNewUnlocks = recentAchievements.Any(ra =>
                LockedAchievements.Any(la => la.Id == ra.AchievementId));

            if (isNewGame || hasNewUnlocks)
            {
                Log($"Fetching full game progress (isNewGame={isNewGame}, hasNewUnlocks={hasNewUnlocks})");
                OnPollingStatusChanged("Updating game info...");

                var newProgress = await _progressService.GetUserGameProgressAsync(
                    _username, 
                    currentGameId, 
                    includeAchievementSets: true, 
                    cancellationToken);

                if (newProgress != null)
                {
                    // Populate game context on achievements
                    foreach (var achievement in newProgress.Achievements)
                    {
                        achievement.GameId = (int)newProgress.GameId;
                        achievement.GameTitle = newProgress.GameTitle;
                    }

                    result.GameUpdated = true;

                    if (isNewGame)
                    {
                        // Initialize tracker for new game (no unlocks detected on first load)
                        _stateTracker.InitializeWithProgress(newProgress);
                        OnPollingStatusChanged($"Changing game to [{newProgress.GameTitle}]");
                        OnGameChanged(CreateGameInfoFromProgress(newProgress), isNewGame: true);
                        result.TriggeredNotifications = true;
                    }
                    else
                    {
                        // Detect changes for the same game
                        var detectionResult = _stateTracker.DetectChanges(newProgress);
                        
                        if (detectionResult.HasNewUnlocks)
                        {
                            OnPollingStatusChanged("CHEEVOS POP!");
                            
                            // Convert unlock events to achievements for the event
                            var unlockedAchievements = detectionResult.NewUnlocks
                                .Select(u => u.Achievement)
                                .ToList();
                            unlockedAchievements.Sort();
                            
                            OnAchievementsUnlocked(unlockedAchievements);
                            result.TriggeredNotifications = true;

                            // Check for mastery
                            if (detectionResult.JustMastered)
                            {
                                OnGameMastered(CreateGameInfoFromProgress(newProgress));
                            }

                            // Refresh user rank/score after unlocks
                            var updatedUser = await _progressService.GetUserSummaryAsync(_username, cancellationToken);
                            if (updatedUser != null)
                            {
                                CurrentUser = updatedUser;
                                result.UserUpdated = true;
                                OnUserInfoUpdated(CurrentUser);
                            }
                        }
                    }

                    CurrentProgress = newProgress;
                }
            }

            result.Success = CurrentProgress != null;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;

            if (ex.Message.Contains("RA backend") || ex.Message.Contains("API"))
            {
                OnPollingStatusChanged("API is down.");
            }
        }

        return result;
    }

    /// <summary>
    /// Fetches game info by ID (for manual search / offline mode).
    /// </summary>
    /// <param name="gameId">The game ID to fetch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The game info if found.</returns>
    public async Task<GameInfo?> GetGameByIdAsync(long gameId, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        var progress = await _progressService.GetUserGameProgressAsync(
            _username, 
            gameId, 
            includeAchievementSets: true, 
            cancellationToken);

        if (progress == null)
        {
            return null;
        }

        // Populate game context on achievements
        foreach (var achievement in progress.Achievements)
        {
            achievement.GameId = (int)progress.GameId;
            achievement.GameTitle = progress.GameTitle;
        }

        CurrentProgress = progress;
        _stateTracker.InitializeWithProgress(progress);

        var gameInfo = CreateGameInfoFromProgress(progress);
        OnGameChanged(gameInfo, isNewGame: true);

        return gameInfo;
    }

    /// <summary>
    /// Finds the next achievement to focus on based on the specified behavior.
    /// </summary>
    /// <param name="currentFocus">The currently focused achievement.</param>
    /// <param name="behavior">The refocus behavior to use.</param>
    /// <returns>The next achievement to focus on, or null if none available.</returns>
    public Achievement? FindNextFocus(Achievement? currentFocus, RefocusBehaviorEnum behavior)
    {
        ThrowIfDisposed();

        if (CurrentProgress?.Achievements == null || LockedAchievements.Count == 0)
            return null;

        var achievements = CurrentProgress.Achievements;
        int currentIndex = currentFocus != null
            ? achievements.FindIndex(a => a.Id == currentFocus.Id)
            : -1;

        switch (behavior)
        {
            case RefocusBehaviorEnum.GO_TO_FIRST:
                currentIndex = -1;
                break;

            case RefocusBehaviorEnum.GO_TO_PREVIOUS:
                while (currentIndex > 0 && !LockedAchievements.Any(a => a.Id == achievements[currentIndex].Id))
                    currentIndex--;
                if (currentIndex == 0)
                    while (currentIndex < achievements.Count - 1 && !LockedAchievements.Any(a => a.Id == achievements[currentIndex].Id))
                        currentIndex++;
                break;

            case RefocusBehaviorEnum.GO_TO_NEXT:
                while (currentIndex < achievements.Count - 1 && !LockedAchievements.Any(a => a.Id == achievements[currentIndex].Id))
                    currentIndex++;
                if (currentIndex == achievements.Count - 1)
                    while (currentIndex > 0 && !LockedAchievements.Any(a => a.Id == achievements[currentIndex].Id))
                        currentIndex--;
                break;

            case RefocusBehaviorEnum.GO_TO_LAST:
                currentIndex = achievements.Count;
                break;
        }

        // Normalize index to valid locked achievement
        if (currentIndex >= achievements.Count)
        {
            currentIndex = achievements.Count - 1;
            while (currentIndex > 0 && !LockedAchievements.Any(a => a.Id == achievements[currentIndex].Id))
                currentIndex--;
        }
        else if (currentIndex < 0)
        {
            currentIndex = 0;
            while (currentIndex < achievements.Count - 1 && !LockedAchievements.Any(a => a.Id == achievements[currentIndex].Id))
                currentIndex++;
        }

        return achievements[currentIndex];
    }

    /// <summary>
    /// Resets the tracking state (e.g., when stopping polling).
    /// </summary>
    public void Reset()
    {
        ThrowIfDisposed();
        
        _stateTracker.ResetAll();
        CurrentUser = null;
        CurrentProgress = null;
    }

    #region Event Triggers

    protected virtual void OnAchievementsUnlocked(List<Achievement> achievements)
    {
        AchievementsUnlocked?.Invoke(this, new AchievementsUnlockedEventArgs(achievements));
    }

    protected virtual void OnGameChanged(GameInfo game, bool isNewGame)
    {
        GameChanged?.Invoke(this, new GameChangedEventArgs(game, isNewGame));
    }

    protected virtual void OnGameMastered(GameInfo game)
    {
        GameMastered?.Invoke(this, new GameMasteredEventArgs(game));
    }

    protected virtual void OnUserInfoUpdated(UserSummary user)
    {
        UserInfoUpdated?.Invoke(this, new UserInfoUpdatedEventArgs(user));
    }

    protected virtual void OnPollingStatusChanged(string status)
    {
        PollingStatusChanged?.Invoke(this, new PollingStatusEventArgs(status));
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a GameInfo from UserGameProgress, grouping achievements into their sets
    /// (Core / Bonus / Specialty / Exclusive) using the per-achievement set membership
    /// populated during API mapping. When no set membership is available (e.g. the V1 API,
    /// or a single-set game), the achievements are kept as a flat list.
    /// </summary>
    private static GameInfo CreateGameInfoFromProgress(UserGameProgress progress)
    {
        var gameInfo = new GameInfo
        {
            Id = progress.GameId,
            Title = progress.GameTitle,
            // Setting ConsoleId triggers the v1 console-name lookup; the explicit name comes after.
            ConsoleId = progress.ConsoleId,
            BadgeUri = progress.BadgeUri,
            ImageBoxArt = progress.ImageBoxArt,
            ImageTitle = progress.ImageTitle,
            ImageIngame = progress.ImageIngame,
            Publisher = progress.Publisher,
            Developer = progress.Developer,
            Genre = progress.Genre,
            Released = progress.Released,
            LastPlayed = progress.LastPlayed
        };
        if (!string.IsNullOrEmpty(progress.ConsoleName))
        {
            gameInfo.ConsoleName = progress.ConsoleName;
        }

        var achievements = progress.Achievements ?? new List<Achievement>();
        var hasSetMembership = achievements.Any(a => a.SetId.HasValue);

        if (!hasSetMembership)
        {
            // No subset information — keep the legacy flat list.
            gameInfo.Achievements = achievements.ToList();
            return gameInfo;
        }

        // Set metadata (name/type) from per-set progress records, keyed by set id.
        var setMetadata = progress.AchievementSets
            .GroupBy(s => s.SetId)
            .ToDictionary(g => g.Key, g => g.First());

        var sets = new List<AchievementSet>();
        foreach (var group in achievements.Where(a => a.SetId.HasValue).GroupBy(a => a.SetId!.Value))
        {
            var sample = group.First();
            setMetadata.TryGetValue(group.Key, out var meta);

            sets.Add(new AchievementSet
            {
                Id = group.Key,
                GameId = progress.GameId,
                Name = meta?.SetName
                       ?? sample.SetName
                       ?? (sample.SetType == AchievementSetType.Core ? "Core" : sample.SetType.ToString()),
                SetType = meta?.SetType ?? sample.SetType,
                BadgeUrl = meta?.BadgeUrl ?? string.Empty,
                // Sort by DisplayOrder so the "first locked" (the Focus achievement) is deterministic
                // regardless of API response order; ties fall back to id.
                Achievements = group.OrderBy(a => a.DisplayOrder).ThenBy(a => a.Id).ToList()
            });
        }

        // Achievements without a set id are treated as Core.
        var untagged = achievements.Where(a => !a.SetId.HasValue).ToList();
        if (untagged.Count > 0)
        {
            var core = sets.FirstOrDefault(s => s.SetType == AchievementSetType.Core);
            if (core != null)
            {
                core.Achievements.AddRange(untagged);
                core.Achievements = core.Achievements.OrderBy(a => a.DisplayOrder).ThenBy(a => a.Id).ToList();
            }
            else
            {
                sets.Add(new AchievementSet
                {
                    GameId = progress.GameId,
                    Name = "Core",
                    SetType = AchievementSetType.Core,
                    Achievements = untagged.OrderBy(a => a.DisplayOrder).ThenBy(a => a.Id).ToList()
                });
            }
        }

        // Order: Core first, then Bonus / Specialty / Exclusive, then by id.
        gameInfo.AchievementSets = sets
            .OrderBy(s => s.SetType)
            .ThenBy(s => s.Id)
            .ToList();

        return gameInfo;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(AchievementTrackingService));
        }
    }

    private static void Log(string message)
    {
        System.Diagnostics.Debug.WriteLine($"[AchievementTrackingService] {message}");
    }

    #endregion

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_progressService is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _disposed = true;
        }
    }
}

#region Event Args Classes

/// <summary>
/// Event arguments for achievements unlocked event.
/// </summary>
public class AchievementsUnlockedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the list of unlocked achievements.
    /// </summary>
    public List<Achievement> Achievements { get; }

    /// <summary>
    /// Creates new event arguments.
    /// </summary>
    /// <param name="achievements">The unlocked achievements.</param>
    public AchievementsUnlockedEventArgs(List<Achievement> achievements)
    {
        Achievements = achievements;
    }
}

/// <summary>
/// Event arguments for game changed event.
/// </summary>
public class GameChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the game that was changed to.
    /// </summary>
    public GameInfo Game { get; }

    /// <summary>
    /// Gets whether this is a new game (vs. same game refresh).
    /// </summary>
    public bool IsNewGame { get; }

    /// <summary>
    /// Creates new event arguments.
    /// </summary>
    /// <param name="game">The game.</param>
    /// <param name="isNewGame">Whether it's a new game.</param>
    public GameChangedEventArgs(GameInfo game, bool isNewGame)
    {
        Game = game;
        IsNewGame = isNewGame;
    }
}

/// <summary>
/// Event arguments for game mastered event.
/// </summary>
public class GameMasteredEventArgs : EventArgs
{
    /// <summary>
    /// Gets the mastered game.
    /// </summary>
    public GameInfo Game { get; }

    /// <summary>
    /// Creates new event arguments.
    /// </summary>
    /// <param name="game">The mastered game.</param>
    public GameMasteredEventArgs(GameInfo game)
    {
        Game = game;
    }
}

/// <summary>
/// Event arguments for user info updated event.
/// </summary>
public class UserInfoUpdatedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the updated user summary.
    /// </summary>
    public UserSummary User { get; }

    /// <summary>
    /// Creates new event arguments.
    /// </summary>
    /// <param name="user">The user summary.</param>
    public UserInfoUpdatedEventArgs(UserSummary user)
    {
        User = user;
    }
}

/// <summary>
/// Event arguments for polling status changed event.
/// </summary>
public class PollingStatusEventArgs : EventArgs
{
    /// <summary>
    /// Gets the status message.
    /// </summary>
    public string Status { get; }

    /// <summary>
    /// Creates new event arguments.
    /// </summary>
    /// <param name="status">The status message.</param>
    public PollingStatusEventArgs(string status)
    {
        Status = status;
    }
}

/// <summary>
/// Result of a polling operation.
/// </summary>
public class PollingResult
{
    /// <summary>
    /// Gets or sets whether the polling completed successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets whether user info was updated.
    /// </summary>
    public bool UserUpdated { get; set; }

    /// <summary>
    /// Gets or sets whether game info was updated.
    /// </summary>
    public bool GameUpdated { get; set; }

    /// <summary>
    /// Gets or sets whether notifications were triggered (new unlocks or game change).
    /// </summary>
    public bool TriggeredNotifications { get; set; }

    /// <summary>
    /// Gets or sets the error message if polling failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}

#endregion

/// <summary>
/// Behavior for refocusing when the current focused achievement is unlocked.
/// </summary>
public enum RefocusBehaviorEnum
{
    /// <summary>
    /// Go to the first locked achievement in the list.
    /// </summary>
    GO_TO_FIRST,

    /// <summary>
    /// Go to the previous locked achievement (or next if at beginning).
    /// </summary>
    GO_TO_PREVIOUS,

    /// <summary>
    /// Go to the next locked achievement (or previous if at end).
    /// </summary>
    GO_TO_NEXT,

    /// <summary>
    /// Go to the last locked achievement in the list.
    /// </summary>
    GO_TO_LAST
}
