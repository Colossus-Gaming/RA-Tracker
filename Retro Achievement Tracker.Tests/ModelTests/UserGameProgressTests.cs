using RATracker.Models;

namespace RATracker.Tests.ModelTests;

/// <summary>
/// Tests for <see cref="UserGameProgress"/>, including the achievement-list count fallback
/// (so mastery/completion is correct even when only the achievement list is populated).
/// </summary>
[TestFixture]
public class UserGameProgressTests
{
    [Test]
    public void EarnedAndTotal_FallBackToAchievementListWhenNotSet()
    {
        var progress = new UserGameProgress
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
            Assert.That(progress.TotalAchievements, Is.EqualTo(3));
            Assert.That(progress.EarnedAchievements, Is.EqualTo(2));
        });
    }

    [Test]
    public void EarnedAndTotal_ExplicitValuesWinOverList()
    {
        var progress = new UserGameProgress
        {
            TotalAchievements = 50,
            EarnedAchievements = 10,
            Achievements = new List<Achievement> { new() { Id = 1, DateEarned = DateTime.Now } }
        };

        Assert.Multiple(() =>
        {
            Assert.That(progress.TotalAchievements, Is.EqualTo(50));
            Assert.That(progress.EarnedAchievements, Is.EqualTo(10));
        });
    }

    [Test]
    public void IsMastered_TrueWhenAllListAchievementsEarned_NoExplicitCounts()
    {
        var progress = new UserGameProgress
        {
            Achievements = new List<Achievement>
            {
                new() { Id = 1, DateEarned = DateTime.Now },
                new() { Id = 2, DateEarned = DateTime.Now }
            }
        };

        Assert.That(progress.IsMastered, Is.True);
    }

    [Test]
    public void IsMastered_FalseWhenSomeLocked()
    {
        var progress = new UserGameProgress
        {
            Achievements = new List<Achievement>
            {
                new() { Id = 1, DateEarned = DateTime.Now },
                new() { Id = 2, DateEarned = null }
            }
        };

        Assert.That(progress.IsMastered, Is.False);
    }

    [Test]
    public void UnlockedAndLocked_PartitionAchievements()
    {
        var progress = new UserGameProgress
        {
            Achievements = new List<Achievement>
            {
                new() { Id = 1, DateEarned = DateTime.Now },
                new() { Id = 2, DateEarned = null },
                new() { Id = 3, DateEarned = null }
            }
        };

        Assert.Multiple(() =>
        {
            Assert.That(progress.UnlockedAchievements, Has.Count.EqualTo(1));
            Assert.That(progress.LockedAchievements, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public void HasMultipleSets_TrueWhenMoreThanOneSetProgress()
    {
        var progress = new UserGameProgress
        {
            AchievementSets = new List<AchievementSetProgress>
            {
                new() { SetId = 1, SetType = AchievementSetType.Core },
                new() { SetId = 2, SetType = AchievementSetType.Bonus }
            }
        };

        Assert.That(progress.HasMultipleSets, Is.True);
    }

    [Test]
    public void AchievementSetProgress_IsCompleted_WhenAllEarned()
    {
        var set = AchievementSetProgress.FromAchievementSet(new AchievementSet
        {
            Id = 1,
            Name = "Bonus",
            SetType = AchievementSetType.Bonus,
            Achievements = new List<Achievement>
            {
                new() { Id = 1, Points = 10, DateEarned = DateTime.Now },
                new() { Id = 2, Points = 20, DateEarned = DateTime.Now }
            }
        });

        Assert.Multiple(() =>
        {
            Assert.That(set.TotalAchievements, Is.EqualTo(2));
            Assert.That(set.EarnedAchievements, Is.EqualTo(2));
            Assert.That(set.TotalPoints, Is.EqualTo(30));
            Assert.That(set.IsCompleted, Is.True);
            Assert.That(set.SetType, Is.EqualTo(AchievementSetType.Bonus));
        });
    }

    [Test]
    public void FromGameInfo_CopiesSetsAndTotals()
    {
        var game = new GameInfo
        {
            Id = 99,
            Title = "Test",
            AchievementSets = new()
            {
                new AchievementSet { Id = 1, SetType = AchievementSetType.Core, Achievements = new() { new Achievement { Id = 1, DateEarned = DateTime.Now } } },
                new AchievementSet { Id = 2, SetType = AchievementSetType.Bonus, Achievements = new() { new Achievement { Id = 2 } } }
            }
        };

        var progress = UserGameProgress.FromGameInfo(game, "user");

        Assert.Multiple(() =>
        {
            Assert.That(progress.GameId, Is.EqualTo(99));
            Assert.That(progress.AchievementSets, Has.Count.EqualTo(2));
            Assert.That(progress.HasMultipleSets, Is.True);
        });
    }
}
