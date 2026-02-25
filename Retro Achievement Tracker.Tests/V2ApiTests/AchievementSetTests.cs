using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using RATracker.Models;
using RATracker.WPF.Http.V2;
using RATracker.WPF.Http.V2.JsonApi;
using RATracker.WPF.Http.V2.Mappers;
using RATracker.WPF.Services;
using System.Net;

namespace RATracker.Tests.V2ApiTests;

/// <summary>
/// Tests for achievement set parsing and multi-set support.
/// </summary>
[TestFixture]
public class AchievementSetTests
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

    #region AchievementSet Model Tests

    [Test]
    public void AchievementSet_ParseSetType_Core_ReturnsCore()
    {
        Assert.Multiple(() =>
        {
            Assert.That(AchievementSet.ParseSetType("core"), Is.EqualTo(AchievementSetType.Core));
            Assert.That(AchievementSet.ParseSetType("Core"), Is.EqualTo(AchievementSetType.Core));
            Assert.That(AchievementSet.ParseSetType("CORE"), Is.EqualTo(AchievementSetType.Core));
            Assert.That(AchievementSet.ParseSetType("base"), Is.EqualTo(AchievementSetType.Core));
            Assert.That(AchievementSet.ParseSetType(0), Is.EqualTo(AchievementSetType.Core));
            Assert.That(AchievementSet.ParseSetType("0"), Is.EqualTo(AchievementSetType.Core));
        });
    }

    [Test]
    public void AchievementSet_ParseSetType_Bonus_ReturnsBonus()
    {
        Assert.Multiple(() =>
        {
            Assert.That(AchievementSet.ParseSetType("bonus"), Is.EqualTo(AchievementSetType.Bonus));
            Assert.That(AchievementSet.ParseSetType("Bonus"), Is.EqualTo(AchievementSetType.Bonus));
            Assert.That(AchievementSet.ParseSetType(1), Is.EqualTo(AchievementSetType.Bonus));
            Assert.That(AchievementSet.ParseSetType("1"), Is.EqualTo(AchievementSetType.Bonus));
        });
    }

    [Test]
    public void AchievementSet_ParseSetType_Specialty_ReturnsSpecialty()
    {
        Assert.Multiple(() =>
        {
            Assert.That(AchievementSet.ParseSetType("specialty"), Is.EqualTo(AchievementSetType.Specialty));
            Assert.That(AchievementSet.ParseSetType("special"), Is.EqualTo(AchievementSetType.Specialty));
            Assert.That(AchievementSet.ParseSetType(2), Is.EqualTo(AchievementSetType.Specialty));
        });
    }

    [Test]
    public void AchievementSet_ParseSetType_Unknown_ReturnsUnknown()
    {
        Assert.Multiple(() =>
        {
            Assert.That(AchievementSet.ParseSetType("invalid"), Is.EqualTo(AchievementSetType.Unknown));
            Assert.That(AchievementSet.ParseSetType(99), Is.EqualTo(AchievementSetType.Unknown));
            Assert.That(AchievementSet.ParseSetType(null), Is.EqualTo(AchievementSetType.Core)); // null defaults to core
        });
    }

    [Test]
    public void AchievementSet_IsCore_ReturnsTrueForCoreSet()
    {
        var coreSet = new AchievementSet { SetType = AchievementSetType.Core };
        var bonusSet = new AchievementSet { SetType = AchievementSetType.Bonus };

        Assert.Multiple(() =>
        {
            Assert.That(coreSet.IsCore, Is.True);
            Assert.That(bonusSet.IsCore, Is.False);
        });
    }

    [Test]
    public void AchievementSet_CalculatesStatistics_Correctly()
    {
        var set = new AchievementSet
        {
            Achievements = new List<Achievement>
            {
                new Achievement { Id = 1, Points = 10, TrueRatio = 20, DateEarned = DateTime.Now },
                new Achievement { Id = 2, Points = 20, TrueRatio = 40, DateEarned = DateTime.Now },
                new Achievement { Id = 3, Points = 30, TrueRatio = 60, DateEarned = null },
                new Achievement { Id = 4, Points = 40, TrueRatio = 80, DateEarned = null }
            }
        };

        Assert.Multiple(() =>
        {
            Assert.That(set.AchievementCount, Is.EqualTo(4));
            Assert.That(set.AchievementsEarned, Is.EqualTo(2));
            Assert.That(set.PointsTotal, Is.EqualTo(100));
            Assert.That(set.PointsEarned, Is.EqualTo(30));
            Assert.That(set.TruePointsTotal, Is.EqualTo(200));
            Assert.That(set.TruePointsEarned, Is.EqualTo(60));
            Assert.That(set.PercentComplete, Is.EqualTo("50.00"));
        });
    }

    #endregion

    #region GameInfo Multi-Set Tests

    [Test]
    public void GameInfo_HasMultipleSets_ReturnsTrueWhenMultipleSetsExist()
    {
        var game = new GameInfo
        {
            AchievementSets = new List<AchievementSet>
            {
                new AchievementSet { Id = 1, SetType = AchievementSetType.Core },
                new AchievementSet { Id = 2, SetType = AchievementSetType.Bonus }
            }
        };

        Assert.That(game.HasMultipleSets, Is.True);
    }

    [Test]
    public void GameInfo_HasMultipleSets_ReturnsFalseWhenSingleSet()
    {
        var game = new GameInfo
        {
            AchievementSets = new List<AchievementSet>
            {
                new AchievementSet { Id = 1, SetType = AchievementSetType.Core }
            }
        };

        Assert.That(game.HasMultipleSets, Is.False);
    }

    [Test]
    public void GameInfo_CoreSet_ReturnsCorrectSet()
    {
        var coreSet = new AchievementSet { Id = 1, SetType = AchievementSetType.Core, Name = "Core" };
        var bonusSet = new AchievementSet { Id = 2, SetType = AchievementSetType.Bonus, Name = "Bonus" };

        var game = new GameInfo
        {
            AchievementSets = new List<AchievementSet> { bonusSet, coreSet }
        };

        Assert.That(game.CoreSet, Is.EqualTo(coreSet));
    }

    [Test]
    public void GameInfo_ActiveSet_ReturnsSelectedSetWhenSet()
    {
        var coreSet = new AchievementSet { Id = 1, SetType = AchievementSetType.Core };
        var bonusSet = new AchievementSet { Id = 2, SetType = AchievementSetType.Bonus };

        var game = new GameInfo
        {
            AchievementSets = new List<AchievementSet> { coreSet, bonusSet },
            SelectedSet = bonusSet
        };

        Assert.That(game.ActiveSet, Is.EqualTo(bonusSet));
    }

    [Test]
    public void GameInfo_ActiveSet_DefaultsToCoreSetWhenNoSelection()
    {
        var coreSet = new AchievementSet { Id = 1, SetType = AchievementSetType.Core };
        var bonusSet = new AchievementSet { Id = 2, SetType = AchievementSetType.Bonus };

        var game = new GameInfo
        {
            AchievementSets = new List<AchievementSet> { bonusSet, coreSet }
        };

        Assert.That(game.ActiveSet, Is.EqualTo(coreSet));
    }

    [Test]
    public void GameInfo_Achievements_ReturnsActiveSetAchievements()
    {
        var coreAchievements = new List<Achievement>
        {
            new Achievement { Id = 1, Title = "Core Achievement" }
        };
        var bonusAchievements = new List<Achievement>
        {
            new Achievement { Id = 2, Title = "Bonus Achievement" }
        };

        var game = new GameInfo
        {
            AchievementSets = new List<AchievementSet>
            {
                new AchievementSet { Id = 1, SetType = AchievementSetType.Core, Achievements = coreAchievements },
                new AchievementSet { Id = 2, SetType = AchievementSetType.Bonus, Achievements = bonusAchievements }
            }
        };

        Assert.That(game.Achievements.First().Title, Is.EqualTo("Core Achievement"));
    }

    [Test]
    public void GameInfo_AllAchievements_ReturnsAllAchievementsFromAllSets()
    {
        var coreAchievements = new List<Achievement>
        {
            new Achievement { Id = 1, Title = "Core Achievement" }
        };
        var bonusAchievements = new List<Achievement>
        {
            new Achievement { Id = 2, Title = "Bonus Achievement" }
        };

        var game = new GameInfo
        {
            AchievementSets = new List<AchievementSet>
            {
                new AchievementSet { Id = 1, SetType = AchievementSetType.Core, Achievements = coreAchievements },
                new AchievementSet { Id = 2, SetType = AchievementSetType.Bonus, Achievements = bonusAchievements }
            }
        };

        Assert.Multiple(() =>
        {
            Assert.That(game.AllAchievements, Has.Count.EqualTo(2));
            Assert.That(game.TotalAchievementsAllSets, Is.EqualTo(2));
        });
    }

    [Test]
    public void GameInfo_LegacyAchievements_WorksWithoutSets()
    {
        var game = new GameInfo
        {
            Achievements = new List<Achievement>
            {
                new Achievement { Id = 1, Title = "Test" },
                new Achievement { Id = 2, Title = "Test 2" }
            }
        };

        Assert.Multiple(() =>
        {
            Assert.That(game.Achievements, Has.Count.EqualTo(2));
            Assert.That(game.AllAchievements, Has.Count.EqualTo(2));
            Assert.That(game.HasMultipleSets, Is.False);
        });
    }

    #endregion

    #region V2ResourceMapper Achievement Set Tests

    [Test]
    public void MapToAchievementSet_MapsBasicProperties()
    {
        var json = @"{
            ""type"": ""achievement-sets"",
            ""id"": ""100"",
            ""attributes"": {
                ""name"": ""Core Set"",
                ""type"": ""core""
            }
        }";

        var resource = JsonConvert.DeserializeObject<JsonApiResource>(json)!;
        var achievementSet = V2ResourceMapper.MapToAchievementSet(resource);

        Assert.Multiple(() =>
        {
            Assert.That(achievementSet.Id, Is.EqualTo(100));
            Assert.That(achievementSet.Name, Is.EqualTo("Core Set"));
            Assert.That(achievementSet.SetType, Is.EqualTo(AchievementSetType.Core));
            Assert.That(achievementSet.IsCore, Is.True);
        });
    }

    [Test]
    public void MapToAchievementSet_MapsBonusType()
    {
        var json = @"{
            ""type"": ""achievement-sets"",
            ""id"": ""101"",
            ""attributes"": {
                ""name"": ""Bonus Challenges"",
                ""type"": 1
            }
        }";

        var resource = JsonConvert.DeserializeObject<JsonApiResource>(json)!;
        var achievementSet = V2ResourceMapper.MapToAchievementSet(resource);

        Assert.Multiple(() =>
        {
            Assert.That(achievementSet.Id, Is.EqualTo(101));
            Assert.That(achievementSet.SetType, Is.EqualTo(AchievementSetType.Bonus));
            Assert.That(achievementSet.IsBonus, Is.True);
        });
    }

    [Test]
    public void MapToAchievementSet_MapsGameRelationship()
    {
        var json = @"{
            ""type"": ""achievement-sets"",
            ""id"": ""100"",
            ""attributes"": {
                ""name"": ""Core Set""
            },
            ""relationships"": {
                ""game"": {
                    ""data"": { ""type"": ""games"", ""id"": ""1234"" }
                }
            }
        }";

        var resource = JsonConvert.DeserializeObject<JsonApiResource>(json)!;
        var achievementSet = V2ResourceMapper.MapToAchievementSet(resource);

        Assert.That(achievementSet.GameId, Is.EqualTo(1234));
    }

    [Test]
    public void MapToAchievementSet_MapsAchievementsFromIncluded()
    {
        var setResource = JsonConvert.DeserializeObject<JsonApiResource>(@"{
            ""type"": ""achievement-sets"",
            ""id"": ""100"",
            ""attributes"": { ""name"": ""Core Set"" },
            ""relationships"": {
                ""achievements"": {
                    ""data"": [
                        { ""type"": ""achievements"", ""id"": ""1"" },
                        { ""type"": ""achievements"", ""id"": ""2"" }
                    ]
                }
            }
        }")!;

        var achievement1 = JsonConvert.DeserializeObject<JsonApiResource>(@"{
            ""type"": ""achievements"",
            ""id"": ""1"",
            ""attributes"": { ""title"": ""First Achievement"", ""points"": 10 }
        }")!;

        var achievement2 = JsonConvert.DeserializeObject<JsonApiResource>(@"{
            ""type"": ""achievements"",
            ""id"": ""2"",
            ""attributes"": { ""title"": ""Second Achievement"", ""points"": 20 }
        }")!;

        var includedIndex = new Dictionary<(string Type, string Id), JsonApiResource>
        {
            { ("achievements", "1"), achievement1 },
            { ("achievements", "2"), achievement2 }
        };

        var achievementSet = V2ResourceMapper.MapToAchievementSet(setResource, includedIndex);

        Assert.Multiple(() =>
        {
            Assert.That(achievementSet.Achievements, Has.Count.EqualTo(2));
            Assert.That(achievementSet.Achievements[0].Title, Is.EqualTo("First Achievement"));
            Assert.That(achievementSet.Achievements[1].Title, Is.EqualTo("Second Achievement"));
            Assert.That(achievementSet.PointsTotal, Is.EqualTo(30));
        });
    }

    [Test]
    public void ExtractAchievementSetsFromIncluded_ExtractsAndSortsSets()
    {
        var gameResource = JsonConvert.DeserializeObject<JsonApiResource>(@"{
            ""type"": ""games"",
            ""id"": ""1234"",
            ""attributes"": { ""title"": ""Test Game"" },
            ""relationships"": {
                ""achievementSets"": {
                    ""data"": [
                        { ""type"": ""achievement-sets"", ""id"": ""101"" },
                        { ""type"": ""achievement-sets"", ""id"": ""100"" }
                    ]
                }
            }
        }")!;

        var coreSet = JsonConvert.DeserializeObject<JsonApiResource>(@"{
            ""type"": ""achievement-sets"",
            ""id"": ""100"",
            ""attributes"": { ""name"": ""Core Set"", ""type"": ""core"" }
        }")!;

        var bonusSet = JsonConvert.DeserializeObject<JsonApiResource>(@"{
            ""type"": ""achievement-sets"",
            ""id"": ""101"",
            ""attributes"": { ""name"": ""Bonus Set"", ""type"": ""bonus"" }
        }")!;

        var includedIndex = new Dictionary<(string Type, string Id), JsonApiResource>
        {
            { ("achievement-sets", "100"), coreSet },
            { ("achievement-sets", "101"), bonusSet }
        };

        var sets = V2ResourceMapper.ExtractAchievementSetsFromIncluded(gameResource, includedIndex);

        Assert.Multiple(() =>
        {
            Assert.That(sets, Has.Count.EqualTo(2));
            // Should be sorted: core first, then bonus
            Assert.That(sets[0].SetType, Is.EqualTo(AchievementSetType.Core));
            Assert.That(sets[1].SetType, Is.EqualTo(AchievementSetType.Bonus));
        });
    }

    #endregion

    #region V2MetadataService Multi-Set Integration Tests

    [Test]
    public async Task GetGameAsync_WithIncludeAchievementSets_ReturnsGameWithSets()
    {
        var jsonResponse = @"{
            ""data"": {
                ""type"": ""games"",
                ""id"": ""1234"",
                ""attributes"": {
                    ""title"": ""Sonic the Hedgehog"",
                    ""developer"": ""Sonic Team""
                },
                ""relationships"": {
                    ""system"": {
                        ""data"": { ""type"": ""systems"", ""id"": ""1"" }
                    },
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
                    ""type"": ""systems"",
                    ""id"": ""1"",
                    ""attributes"": { ""name"": ""Mega Drive/Genesis"" }
                },
                {
                    ""type"": ""achievement-sets"",
                    ""id"": ""100"",
                    ""attributes"": { ""name"": ""Core Set"", ""type"": ""core"" },
                    ""relationships"": {
                        ""achievements"": {
                            ""data"": [
                                { ""type"": ""achievements"", ""id"": ""1"" }
                            ]
                        }
                    }
                },
                {
                    ""type"": ""achievement-sets"",
                    ""id"": ""101"",
                    ""attributes"": { ""name"": ""Bonus Set"", ""type"": ""bonus"" }
                },
                {
                    ""type"": ""achievements"",
                    ""id"": ""1"",
                    ""attributes"": { ""title"": ""Ring Collector"", ""points"": 10 }
                }
            ]
        }";

        SetupMockResponse(jsonResponse);

        using var service = new V2MetadataService(_client);
        var game = await service.GetGameAsync(1234, includeAchievements: false, includeAchievementSets: true);

        Assert.Multiple(() =>
        {
            Assert.That(game, Is.Not.Null);
            Assert.That(game!.Id, Is.EqualTo(1234));
            Assert.That(game.Title, Is.EqualTo("Sonic the Hedgehog"));
            Assert.That(game.AchievementSets, Has.Count.EqualTo(2));
            Assert.That(game.HasMultipleSets, Is.True);
            Assert.That(game.CoreSet, Is.Not.Null);
            Assert.That(game.CoreSet!.Name, Is.EqualTo("Core Set"));
        });
    }

    [Test]
    public async Task GetGameAsync_WithoutAchievementSets_ReturnsGameWithEmptySets()
    {
        var jsonResponse = @"{
            ""data"": {
                ""type"": ""games"",
                ""id"": ""1234"",
                ""attributes"": {
                    ""title"": ""Sonic the Hedgehog""
                },
                ""relationships"": {
                    ""system"": {
                        ""data"": { ""type"": ""systems"", ""id"": ""1"" }
                    }
                }
            },
            ""included"": [
                {
                    ""type"": ""systems"",
                    ""id"": ""1"",
                    ""attributes"": { ""name"": ""Mega Drive/Genesis"" }
                }
            ]
        }";

        SetupMockResponse(jsonResponse);

        using var service = new V2MetadataService(_client);
        var game = await service.GetGameAsync(1234, includeAchievements: false, includeAchievementSets: false);

        Assert.Multiple(() =>
        {
            Assert.That(game, Is.Not.Null);
            Assert.That(game!.AchievementSets, Has.Count.EqualTo(0));
            Assert.That(game.HasMultipleSets, Is.False);
        });
    }

    #endregion

    #region Feature Flag Tests

    [Test]
    public void FeatureFlagService_EnableMultiSet_DefaultsToTrue()
    {
        // Phase 2 complete - multi-set support is enabled by default
        var featureFlags = new FeatureFlagService();
        Assert.That(featureFlags.EnableMultiSet, Is.True);
    }

    [Test]
    public void FeatureFlagService_EnableMultiSet_CanBeDisabled()
    {
        var featureFlags = new FeatureFlagService(enableMultiSet: false);
        Assert.That(featureFlags.EnableMultiSet, Is.False);
    }

    #endregion

    #region Helper Methods

    private void SetupMockResponse(string jsonContent, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/vnd.api+json")
        };

        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    #endregion
}
