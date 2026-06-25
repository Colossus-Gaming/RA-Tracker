using System.Net;
using Moq;
using Moq.Protected;
using RATracker.Models;
using RATracker.WPF.Http.V2;
using RATracker.WPF.Services;

namespace RATracker.Tests.V2ApiTests;

/// <summary>
/// Verifies that <see cref="V2ProgressService"/> maps a multi-set game-progress JSON:API response into
/// achievements tagged with their set membership (the data the subset-tracking UI depends on).
/// </summary>
[TestFixture]
public class V2ProgressServiceSubsetTests
{
    private Mock<HttpMessageHandler> _mockHandler = null!;
    private HttpClient _httpClient = null!;
    private V2Client _client = null!;

    private const string TestApiKey = "test-api-key";
    private const string TestBaseUrl = "https://test.retroachievements.org";

    [SetUp]
    public void SetUp()
    {
        _mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Loose);
        _httpClient = new HttpClient(_mockHandler.Object);
        _client = new V2Client(TestApiKey, _httpClient, TestBaseUrl);
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _httpClient.Dispose();
    }

    private const string MultiSetProgressJson = @"{
        ""data"": {
            ""type"": ""user-game-progress"",
            ""id"": ""1234"",
            ""attributes"": { ""gameId"": ""1234"" },
            ""relationships"": {
                ""game"": { ""data"": { ""type"": ""games"", ""id"": ""1234"" } },
                ""achievementSets"": {
                    ""data"": [
                        { ""type"": ""achievement-sets"", ""id"": ""100"" },
                        { ""type"": ""achievement-sets"", ""id"": ""101"" }
                    ]
                }
            }
        },
        ""included"": [
            {
                ""type"": ""games"", ""id"": ""1234"",
                ""attributes"": { ""title"": ""Subset Game"" },
                ""relationships"": { ""system"": { ""data"": { ""type"": ""systems"", ""id"": ""7"" } } }
            },
            { ""type"": ""systems"", ""id"": ""7"", ""attributes"": { ""name"": ""NES"" } },
            {
                ""type"": ""achievement-sets"", ""id"": ""100"",
                ""attributes"": { ""name"": ""Core Set"", ""type"": ""core"" },
                ""relationships"": { ""achievements"": { ""data"": [
                    { ""type"": ""achievements"", ""id"": ""1"" },
                    { ""type"": ""achievements"", ""id"": ""2"" }
                ] } }
            },
            {
                ""type"": ""achievement-sets"", ""id"": ""101"",
                ""attributes"": { ""name"": ""Bonus Set"", ""type"": ""bonus"" },
                ""relationships"": { ""achievements"": { ""data"": [
                    { ""type"": ""achievements"", ""id"": ""3"" }
                ] } }
            },
            { ""type"": ""achievements"", ""id"": ""1"", ""attributes"": { ""title"": ""Core A"", ""points"": 5 } },
            { ""type"": ""achievements"", ""id"": ""2"", ""attributes"": { ""title"": ""Core B"", ""points"": 10 } },
            { ""type"": ""achievements"", ""id"": ""3"", ""attributes"": { ""title"": ""Bonus A"", ""points"": 25 } }
        ]
    }";

    private void SetupResponse(string json, HttpStatusCode status = HttpStatusCode.OK)
    {
        var response = new HttpResponseMessage(status)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/vnd.api+json")
        };
        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    [Test]
    public async Task GetUserGameProgress_TagsAchievementsWithSetMembership()
    {
        SetupResponse(MultiSetProgressJson);
        using var service = new V2ProgressService(_client);

        var progress = await service.GetUserGameProgressAsync("user", 1234, includeAchievementSets: true);

        Assert.That(progress, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(progress!.Achievements, Has.Count.EqualTo(3));

            var coreA = progress.Achievements.Single(a => a.Id == 1);
            Assert.That(coreA.SetType, Is.EqualTo(AchievementSetType.Core));
            Assert.That(coreA.SetId, Is.EqualTo(100));
            Assert.That(coreA.SetName, Is.EqualTo("Core Set"));

            var bonusA = progress.Achievements.Single(a => a.Id == 3);
            Assert.That(bonusA.SetType, Is.EqualTo(AchievementSetType.Bonus));
            Assert.That(bonusA.SetId, Is.EqualTo(101));
            Assert.That(bonusA.SetName, Is.EqualTo("Bonus Set"));
        });
    }

    [Test]
    public async Task GetUserGameProgress_PopulatesPerSetProgress()
    {
        SetupResponse(MultiSetProgressJson);
        using var service = new V2ProgressService(_client);

        var progress = await service.GetUserGameProgressAsync("user", 1234, includeAchievementSets: true);

        Assert.Multiple(() =>
        {
            Assert.That(progress!.AchievementSets, Has.Count.EqualTo(2));
            Assert.That(progress.HasMultipleSets, Is.True);
            Assert.That(progress.AchievementSets.Any(s => s.SetType == AchievementSetType.Core), Is.True);
            Assert.That(progress.AchievementSets.Any(s => s.SetType == AchievementSetType.Bonus), Is.True);
        });
    }

    [Test]
    public async Task GetUserGameProgress_MapsGameAndConsole()
    {
        SetupResponse(MultiSetProgressJson);
        using var service = new V2ProgressService(_client);

        var progress = await service.GetUserGameProgressAsync("user", 1234, includeAchievementSets: true);

        Assert.Multiple(() =>
        {
            Assert.That(progress!.GameTitle, Is.EqualTo("Subset Game"));
            Assert.That(progress.ConsoleName, Is.EqualTo("NES"));
        });
    }

    [Test]
    public async Task GetUserGameProgress_EmptyUser_ReturnsNull()
    {
        using var service = new V2ProgressService(_client);

        Assert.That(await service.GetUserGameProgressAsync("", 1234), Is.Null);
    }

    [Test]
    public async Task GetGameAchievementsWithSets_TagsEachAchievementWithItsSetFromAchievementSetRelationship()
    {
        // Real v2 shape (Super Mario Bros, game 1446): /v2/achievements?filter[gameId]&include=achievementSet
        // returns every achievement across all sets, each carrying an achievementSet relationship.
        // The included achievement-set's types[] array gives the set type for THIS game.
        var jsonResponse = @"{
            ""data"": [
                {
                    ""type"": ""achievements"",
                    ""id"": ""3152"",
                    ""attributes"": {
                        ""title"": ""If I Were a Rich Man"",
                        ""description"": ""Collect 50 Coins"",
                        ""points"": 1,
                        ""pointsWeighted"": 1,
                        ""badgeUrl"": ""https://media.retroachievements.org/Badge/386432.png"",
                        ""orderColumn"": 5
                    },
                    ""relationships"": {
                        ""achievementSet"": { ""data"": { ""type"": ""achievement-sets"", ""id"": ""1019"" } }
                    }
                },
                {
                    ""type"": ""achievements"",
                    ""id"": ""500001"",
                    ""attributes"": {
                        ""title"": ""Bonus Run"",
                        ""description"": ""Beat the game without dying"",
                        ""points"": 25,
                        ""badgeUrl"": ""https://media.retroachievements.org/Badge/9999.png"",
                        ""orderColumn"": 1
                    },
                    ""relationships"": {
                        ""achievementSet"": { ""data"": { ""type"": ""achievement-sets"", ""id"": ""6598"" } }
                    }
                }
            ],
            ""included"": [
                {
                    ""type"": ""achievement-sets"",
                    ""id"": ""1019"",
                    ""attributes"": {
                        ""title"": null,
                        ""achievementsPublished"": 77,
                        ""types"": [{ ""gameId"": 1446, ""type"": ""core"" }]
                    }
                },
                {
                    ""type"": ""achievement-sets"",
                    ""id"": ""6598"",
                    ""attributes"": {
                        ""title"": ""Bonus Challenges"",
                        ""achievementsPublished"": 12,
                        ""types"": [{ ""gameId"": 1446, ""type"": ""bonus"" }]
                    }
                }
            ]
        }";

        SetupResponse(jsonResponse);

        using var service = new V2ProgressService(_client);
        var achievements = await service.GetGameAchievementsWithSetsAsync(1446);

        Assert.That(achievements, Has.Count.EqualTo(2));

        var core = achievements.Single(a => a.Id == 3152);
        Assert.Multiple(() =>
        {
            Assert.That(core.SetId, Is.EqualTo(1019));
            Assert.That(core.SetType, Is.EqualTo(AchievementSetType.Core));
            Assert.That(core.SetName, Is.EqualTo("Core"));  // title is null -> falls back to type name
            Assert.That(core.Title, Is.EqualTo("If I Were a Rich Man"));
            Assert.That(core.Points, Is.EqualTo(1));
            Assert.That(core.DisplayOrder, Is.EqualTo(5));
        });

        var bonus = achievements.Single(a => a.Id == 500001);
        Assert.Multiple(() =>
        {
            Assert.That(bonus.SetId, Is.EqualTo(6598));
            Assert.That(bonus.SetType, Is.EqualTo(AchievementSetType.Bonus));
            Assert.That(bonus.SetName, Is.EqualTo("Bonus Challenges"));
            Assert.That(bonus.Title, Is.EqualTo("Bonus Run"));
        });
    }

    [Test]
    public async Task GetPlayerAchievementSets_MapsSetTypeFromSetContextAndTotalsFromIncluded()
    {
        // Real shape from live probe of /users/{u}/player-achievement-sets?filter[gameId]={id}&include=achievementSet
        // setContext carries the discriminator (core/bonus/specialty/exclusive); the included achievement-set
        // carries achievementsPublished (total) and pointsTotal.
        var jsonResponse = @"{
            ""data"": [
                {
                    ""type"": ""player-achievement-sets"",
                    ""id"": ""9203964"",
                    ""attributes"": {
                        ""achievementsUnlocked"": 12,
                        ""achievementsUnlockedHardcore"": 12,
                        ""points"": 60,
                        ""pointsHardcore"": 60,
                        ""completionPercentage"": ""0.500000000"",
                        ""setContext"": [{ ""achievementSetId"": 2463, ""gameId"": 10268, ""type"": ""core"" }]
                    },
                    ""relationships"": {
                        ""achievementSet"": { ""data"": { ""type"": ""achievement-sets"", ""id"": ""2463"" } }
                    }
                },
                {
                    ""type"": ""player-achievement-sets"",
                    ""id"": ""9203965"",
                    ""attributes"": {
                        ""achievementsUnlocked"": 2,
                        ""points"": 25,
                        ""completionPercentage"": ""0.400000000"",
                        ""setContext"": [{ ""achievementSetId"": 9999, ""gameId"": 10268, ""type"": ""bonus"" }]
                    },
                    ""relationships"": {
                        ""achievementSet"": { ""data"": { ""type"": ""achievement-sets"", ""id"": ""9999"" } }
                    }
                }
            ],
            ""included"": [
                { ""type"": ""achievement-sets"", ""id"": ""2463"", ""attributes"": { ""title"": null, ""pointsTotal"": 120, ""achievementsPublished"": 24 } },
                { ""type"": ""achievement-sets"", ""id"": ""9999"", ""attributes"": { ""title"": ""Bonus Challenges"", ""pointsTotal"": 50, ""achievementsPublished"": 5 } }
            ]
        }";

        SetupResponse(jsonResponse);

        using var service = new V2ProgressService(_client);
        var sets = await service.GetPlayerAchievementSetsAsync("RetroS3xual", 10268);

        Assert.That(sets, Has.Count.EqualTo(2));

        var core = sets.Single(s => s.SetType == AchievementSetType.Core);
        Assert.Multiple(() =>
        {
            Assert.That(core.SetId, Is.EqualTo(2463));
            Assert.That(core.EarnedAchievements, Is.EqualTo(12));
            Assert.That(core.TotalAchievements, Is.EqualTo(24));
            Assert.That(core.TotalPoints, Is.EqualTo(120));
            Assert.That(core.EarnedPoints, Is.EqualTo(60));
            Assert.That(core.SetName, Is.EqualTo("Core"));   // title is null in API; falls back to type name
        });

        var bonus = sets.Single(s => s.SetType == AchievementSetType.Bonus);
        Assert.Multiple(() =>
        {
            Assert.That(bonus.SetId, Is.EqualTo(9999));
            Assert.That(bonus.SetName, Is.EqualTo("Bonus Challenges"));
            Assert.That(bonus.TotalAchievements, Is.EqualTo(5));
            Assert.That(bonus.EarnedAchievements, Is.EqualTo(2));
        });
    }
}
