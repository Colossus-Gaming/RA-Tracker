using RATracker.Models;

namespace RATracker.Tests.ModelTests;

/// <summary>
/// Tests for the real <see cref="GameInfo"/> model, including console mapping and multi-set behavior.
/// </summary>
[TestFixture]
public class GameInfoTests
{
    [TestCase(1, "Sega Genesis")]
    [TestCase(3, "Super Nintendo Entertainment System")]
    [TestCase(5, "Nintendo Game Boy Advance")]
    [TestCase(7, "Nintendo Entertainment System")]
    [TestCase(27, "Arcade")]
    [TestCase(40, "Sega Dreamcast")]
    public void ConsoleId_MapsToConsoleName(int consoleId, string expected)
    {
        Assert.That(new GameInfo { ConsoleId = consoleId }.ConsoleName, Is.EqualTo(expected));
    }

    [Test]
    public void ConsoleId_UnknownId_MapsToUnknownConsole()
    {
        Assert.That(new GameInfo { ConsoleId = 9999 }.ConsoleName, Is.EqualTo("Unknown Console"));
    }

    [Test]
    public void AchievementsEarned_CountsOnlyUnlocked()
    {
        var game = new GameInfo
        {
            Achievements = new List<Achievement>
            {
                new() { Id = 1, DateEarned = DateTime.Now },
                new() { Id = 2, DateEarned = DateTime.Now },
                new() { Id = 3, DateEarned = null }
            }
        };

        Assert.Multiple(() =>
        {
            Assert.That(game.AchievementsEarned, Is.EqualTo(2));
            Assert.That(game.AchievementsPossible, Is.EqualTo(3));
        });
    }

    [Test]
    public void GamePoints_SumOnlyCountUnlockedForEarned()
    {
        var game = new GameInfo
        {
            Achievements = new List<Achievement>
            {
                new() { Id = 1, Points = 10, TrueRatio = 20, DateEarned = DateTime.Now },
                new() { Id = 2, Points = 25, TrueRatio = 50, DateEarned = null }
            }
        };

        Assert.Multiple(() =>
        {
            Assert.That(game.GamePointsEarned, Is.EqualTo(10));
            Assert.That(game.GamePointsPossible, Is.EqualTo(35));
            Assert.That(game.GameTruePointsEarned, Is.EqualTo(20));
            Assert.That(game.GameTruePointsPossible, Is.EqualTo(70));
        });
    }

    [Test]
    public void PercentComplete_CalculatesCorrectly()
    {
        var game = new GameInfo
        {
            Achievements = new List<Achievement>
            {
                new() { Id = 1, DateEarned = DateTime.Now },
                new() { Id = 2, DateEarned = null },
                new() { Id = 3, DateEarned = null },
                new() { Id = 4, DateEarned = null }
            }
        };

        Assert.That(game.PercentComplete, Is.EqualTo("25.00"));
    }

    [Test]
    public void CompareTo_SortsByLastPlayedDescending_NullsFirst()
    {
        var games = new List<GameInfo>
        {
            new() { Id = 1, LastPlayed = new DateTime(2024, 1, 1) },
            new() { Id = 2, LastPlayed = new DateTime(2024, 6, 1) },
            new() { Id = 3, LastPlayed = new DateTime(2024, 3, 1) },
            new() { Id = 4, LastPlayed = null }
        };

        games.Sort();

        Assert.Multiple(() =>
        {
            Assert.That(games[0].Id, Is.EqualTo(4));
            Assert.That(games[1].Id, Is.EqualTo(2));
            Assert.That(games[2].Id, Is.EqualTo(3));
            Assert.That(games[3].Id, Is.EqualTo(1));
        });
    }

    #region Multi-set behavior

    [Test]
    public void HasMultipleSets_TrueOnlyWhenMoreThanOneSet()
    {
        var single = new GameInfo { AchievementSets = new() { new AchievementSet { Id = 1, SetType = AchievementSetType.Core } } };
        var multi = new GameInfo
        {
            AchievementSets = new()
            {
                new AchievementSet { Id = 1, SetType = AchievementSetType.Core },
                new AchievementSet { Id = 2, SetType = AchievementSetType.Bonus }
            }
        };

        Assert.Multiple(() =>
        {
            Assert.That(single.HasMultipleSets, Is.False);
            Assert.That(multi.HasMultipleSets, Is.True);
        });
    }

    [Test]
    public void CoreSet_ReturnsCoreRegardlessOfOrder()
    {
        var core = new AchievementSet { Id = 1, SetType = AchievementSetType.Core };
        var bonus = new AchievementSet { Id = 2, SetType = AchievementSetType.Bonus };
        var game = new GameInfo { AchievementSets = new() { bonus, core } };

        Assert.That(game.CoreSet, Is.SameAs(core));
    }

    [Test]
    public void ActiveSet_DefaultsToCore_RespectsSelection()
    {
        var core = new AchievementSet { Id = 1, SetType = AchievementSetType.Core };
        var bonus = new AchievementSet { Id = 2, SetType = AchievementSetType.Bonus };
        var game = new GameInfo { AchievementSets = new() { bonus, core } };

        Assert.That(game.ActiveSet, Is.SameAs(core));

        game.SelectedSet = bonus;
        Assert.That(game.ActiveSet, Is.SameAs(bonus));
    }

    [Test]
    public void Achievements_ReturnsActiveSetAchievements_AllAchievementsReturnsUnion()
    {
        var game = new GameInfo
        {
            AchievementSets = new()
            {
                new AchievementSet { Id = 1, SetType = AchievementSetType.Core, Achievements = new() { new Achievement { Id = 1, Title = "Core" } } },
                new AchievementSet { Id = 2, SetType = AchievementSetType.Bonus, Achievements = new() { new Achievement { Id = 2, Title = "Bonus" } } }
            }
        };

        Assert.Multiple(() =>
        {
            Assert.That(game.Achievements.Single().Title, Is.EqualTo("Core"));
            Assert.That(game.AllAchievements, Has.Count.EqualTo(2));
            Assert.That(game.TotalAchievementsAllSets, Is.EqualTo(2));
        });
    }

    [Test]
    public void Achievements_FallsBackToLegacyListWhenNoSets()
    {
        var game = new GameInfo
        {
            Achievements = new() { new Achievement { Id = 1 }, new Achievement { Id = 2 } }
        };

        Assert.Multiple(() =>
        {
            Assert.That(game.HasMultipleSets, Is.False);
            Assert.That(game.Achievements, Has.Count.EqualTo(2));
            Assert.That(game.AllAchievements, Has.Count.EqualTo(2));
        });
    }

    #endregion
}
