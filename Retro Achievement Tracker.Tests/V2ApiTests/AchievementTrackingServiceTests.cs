using Moq;
using RATracker.Models;
using RATracker.WPF.Services;

namespace RATracker.Tests.V2ApiTests;

/// <summary>
/// Tests for the WPF AchievementTrackingService.
/// </summary>
[TestFixture]
public class AchievementTrackingServiceTests
{
    private Mock<IProgressService> _mockProgressService = null!;
    private AchievementTrackingService _trackingService = null!;

    private const string TestUsername = "TestUser";

    [SetUp]
    public void SetUp()
    {
        _mockProgressService = new Mock<IProgressService>();
        _trackingService = new AchievementTrackingService(_mockProgressService.Object, TestUsername);
    }

    [TearDown]
    public void TearDown()
    {
        _trackingService.Dispose();
    }

    #region Constructor Tests

    [Test]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        using var service = new AchievementTrackingService(_mockProgressService.Object, TestUsername);
        
        Assert.That(service, Is.Not.Null);
    }

    [Test]
    public void Constructor_WithNullProgressService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new AchievementTrackingService(null!, TestUsername));
    }

    [Test]
    public void Constructor_WithNullUsername_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new AchievementTrackingService(_mockProgressService.Object, null!));
    }

    #endregion

    #region Initial State Tests

    [Test]
    public void InitialState_CurrentUser_IsNull()
    {
        Assert.That(_trackingService.CurrentUser, Is.Null);
    }

    [Test]
    public void InitialState_CurrentProgress_IsNull()
    {
        Assert.That(_trackingService.CurrentProgress, Is.Null);
    }

    [Test]
    public void InitialState_CurrentGame_IsNull()
    {
        Assert.That(_trackingService.CurrentGame, Is.Null);
    }

    [Test]
    public void InitialState_LockedAchievements_IsEmpty()
    {
        Assert.That(_trackingService.LockedAchievements, Is.Empty);
    }

    [Test]
    public void InitialState_UnlockedAchievements_IsEmpty()
    {
        Assert.That(_trackingService.UnlockedAchievements, Is.Empty);
    }

    #endregion

    #region PollAsync Tests

    [Test]
    public async Task PollAsync_WhenUserSummaryNull_ReturnsFailed()
    {
        _mockProgressService
            .Setup(s => s.GetUserSummaryAsync(TestUsername, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSummary?)null);

        var result = await _trackingService.PollAsync();

        Assert.That(result.Success, Is.False);
    }

    [Test]
    public async Task PollAsync_WhenUserSummaryReturned_FiresUserInfoUpdated()
    {
        var userSummary = new UserSummary
        {
            UserName = TestUsername,
            Rank = 1000,
            TotalPoints = 5000,
            LastGameID = 1234
        };

        UserSummary? receivedUser = null;
        _trackingService.UserInfoUpdated += (s, e) => receivedUser = e.User;

        _mockProgressService
            .Setup(s => s.GetUserSummaryAsync(TestUsername, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userSummary);

        _mockProgressService
            .Setup(s => s.GetUserRecentlyPlayedGamesAsync(TestUsername, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RecentlyPlayedGame>());

        await _trackingService.PollAsync();

        Assert.That(receivedUser, Is.Not.Null);
        Assert.That(receivedUser?.UserName, Is.EqualTo(TestUsername));
    }

    [Test]
    public async Task PollAsync_WhenNoRecentlyPlayedGames_ReturnsSuccess()
    {
        SetupBasicUserSummary();

        _mockProgressService
            .Setup(s => s.GetUserRecentlyPlayedGamesAsync(TestUsername, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RecentlyPlayedGame>());

        var result = await _trackingService.PollAsync();

        Assert.That(result.Success, Is.True);
    }

    [Test]
    public async Task PollAsync_WhenNewGameDetected_FiresGameChanged()
    {
        SetupBasicUserSummary();
        SetupRecentlyPlayedGame(1234, "Test Game");
        SetupGameProgress(1234, "Test Game", new List<Achievement>
        {
            new Achievement { Id = 1, Title = "Achievement 1", DateEarned = null }
        });

        GameInfo? receivedGame = null;
        _trackingService.GameChanged += (s, e) => receivedGame = e.Game;

        await _trackingService.PollAsync();

        Assert.Multiple(() =>
        {
            Assert.That(receivedGame, Is.Not.Null);
            Assert.That(receivedGame?.Id, Is.EqualTo(1234));
            Assert.That(receivedGame?.Title, Is.EqualTo("Test Game"));
        });
    }

    [Test]
    public async Task PollAsync_WhenNewAchievementsUnlocked_FiresAchievementsUnlocked()
    {
        // First poll - set up initial state
        SetupBasicUserSummary();
        SetupRecentlyPlayedGame(1234, "Test Game");
        SetupGameProgress(1234, "Test Game", new List<Achievement>
        {
            new Achievement { Id = 1, Title = "Achievement 1", DateEarned = DateTime.Now },
            new Achievement { Id = 2, Title = "Achievement 2", DateEarned = null }
        });

        await _trackingService.PollAsync();

        // Reset recent achievements to show a new unlock
        _mockProgressService
            .Setup(s => s.GetUserRecentAchievementsAsync(TestUsername, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RecentAchievement>
            {
                new RecentAchievement { AchievementId = 2, Title = "Achievement 2" }
            });

        // Update progress to show achievement 2 now unlocked
        SetupGameProgress(1234, "Test Game", new List<Achievement>
        {
            new Achievement { Id = 1, Title = "Achievement 1", DateEarned = DateTime.Now },
            new Achievement { Id = 2, Title = "Achievement 2", DateEarned = DateTime.Now }
        });

        List<Achievement>? unlockedAchievements = null;
        _trackingService.AchievementsUnlocked += (s, e) => unlockedAchievements = e.Achievements;

        await _trackingService.PollAsync();

        Assert.Multiple(() =>
        {
            Assert.That(unlockedAchievements, Is.Not.Null);
            Assert.That(unlockedAchievements, Has.Count.EqualTo(1));
            Assert.That(unlockedAchievements?[0].Id, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task PollAsync_WhenGameMastered_FiresGameMastered()
    {
        // First poll - set up initial state with one locked
        SetupBasicUserSummary();
        SetupRecentlyPlayedGame(1234, "Test Game");
        SetupGameProgress(1234, "Test Game", new List<Achievement>
        {
            new Achievement { Id = 1, Title = "Achievement 1", DateEarned = DateTime.Now },
            new Achievement { Id = 2, Title = "Achievement 2", DateEarned = null }
        });

        await _trackingService.PollAsync();

        // Setup for mastery
        _mockProgressService
            .Setup(s => s.GetUserRecentAchievementsAsync(TestUsername, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RecentAchievement>
            {
                new RecentAchievement { AchievementId = 2, Title = "Achievement 2" }
            });

        SetupGameProgress(1234, "Test Game", new List<Achievement>
        {
            new Achievement { Id = 1, Title = "Achievement 1", DateEarned = DateTime.Now },
            new Achievement { Id = 2, Title = "Achievement 2", DateEarned = DateTime.Now }
        });

        GameInfo? masteredGame = null;
        _trackingService.GameMastered += (s, e) => masteredGame = e.Game;

        await _trackingService.PollAsync();

        Assert.Multiple(() =>
        {
            Assert.That(masteredGame, Is.Not.Null);
            Assert.That(masteredGame?.Id, Is.EqualTo(1234));
        });
    }

    [Test]
    public async Task PollAsync_FiresPollingStatusChanged()
    {
        SetupBasicUserSummary();
        SetupRecentlyPlayedGame(1234, "Test Game");
        SetupGameProgress(1234, "Test Game", new List<Achievement>());

        var statusMessages = new List<string>();
        _trackingService.PollingStatusChanged += (s, e) => statusMessages.Add(e.Status);

        await _trackingService.PollAsync();

        Assert.That(statusMessages, Has.Count.GreaterThan(0));
    }

    [Test]
    public async Task PollAsync_WhenException_SetsErrorMessage()
    {
        _mockProgressService
            .Setup(s => s.GetUserSummaryAsync(TestUsername, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test error"));

        var result = await _trackingService.PollAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Test error"));
        });
    }

    #endregion

    #region GetGameByIdAsync Tests

    [Test]
    public async Task GetGameByIdAsync_ReturnsGameInfo()
    {
        SetupGameProgress(1234, "Test Game", new List<Achievement>
        {
            new Achievement { Id = 1, Title = "Achievement 1" }
        });

        var game = await _trackingService.GetGameByIdAsync(1234);

        Assert.Multiple(() =>
        {
            Assert.That(game, Is.Not.Null);
            Assert.That(game?.Id, Is.EqualTo(1234));
            Assert.That(game?.Title, Is.EqualTo("Test Game"));
        });
    }

    [Test]
    public async Task GetGameByIdAsync_WhenProgressNull_ReturnsNull()
    {
        _mockProgressService
            .Setup(s => s.GetUserGameProgressAsync(TestUsername, 1234, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserGameProgress?)null);

        var game = await _trackingService.GetGameByIdAsync(1234);

        Assert.That(game, Is.Null);
    }

    [Test]
    public async Task GetGameByIdAsync_FiresGameChanged()
    {
        SetupGameProgress(1234, "Test Game", new List<Achievement>());

        GameInfo? receivedGame = null;
        _trackingService.GameChanged += (s, e) => receivedGame = e.Game;

        await _trackingService.GetGameByIdAsync(1234);

        Assert.That(receivedGame, Is.Not.Null);
    }

    #endregion

    #region FindNextFocus Tests

    [Test]
    public async Task FindNextFocus_GoToFirst_ReturnsFirstLocked()
    {
        await SetupServiceWithAchievements(new List<Achievement>
        {
            new Achievement { Id = 1, Title = "A1", DateEarned = DateTime.Now },
            new Achievement { Id = 2, Title = "A2", DateEarned = null },
            new Achievement { Id = 3, Title = "A3", DateEarned = null }
        });

        var currentFocus = _trackingService.LockedAchievements.Last();
        var nextFocus = _trackingService.FindNextFocus(currentFocus, RefocusBehaviorEnum.GO_TO_FIRST);

        Assert.That(nextFocus?.Id, Is.EqualTo(2));
    }

    [Test]
    public async Task FindNextFocus_GoToLast_ReturnsLastLocked()
    {
        await SetupServiceWithAchievements(new List<Achievement>
        {
            new Achievement { Id = 1, Title = "A1", DateEarned = DateTime.Now },
            new Achievement { Id = 2, Title = "A2", DateEarned = null },
            new Achievement { Id = 3, Title = "A3", DateEarned = null }
        });

        var currentFocus = _trackingService.LockedAchievements.First();
        var nextFocus = _trackingService.FindNextFocus(currentFocus, RefocusBehaviorEnum.GO_TO_LAST);

        Assert.That(nextFocus?.Id, Is.EqualTo(3));
    }

    [Test]
    public async Task FindNextFocus_WhenNoLockedAchievements_ReturnsNull()
    {
        await SetupServiceWithAchievements(new List<Achievement>
        {
            new Achievement { Id = 1, Title = "A1", DateEarned = DateTime.Now },
            new Achievement { Id = 2, Title = "A2", DateEarned = DateTime.Now }
        });

        var nextFocus = _trackingService.FindNextFocus(null, RefocusBehaviorEnum.GO_TO_FIRST);

        Assert.That(nextFocus, Is.Null);
    }

    [Test]
    public async Task FindNextFocus_WhenNoProgress_ReturnsNull()
    {
        // Don't set up any progress
        var nextFocus = _trackingService.FindNextFocus(null, RefocusBehaviorEnum.GO_TO_FIRST);

        Assert.That(nextFocus, Is.Null);
    }

    #endregion

    #region Reset Tests

    [Test]
    public async Task Reset_ClearsCurrentState()
    {
        SetupBasicUserSummary();
        SetupRecentlyPlayedGame(1234, "Test Game");
        SetupGameProgress(1234, "Test Game", new List<Achievement>());

        await _trackingService.PollAsync();

        Assert.That(_trackingService.CurrentProgress, Is.Not.Null);

        _trackingService.Reset();

        Assert.Multiple(() =>
        {
            Assert.That(_trackingService.CurrentUser, Is.Null);
            Assert.That(_trackingService.CurrentProgress, Is.Null);
            Assert.That(_trackingService.CurrentGame, Is.Null);
        });
    }

    #endregion

    #region Dispose Tests

    [Test]
    public void Dispose_AfterDispose_ThrowsOnPoll()
    {
        _trackingService.Dispose();

        Assert.ThrowsAsync<ObjectDisposedException>(() => _trackingService.PollAsync());
    }

    [Test]
    public void Dispose_AfterDispose_ThrowsOnGetGameById()
    {
        _trackingService.Dispose();

        Assert.ThrowsAsync<ObjectDisposedException>(() => _trackingService.GetGameByIdAsync(1234));
    }

    [Test]
    public void Dispose_AfterDispose_ThrowsOnFindNextFocus()
    {
        _trackingService.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _trackingService.FindNextFocus(null, RefocusBehaviorEnum.GO_TO_FIRST));
    }

    [Test]
    public void Dispose_AfterDispose_ThrowsOnReset()
    {
        _trackingService.Dispose();

        Assert.Throws<ObjectDisposedException>(() => _trackingService.Reset());
    }

    #endregion

    #region LockedAchievements and UnlockedAchievements Tests

    [Test]
    public async Task LockedAchievements_ReturnsOnlyLocked()
    {
        await SetupServiceWithAchievements(new List<Achievement>
        {
            new Achievement { Id = 1, Title = "A1", DateEarned = DateTime.Now },
            new Achievement { Id = 2, Title = "A2", DateEarned = null },
            new Achievement { Id = 3, Title = "A3", DateEarned = null }
        });

        var locked = _trackingService.LockedAchievements;

        Assert.Multiple(() =>
        {
            Assert.That(locked, Has.Count.EqualTo(2));
            Assert.That(locked.All(a => !a.DateEarned.HasValue), Is.True);
        });
    }

    [Test]
    public async Task UnlockedAchievements_ReturnsOnlyUnlocked()
    {
        await SetupServiceWithAchievements(new List<Achievement>
        {
            new Achievement { Id = 1, Title = "A1", DateEarned = DateTime.Now },
            new Achievement { Id = 2, Title = "A2", DateEarned = null },
            new Achievement { Id = 3, Title = "A3", DateEarned = DateTime.Now }
        });

        var unlocked = _trackingService.UnlockedAchievements;

        Assert.Multiple(() =>
        {
            Assert.That(unlocked, Has.Count.EqualTo(2));
            Assert.That(unlocked.All(a => a.DateEarned.HasValue), Is.True);
        });
    }

    #endregion

    #region Event Args Tests

    [Test]
    public void AchievementsUnlockedEventArgs_StoresAchievements()
    {
        var achievements = new List<Achievement>
        {
            new Achievement { Id = 1, Title = "Test" }
        };

        var args = new AchievementsUnlockedEventArgs(achievements);

        Assert.That(args.Achievements, Is.SameAs(achievements));
    }

    [Test]
    public void GameChangedEventArgs_StoresGameAndIsNewGame()
    {
        var game = new GameInfo { Id = 1234, Title = "Test Game" };
        var args = new GameChangedEventArgs(game, isNewGame: true);

        Assert.Multiple(() =>
        {
            Assert.That(args.Game, Is.SameAs(game));
            Assert.That(args.IsNewGame, Is.True);
        });
    }

    [Test]
    public void GameMasteredEventArgs_StoresGame()
    {
        var game = new GameInfo { Id = 1234, Title = "Test Game" };
        var args = new GameMasteredEventArgs(game);

        Assert.That(args.Game, Is.SameAs(game));
    }

    [Test]
    public void UserInfoUpdatedEventArgs_StoresUser()
    {
        var user = new UserSummary { UserName = "TestUser" };
        var args = new UserInfoUpdatedEventArgs(user);

        Assert.That(args.User, Is.SameAs(user));
    }

    [Test]
    public void PollingStatusEventArgs_StoresStatus()
    {
        var args = new PollingStatusEventArgs("Test Status");

        Assert.That(args.Status, Is.EqualTo("Test Status"));
    }

    [Test]
    public void PollingResult_DefaultValues_AreCorrect()
    {
        var result = new PollingResult();

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.UserUpdated, Is.False);
            Assert.That(result.GameUpdated, Is.False);
            Assert.That(result.TriggeredNotifications, Is.False);
            Assert.That(result.ErrorMessage, Is.Null);
        });
    }

    #endregion

    #region RefocusBehaviorEnum Tests

    [Test]
    public void RefocusBehaviorEnum_HasExpectedValues()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Enum.IsDefined(typeof(RefocusBehaviorEnum), RefocusBehaviorEnum.GO_TO_FIRST), Is.True);
            Assert.That(Enum.IsDefined(typeof(RefocusBehaviorEnum), RefocusBehaviorEnum.GO_TO_PREVIOUS), Is.True);
            Assert.That(Enum.IsDefined(typeof(RefocusBehaviorEnum), RefocusBehaviorEnum.GO_TO_NEXT), Is.True);
            Assert.That(Enum.IsDefined(typeof(RefocusBehaviorEnum), RefocusBehaviorEnum.GO_TO_LAST), Is.True);
        });
    }

    #endregion

    #region Helper Methods

    private void SetupBasicUserSummary()
    {
        var userSummary = new UserSummary
        {
            UserName = TestUsername,
            Rank = 1000,
            TotalPoints = 5000,
            LastGameID = 1234
        };

        _mockProgressService
            .Setup(s => s.GetUserSummaryAsync(TestUsername, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userSummary);
    }

    private void SetupRecentlyPlayedGame(long gameId, string title)
    {
        _mockProgressService
            .Setup(s => s.GetUserRecentlyPlayedGamesAsync(TestUsername, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RecentlyPlayedGame>
            {
                new RecentlyPlayedGame
                {
                    GameId = gameId,
                    Title = title,
                    ConsoleName = "Genesis"
                }
            });

        // Default empty recent achievements
        _mockProgressService
            .Setup(s => s.GetUserRecentAchievementsAsync(TestUsername, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RecentAchievement>());
    }

    private void SetupGameProgress(long gameId, string title, List<Achievement> achievements)
    {
        var progress = new UserGameProgress
        {
            UserId = TestUsername,
            GameId = gameId,
            GameTitle = title,
            ConsoleName = "Genesis",
            TotalAchievements = achievements.Count,
            EarnedAchievements = achievements.Count(a => a.DateEarned.HasValue),
            Achievements = achievements
        };

        _mockProgressService
            .Setup(s => s.GetUserGameProgressAsync(TestUsername, gameId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(progress);
    }

    private async Task SetupServiceWithAchievements(List<Achievement> achievements)
    {
        SetupGameProgress(1234, "Test Game", achievements);

        // Call GetGameByIdAsync to set up internal state
        await _trackingService.GetGameByIdAsync(1234);
    }

    #endregion
}
