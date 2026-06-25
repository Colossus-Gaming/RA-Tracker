using RATracker.Models;
using RATracker.WPF.ViewModels;

namespace RATracker.Tests.ViewModelTests;

/// <summary>
/// Tests for multi-set achievement functionality in MainViewModel.
/// </summary>
[TestFixture]
public class MainViewModelMultiSetTests
{
    private MainViewModel _viewModel = null!;

    [SetUp]
    public void SetUp()
    {
        // Skip placeholder sample data so each test starts from a clean, deterministic state.
        _viewModel = new MainViewModel(loadSampleData: false);
    }

    #region UpdateAvailableAchievementSets Tests

    [Test]
    public void CurrentGame_WithMultipleSets_HasMultipleSetsReturnsTrue()
    {
        // Arrange
        var game = CreateMultiSetGame();

        // Act
        _viewModel.CurrentGame = game;

        // Assert
        Assert.That(_viewModel.HasMultipleSets, Is.True);
    }

    [Test]
    public void CurrentGame_WithSingleSet_HasMultipleSetsReturnsFalse()
    {
        // Arrange
        var game = CreateSingleSetGame();

        // Act
        _viewModel.CurrentGame = game;

        // Assert
        Assert.That(_viewModel.HasMultipleSets, Is.False);
    }

    [Test]
    public void CurrentGame_WithNullGame_HasMultipleSetsReturnsFalse()
    {
        // Act
        _viewModel.CurrentGame = null;

        // Assert
        Assert.That(_viewModel.HasMultipleSets, Is.False);
    }

