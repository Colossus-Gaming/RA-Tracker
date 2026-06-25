using Moq;
using RATracker.Models;
using RATracker.WPF.Services;

namespace RATracker.Tests.ServiceTests;

/// <summary>
/// Tests that <see cref="AchievementTrackingService"/> reconstructs per-set achievement groupings
/// from tagged achievements (the subset-tracking feature), rather than dumping everything into Core.
/// </summary>
[TestFixture]
public class AchievementTrackingServiceSubsetTests
{
    private const string User = "TestUser";

    private static UserGameProgress BuildMultiSetProgress()
    {
        // Achievements carry set membership, exactly as the V2 mapper now populates them.
        var achievements = new List<Achievement>
        {
            new() { Id = 1, Title = "Core A", SetId = 10, SetType = AchievementSetType.Core, SetName = "Core", DateEarned = DateTime.Now },
            new() { Id = 2, Title = "Core B", SetId = 10, SetType = AchievementSetType.Core, SetName = "Core" },
            new() { Id = 3, Title = "Bonus A", SetId = 20, SetType = AchievementSetType.Bonus, SetName = "Bonus Challenges" },
            new() { Id = 4, Title = "Bonus B", SetId = 20, SetType = AchievementSetType.Bonus, SetName = "Bonus Challenges", DateEarned = DateTime.Now },
        };

        return new UserGameProgress
        {
            UserId = User,
            GameId = 555,
            GameTitle = "Subset Game",
            ConsoleName = "NES",
            Achievements = achievements,
            AchievementSets = new List<AchievementSetProgress>
            {
                new() { SetId = 10, SetName = "Core", SetType = AchievementSetType.Core, TotalAchievements = 2, EarnedAchievements = 1 },
                new() { SetId = 20, SetName = "Bonus Challenges", SetType = AchievementSetType.Bonus, TotalAchievements = 2, EarnedAchievements = 1 },
            }
        };
    }

    private static AchievementTrackingService CreateService(UserGameProgress progress)
    {
        var mock = new Mock<IProgressService>();
        mock.Setup(p => p.GetUserGameProgressAsync(User, progress.GameId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(progress);
        return new AchievementTrackingService(mock.Object, User);
    }

    [Test]
    public async Task GetGameByIdAsync_GroupsAchievementsIntoTheirSets()
    {
        var service = CreateService(BuildMultiSetProgress());

        var game = await service.GetGameByIdAsync(555);

        Assert.That(game, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(game!.HasMultipleSets, Is.True);
            Assert.That(game.AchievementSets, Has.Count.EqualTo(2));

            var core = game.AchievementSets.Single(s => s.SetType == AchievementSetType.Core);
            var bonus = game.AchievementSets.Single(s => s.SetType == AchievementSetType.Bonus);

            Assert.That(core.Achievements.Select(a => a.Id), Is.EquivalentTo(new[] { 1, 2 }));
            Assert.That(bonus.Achievements.Select(a => a.Id), Is.EquivalentTo(new[] { 3, 4 }));
            Assert.That(bonus.Name, Is.EqualTo("Bonus Challenges"));
        });
    }

    [Test]
    public async Task GetGameByIdAsync_OrdersCoreSetFirst()
    {
        var service = CreateService(BuildMultiSetProgress());

        var game = await service.GetGameByIdAsync(555);

        Assert.That(game!.AchievementSets[0].SetType, Is.EqualTo(AchievementSetType.Core));
    }

    [Test]
    public async Task GetGameByIdAsync_PreservesPerSetEarnedCounts()
    {
        var service = CreateService(BuildMultiSetProgress());

        var game = await service.GetGameByIdAsync(555);

        var bonus = game!.AchievementSets.Single(s => s.SetType == AchievementSetType.Bonus);
        Assert.Multiple(() =>
        {
            Assert.That(bonus.AchievementsEarned, Is.EqualTo(1));
            Assert.That(bonus.AchievementCount, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task GetGameByIdAsync_NoSetMembership_KeepsFlatList()
    {
        var flat = new UserGameProgress
        {
            UserId = User,
            GameId = 777,
            GameTitle = "Flat Game",
            Achievements = new List<Achievement>
            {
                new() { Id = 1, Title = "A" },
                new() { Id = 2, Title = "B", DateEarned = DateTime.Now }
            }
        };
        var service = CreateService(flat);

        var game = await service.GetGameByIdAsync(777);

        Assert.Multiple(() =>
        {
            Assert.That(game!.HasMultipleSets, Is.False);
            Assert.That(game.AchievementSets, Is.Empty);
            Assert.That(game.Achievements, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public async Task GetGameByIdAsync_UntaggedAchievementsFoldIntoCore()
    {
        var progress = new UserGameProgress
        {
            UserId = User,
            GameId = 888,
            GameTitle = "Mixed",
            Achievements = new List<Achievement>
            {
                new() { Id = 1, SetId = 10, SetType = AchievementSetType.Core, SetName = "Core" },
                new() { Id = 2, SetId = 20, SetType = AchievementSetType.Bonus, SetName = "Bonus" },
                new() { Id = 3 } // no set id -> should land in Core
            }
        };
        var service = CreateService(progress);

        var game = await service.GetGameByIdAsync(888);

        var core = game!.AchievementSets.Single(s => s.SetType == AchievementSetType.Core);
        Assert.That(core.Achievements.Select(a => a.Id), Does.Contain(3));
    }

    [Test]
    public async Task GetGameByIdAsync_UnknownGame_ReturnsNull()
    {
        var mock = new Mock<IProgressService>();
        mock.Setup(p => p.GetUserGameProgressAsync(User, 999, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserGameProgress?)null);
        var service = new AchievementTrackingService(mock.Object, User);

        Assert.That(await service.GetGameByIdAsync(999), Is.Null);
    }
}
