using Retro_Achievement_Tracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Retro_Achievement_Tracker.Services
{
    /// <summary>
    /// Service responsible for tracking achievement progress and detecting changes.
    /// This class is designed to be testable independently of the UI.
    /// </summary>
    public class AchievementTrackingService
    {
        private readonly RetroAchievementAPIClient _apiClient;
        private List<Achievement> _previousUnlockedAchievements = new List<Achievement>();
        private int _maxUnlockedCount;

        public UserSummary CurrentUser { get; private set; }
        public GameInfo CurrentGame { get; private set; }

        public event EventHandler<AchievementsUnlockedEventArgs> AchievementsUnlocked;
        public event EventHandler<GameChangedEventArgs> GameChanged;
        public event EventHandler<GameMasteredEventArgs> GameMastered;
        public event EventHandler<UserInfoUpdatedEventArgs> UserInfoUpdated;
        public event EventHandler<PollingStatusEventArgs> PollingStatusChanged;

        public AchievementTrackingService(string username, string apiKey)
        {
            _apiClient = new RetroAchievementAPIClient(username, apiKey);
        }

        /// <summary>
        /// Gets the list of locked achievements for the current game.
        /// </summary>
        public List<Achievement> LockedAchievements
        {
            get
            {
                if (CurrentGame?.Achievements != null)
                {
                    return CurrentGame.Achievements.FindAll(x => !x.DateEarned.HasValue);
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
                if (CurrentGame?.Achievements != null)
                {
                    return CurrentGame.Achievements.FindAll(x => x.DateEarned.HasValue);
                }
                return new List<Achievement>();
            }
        }

        /// <summary>
        /// Performs a full poll cycle to check for user and game updates.
        /// </summary>
        /// <returns>True if updates were detected and processed.</returns>
        public async Task<PollingResult> PollAsync()
        {
            var result = new PollingResult();

            try
            {
                // Fetch user summary if not yet loaded
                if (CurrentUser == null)
                {
                    OnPollingStatusChanged("Updating user info...");
                    CurrentUser = await _apiClient.GetUserSummary();
                    result.UserUpdated = true;
                    OnUserInfoUpdated(CurrentUser);
                }

                if (CurrentUser == null || CurrentUser.LastGameID <= 0)
                {
                    result.Success = false;
                    return result;
                }

                // Check recently played games
                var recentlyPlayed = await _apiClient.GetRecentlyPlayedGames();
                if (recentlyPlayed.Count == 0)
                {
                    result.Success = true;
                    return result;
                }

                var recentAchievements = await _apiClient.GetRecentAchievements();
                bool isNewGame = CurrentGame == null || !recentlyPlayed[0].Id.Equals(CurrentGame.Id);
                bool hasNewUnlocks = recentAchievements.Any(x => LockedAchievements.Contains(x));

                if (isNewGame || hasNewUnlocks)
                {
                    OnPollingStatusChanged("Updating game info...");
                    var previousGame = CurrentGame;
                    CurrentGame = await _apiClient.GetGameInfoAndProgress(recentlyPlayed[0].Id);

                    // Populate game context on achievements
                    CurrentGame.Achievements?.ForEach(achievement =>
                    {
                        achievement.GameId = (int)CurrentGame.Id;
                        achievement.GameTitle = CurrentGame.Title;
                    });

                    result.GameUpdated = true;
                    result.TriggeredNotifications = ProcessGameProgress(!isNewGame, previousGame);

                    if (result.TriggeredNotifications)
                    {
                        // Refresh user rank/score after unlocks
                        var rankAndScore = await _apiClient.GetRankAndScore();
                        CurrentUser.Rank = rankAndScore.Rank;
                        CurrentUser.TotalPoints = rankAndScore.Score;
                        result.UserUpdated = true;
                        OnUserInfoUpdated(CurrentUser);
                    }
                }

                result.Success = CurrentGame != null;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;

                if (ex.Message.Contains("RA backend"))
                {
                    OnPollingStatusChanged("API is down.");
                }
            }

            return result;
        }

        /// <summary>
        /// Fetches game info by ID (for manual search / offline mode).
        /// </summary>
        public async Task<GameInfo> GetGameByIdAsync(long gameId)
        {
            var gameInfo = await _apiClient.GetGameInfoExtended(gameId);

            gameInfo.Achievements?.ForEach(achievement =>
            {
                achievement.GameId = (int)gameInfo.Id;
                achievement.GameTitle = gameInfo.Title;
            });

            CurrentGame = gameInfo;
            _previousUnlockedAchievements = UnlockedAchievements.ToList();
            _maxUnlockedCount = UnlockedAchievements.Count;

            OnGameChanged(gameInfo, isNewGame: true);

            return gameInfo;
        }

        /// <summary>
        /// Processes game progress and detects new achievements.
        /// </summary>
        /// <param name="sameGame">True if we're checking the same game as before.</param>
        /// <param name="previousGame">The previous game info (for game change detection).</param>
        /// <returns>True if notifications were triggered.</returns>
        private bool ProcessGameProgress(bool sameGame, GameInfo previousGame)
        {
            bool triggeredNotifications = false;

            if (sameGame)
            {
                // Detect newly unlocked achievements
                var newlyUnlocked = UnlockedAchievements
                    .FindAll(a => !_previousUnlockedAchievements.Contains(a))
                    .ToList();

                if (newlyUnlocked.Count > 0 && UnlockedAchievements.Count > _maxUnlockedCount)
                {
                    _maxUnlockedCount = UnlockedAchievements.Count;
                    newlyUnlocked.Sort();

                    OnPollingStatusChanged("CHEEVOS POP!");
                    OnAchievementsUnlocked(newlyUnlocked);
                    triggeredNotifications = true;

                    // Check for mastery
                    if (UnlockedAchievements.Count == CurrentGame.Achievements.Count &&
                        _previousUnlockedAchievements.Count < CurrentGame.Achievements.Count)
                    {
                        OnGameMastered(CurrentGame);
                    }
                }
            }
            else
            {
                // New game detected
                _maxUnlockedCount = UnlockedAchievements.Count;
                OnPollingStatusChanged($"Changing game to [{CurrentGame.Title}]");
                OnGameChanged(CurrentGame, isNewGame: true);
                triggeredNotifications = true;
            }

            _previousUnlockedAchievements = UnlockedAchievements.ToList();
            return triggeredNotifications;
        }

        /// <summary>
        /// Finds the next achievement to focus on based on the specified behavior.
        /// </summary>
        public Achievement FindNextFocus(Achievement currentFocus, RefocusBehaviorEnum behavior)
        {
            if (CurrentGame?.Achievements == null || LockedAchievements.Count == 0)
                return null;

            int currentIndex = currentFocus != null
                ? CurrentGame.Achievements.IndexOf(currentFocus)
                : -1;

            switch (behavior)
            {
                case RefocusBehaviorEnum.GO_TO_FIRST:
                    currentIndex = -1;
                    break;

                case RefocusBehaviorEnum.GO_TO_PREVIOUS:
                    while (currentIndex > 0 && !LockedAchievements.Contains(CurrentGame.Achievements[currentIndex]))
                        currentIndex--;
                    if (currentIndex == 0)
                        while (currentIndex < CurrentGame.Achievements.Count - 1 && !LockedAchievements.Contains(CurrentGame.Achievements[currentIndex]))
                            currentIndex++;
                    break;

                case RefocusBehaviorEnum.GO_TO_NEXT:
                    while (currentIndex < CurrentGame.Achievements.Count - 1 && !LockedAchievements.Contains(CurrentGame.Achievements[currentIndex]))
                        currentIndex++;
                    if (currentIndex == CurrentGame.Achievements.Count - 1)
                        while (currentIndex > 0 && !LockedAchievements.Contains(CurrentGame.Achievements[currentIndex]))
                            currentIndex--;
                    break;

                case RefocusBehaviorEnum.GO_TO_LAST:
                    currentIndex = CurrentGame.Achievements.Count;
                    break;
            }

            // Normalize index to valid locked achievement
            if (currentIndex >= CurrentGame.Achievements.Count)
            {
                currentIndex = CurrentGame.Achievements.Count - 1;
                while (currentIndex > 0 && !LockedAchievements.Contains(CurrentGame.Achievements[currentIndex]))
                    currentIndex--;
            }
            else if (currentIndex < 0)
            {
                currentIndex = 0;
                while (currentIndex < CurrentGame.Achievements.Count - 1 && !LockedAchievements.Contains(CurrentGame.Achievements[currentIndex]))
                    currentIndex++;
            }

            return CurrentGame.Achievements[currentIndex];
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
    }

    #region Event Args Classes

    public class AchievementsUnlockedEventArgs : EventArgs
    {
        public List<Achievement> Achievements { get; }

        public AchievementsUnlockedEventArgs(List<Achievement> achievements)
        {
            Achievements = achievements;
        }
    }

    public class GameChangedEventArgs : EventArgs
    {
        public GameInfo Game { get; }
        public bool IsNewGame { get; }

        public GameChangedEventArgs(GameInfo game, bool isNewGame)
        {
            Game = game;
            IsNewGame = isNewGame;
        }
    }

    public class GameMasteredEventArgs : EventArgs
    {
        public GameInfo Game { get; }

        public GameMasteredEventArgs(GameInfo game)
        {
            Game = game;
        }
    }

    public class UserInfoUpdatedEventArgs : EventArgs
    {
        public UserSummary User { get; }

        public UserInfoUpdatedEventArgs(UserSummary user)
        {
            User = user;
        }
    }

    public class PollingStatusEventArgs : EventArgs
    {
        public string Status { get; }

        public PollingStatusEventArgs(string status)
        {
            Status = status;
        }
    }

    public class PollingResult
    {
        public bool Success { get; set; }
        public bool UserUpdated { get; set; }
        public bool GameUpdated { get; set; }
        public bool TriggeredNotifications { get; set; }
        public string ErrorMessage { get; set; }
    }

    #endregion
}