    [Test]
    public void CurrentGame_WithMultipleSets_PopulatesAvailableAchievementSets()
    {
        // Arrange
        var game = CreateMultiSetGame();

        // Act
        _viewModel.CurrentGame = game;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_viewModel.AvailableAchievementSets, Has.Count.EqualTo(2));
            Assert.That(_viewModel.AvailableAchievementSets.Any(s => s.SetType == AchievementSetType.Core), Is.True);
            Assert.That(_viewModel.AvailableAchievementSets.Any(s => s.SetType == AchievementSetType.Bonus), Is.True);
        });
    }

    [Test]
    public void CurrentGame_WithMultipleSets_SelectsCoreSetByDefault()
    {
        // Arrange
        var game = CreateMultiSetGame();

        // Act
        _viewModel.CurrentGame = game;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_viewModel.SelectedAchievementSet, Is.Not.Null);
            Assert.That(_viewModel.SelectedAchievementSet!.SetType, Is.EqualTo(AchievementSetType.Core));
        });
    }

    [Test]
    public void CurrentGame_WhenChangedToSingleSetGame_ShowsSingleSet()
    {
        // Arrange - first set a multi-set game
        var multiSetGame = CreateMultiSetGame();
        _viewModel.CurrentGame = multiSetGame;
        Assert.That(_viewModel.AvailableAchievementSets, Has.Count.EqualTo(2));

        // Act - change to single-set game
        var singleSetGame = CreateSingleSetGame();
        _viewModel.CurrentGame = singleSetGame;

        // Assert - the dropdown still surfaces the one (core) set so it stays visible on the dashboard
        Assert.Multiple(() =>
        {
            Assert.That(_viewModel.HasMultipleSets, Is.False);
            Assert.That(_viewModel.AvailableAchievementSets, Has.Count.EqualTo(1));
            Assert.That(_viewModel.AvailableAchievementSets[0].SetType, Is.EqualTo(AchievementSetType.Core));
        });
    }

    #endregion

    #region SelectedAchievementSet Tests

    [Test]
    public void SelectedAchievementSet_WhenChanged_UpdatesLockedAchievementsList()
    {
        // Arrange
        var game = CreateMultiSetGame();
        _viewModel.CurrentGame = game;
        var initialLockedCount = _viewModel.LockedAchievements.Count;

        // Get the bonus set
        var bonusSet = _viewModel.AvailableAchievementSets.First(s => s.SetType == AchievementSetType.Bonus);

        // Act
        _viewModel.SelectedAchievementSet = bonusSet;

        // Assert - bonus set has 3 achievements, all locked
        Assert.That(_viewModel.LockedAchievements, Has.Count.EqualTo(3));
    }

    [Test]
    public void SelectedAchievementSet_WhenChanged_UpdatesUnlockedAchievementsList()
    {
        // Arrange
        var game = CreateMultiSetGame();
        _viewModel.CurrentGame = game;

        // Get the core set (which has 2 unlocked achievements)
        var coreSet = _viewModel.AvailableAchievementSets.First(s => s.SetType == AchievementSetType.Core);
        _viewModel.SelectedAchievementSet = coreSet;

        // Assert - core set has 2 unlocked achievements
        Assert.That(_viewModel.UnlockedAchievements, Has.Count.EqualTo(2));
    }

    [Test]
    public void SelectedAchievementSet_WhenChanged_UpdatesSelectedSetName()
    {
        // Arrange
        var game = CreateMultiSetGame();
        _viewModel.CurrentGame = game;

        var bonusSet = _viewModel.AvailableAchievementSets.First(s => s.SetType == AchievementSetType.Bonus);

        // Act
        _viewModel.SelectedAchievementSet = bonusSet;

        // Assert
        Assert.That(_viewModel.SelectedSetName, Is.EqualTo("Bonus Challenges"));
    }

    [Test]
    public void SelectedAchievementSet_WhenChanged_ResetsFocusToFirstLockedAchievement()
    {
        // Arrange
        var game = CreateMultiSetGame();
        _viewModel.CurrentGame = game;

        var bonusSet = _viewModel.AvailableAchievementSets.First(s => s.SetType == AchievementSetType.Bonus);

        // Act
        _viewModel.SelectedAchievementSet = bonusSet;

        // Assert - should have a focus achievement from the bonus set
        Assert.Multiple(() =>
        {
            Assert.That(_viewModel.CurrentFocusAchievement, Is.Not.Null);
            Assert.That(_viewModel.CurrentFocusAchievement!.Title, Does.StartWith("Speed Run"));
        });
    }

    [Test]
    public void SelectedAchievementSet_WhenChanged_UpdatesGameStatistics()
    {
        // Arrange
        var game = CreateMultiSetGame();
        _viewModel.CurrentGame = game;

        // Get initial stats (core set: 6 achievements, 2 unlocked)
        var initialEarned = _viewModel.GameAchievementsEarned;
        var initialTotal = _viewModel.GameAchievementsTotal;

        // Switch to bonus set (3 achievements, 0 unlocked)
        var bonusSet = _viewModel.AvailableAchievementSets.First(s => s.SetType == AchievementSetType.Bonus);
        
        // Act
        _viewModel.SelectedAchievementSet = bonusSet;

        // Assert - stats should reflect the bonus set now
        Assert.Multiple(() =>
        {
            // Note: GameAchievementsTotal comes from CurrentGame.Achievements which 
            // reflects the selected set via GameInfo.ActiveSet
            Assert.That(_viewModel.LockedAchievements, Has.Count.EqualTo(3), "Bonus set has 3 locked achievements");
            Assert.That(_viewModel.UnlockedAchievements, Has.Count.EqualTo(0), "Bonus set has 0 unlocked achievements");
        });
    }

    [Test]
    public void SelectedAchievementSet_WhenSetToNull_DoesNotThrow()
    {
        // Arrange
        var game = CreateMultiSetGame();
        _viewModel.CurrentGame = game;

        // Act & Assert
        Assert.DoesNotThrow(() => _viewModel.SelectedAchievementSet = null);
    }

    #endregion

    #region Single Set Game (Backward Compatibility) Tests

    [Test]
    public void SingleSetGame_WorksWithLegacyAchievementsList()
    {
        // Arrange - create a game with direct achievements (no sets)
        var game = new GameInfo
        {
            Id = 1,
            Title = "Legacy Game",
            Achievements = new List<Achievement>
            {
                new Achievement { Id = 1, Title = "Achievement 1", Points = 10 },
                new Achievement { Id = 2, Title = "Achievement 2", Points = 20 }
            }
        };

        // Act
        _viewModel.CurrentGame = game;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_viewModel.HasMultipleSets, Is.False);
            // A legacy (no-set) game synthesizes a single Core set so the dropdown is still present.
            Assert.That(_viewModel.AvailableAchievementSets, Has.Count.EqualTo(1));
            Assert.That(_viewModel.AvailableAchievementSets[0].SetType, Is.EqualTo(AchievementSetType.Core));
            Assert.That(_viewModel.LockedAchievements, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public void SingleSetGame_SelectedSetNameReflectsCoreSet()
    {
        // Arrange
        var game = CreateSingleSetGame();

        // Act
        _viewModel.CurrentGame = game;

        // Assert - single-set games still select their one set (named "Core" here)
        Assert.That(_viewModel.SelectedSetName, Is.EqualTo("Core"));
    }

    [Test]
    public void HasAchievementSets_ReflectsWhetherDropdownShouldShow()
    {
        Assert.Multiple(() =>
        {
            Assert.That(_viewModel.HasAchievementSets, Is.False, "No game loaded");

            _viewModel.CurrentGame = CreateSingleSetGame();
            Assert.That(_viewModel.HasAchievementSets, Is.True, "Single-set game still shows the dropdown");

            _viewModel.CurrentGame = CreateMultiSetGame();
            Assert.That(_viewModel.HasAchievementSets, Is.True, "Multiset game shows the dropdown");

            _viewModel.CurrentGame = null;
            Assert.That(_viewModel.HasAchievementSets, Is.False, "Cleared game hides the dropdown");
        });
    }

    #endregion

    #region Focus Navigation with Multi-Set Tests

    [Test]
    public void FocusNavigation_StopsAtLastWithinSelectedSet()
    {
        // Arrange
        var game = CreateMultiSetGame();
        _viewModel.CurrentGame = game;

        // Switch to core set and navigate
        var coreSet = _viewModel.AvailableAchievementSets.First(s => s.SetType == AchievementSetType.Core);
        _viewModel.SelectedAchievementSet = coreSet;

        var lockedCount = _viewModel.LockedAchievements.Count;

        // Act - press Next more times than there are achievements
        for (int i = 0; i < lockedCount + 3; i++)
        {
            _viewModel.NextFocusCommand.Execute(null);
        }

        // Assert - no wrap-around: stays on the last achievement in the set
        Assert.That(_viewModel.CurrentFocusIndex, Is.EqualTo(lockedCount - 1));
    }

    #endregion

    #region Helper Methods

    private static GameInfo CreateMultiSetGame()
    {
        var coreAchievements = new List<Achievement>
        {
            new Achievement { Id = 1, Title = "First Steps", Description = "Complete the tutorial", Points = 5, DateEarned = DateTime.Now.AddHours(-2) },
            new Achievement { Id = 2, Title = "Explorer", Description = "Discover a hidden area", Points = 10, DateEarned = DateTime.Now.AddHours(-1) },
            new Achievement { Id = 3, Title = "Collector", Description = "Collect 100 coins", Points = 15 },
            new Achievement { Id = 4, Title = "Speed Demon", Description = "Complete level 1 in under 60 seconds", Points = 25 },
            new Achievement { Id = 5, Title = "Boss Slayer", Description = "Defeat the first boss", Points = 20 },
            new Achievement { Id = 6, Title = "Perfectionist", Description = "Complete a level without taking damage", Points = 50 },
        };

        var bonusAchievements = new List<Achievement>
        {
            new Achievement { Id = 101, Title = "Speed Run Master", Description = "Complete the entire game in under 30 minutes", Points = 100 },
            new Achievement { Id = 102, Title = "No Death Run", Description = "Complete the game without dying", Points = 150 },
            new Achievement { Id = 103, Title = "Secret Hunter", Description = "Find all 10 hidden secrets", Points = 75 },
        };

        var coreSet = new AchievementSet
        {
            Id = 1,
            Name = "Core",
            SetType = AchievementSetType.Core,
            GameId = 1,
            Achievements = coreAchievements
        };

        var bonusSet = new AchievementSet
        {
            Id = 2,
            Name = "Bonus Challenges",
            SetType = AchievementSetType.Bonus,
            GameId = 1,
            Achievements = bonusAchievements
        };

        return new GameInfo
        {
            Id = 1,
            Title = "Test Game",
            ConsoleName = "Test Console",
            AchievementSets = new List<AchievementSet> { coreSet, bonusSet }
        };
    }

    private static GameInfo CreateSingleSetGame()
    {
        var coreAchievements = new List<Achievement>
        {
            new Achievement { Id = 1, Title = "Achievement 1", Points = 10 },
            new Achievement { Id = 2, Title = "Achievement 2", Points = 20 },
        };

        var coreSet = new AchievementSet
        {
            Id = 1,
            Name = "Core",
            SetType = AchievementSetType.Core,
            GameId = 1,
            Achievements = coreAchievements
        };

        return new GameInfo
        {
            Id = 2,
            Title = "Single Set Game",
            ConsoleName = "Test Console",
            AchievementSets = new List<AchievementSet> { coreSet }
        };
    }

    #endregion
}
