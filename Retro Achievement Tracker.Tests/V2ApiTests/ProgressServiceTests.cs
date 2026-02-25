using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using RATracker.Models;
using RATracker.WPF.Http.V2;
using RATracker.WPF.Services;
using System.Net;

namespace RATracker.Tests.V2ApiTests;

/// <summary>
/// Tests for the progress service implementations.
/// </summary>
[TestFixture]
public class ProgressServiceTests
{
    private Mock<HttpMessageHandler> _mockHandler = null!;
    private HttpClient _httpClient = null!;
    private V2Client _v2Client = null!;

    private const string TestApiKey = "test-api-key";
    private const string TestUsername = "TestUser";
    private const string TestBaseUrl = "https://test.retroachievements.org";

    [SetUp]
    public void SetUp()
    {
        _mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Loose);
        _httpClient = new HttpClient(_mockHandler.Object);
        _v2Client = new V2Client(TestApiKey, _httpClient, TestBaseUrl);
    }

    [TearDown]
    public void TearDown()
    {
        _v2Client.Dispose();
        _httpClient.Dispose();
    }

    #region UserGameProgress Model Tests

    [Test]
    public void UserGameProgress_CompletionPercentage_CalculatesCorrectly()
    {
        var progress = new UserGameProgress
        {
            TotalAchievements = 100,
            EarnedAchievements = 50
        };

        Assert.That(progress.CompletionPercentage, Is.EqualTo(50.0));
        Assert.That(progress.CompletionPercentageFormatted, Is.EqualTo("50.00"));
    }

    [Test]
    public void UserGameProgress_CompletionPercentage_ZeroTotal_ReturnsZero()
    {
        var progress = new UserGameProgress
        {
            TotalAchievements = 0,
            EarnedAchievements = 0
        };

        Assert.That(progress.CompletionPercentage, Is.EqualTo(0.0));
    }

    [Test]
    public void UserGameProgress_IsMastered_ReturnsTrueWhenAllEarned()
    {
        var progress = new UserGameProgress
        {
            TotalAchievements = 10,
            EarnedAchievements = 10
        };

        Assert.That(progress.IsMastered, Is.True);
    }

    [Test]
    public void UserGameProgress_IsMastered_ReturnsFalseWhenNotComplete()
    {
        var progress = new UserGameProgress
        {
            TotalAchievements = 10,
            EarnedAchievements = 9
        };

        Assert.That(progress.IsMastered, Is.False);
    }

    [Test]
    public void UserGameProgress_UnlockedAndLocked_SeparatesCorrectly()
    {
        var progress = new UserGameProgress
        {
            Achievements = new List<Achievement>
            {
                new Achievement { Id = 1, DateEarned = DateTime.Now },
                new Achievement { Id = 2, DateEarned = null },
                new Achievement { Id = 3, DateEarned = DateTime.Now },
                new Achievement { Id = 4, DateEarned = null }
            }
        };

        Assert.Multiple(() =>
        {
            Assert.That(progress.UnlockedAchievements, Has.Count.EqualTo(2));
            Assert.That(progress.LockedAchievements, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public void UserGameProgress_FromGameInfo_MapsCorrectly()
    {
        var gameInfo = new GameInfo
        {
            Id = 1234,
            Title = "Test Game",
            ConsoleName = "Genesis",
            Achievements = new List<Achievement>
            {
                new Achievement { Id = 1, Points = 10, TrueRatio = 20, DateEarned = DateTime.Now },
                new Achievement { Id = 2, Points = 20, TrueRatio = 40, DateEarned = null }
            }
        };

        var progress = UserGameProgress.FromGameInfo(gameInfo, TestUsername);

        Assert.Multiple(() =>
        {
            Assert.That(progress.UserId, Is.EqualTo(TestUsername));
            Assert.That(progress.GameId, Is.EqualTo(1234));
            Assert.That(progress.GameTitle, Is.EqualTo("Test Game"));
            Assert.That(progress.TotalAchievements, Is.EqualTo(2));
            Assert.That(progress.EarnedAchievements, Is.EqualTo(1));
            Assert.That(progress.TotalPoints, Is.EqualTo(30));
            Assert.That(progress.EarnedPoints, Is.EqualTo(10));
        });
    }

    [Test]
    public void UserGameProgress_HasMultipleSets_ReturnsTrueWhenMultipleSets()
    {
        var progress = new UserGameProgress
        {
            AchievementSets = new List<AchievementSetProgress>
            {
                new AchievementSetProgress { SetId = 1 },
                new AchievementSetProgress { SetId = 2 }
            }
        };

        Assert.That(progress.HasMultipleSets, Is.True);
    }

    #endregion

    #region AchievementSetProgress Model Tests

    [Test]
    public void AchievementSetProgress_FromAchievementSet_MapsCorrectly()
    {
        var achievementSet = new AchievementSet
        {
            Id = 100,
            Name = "Core Set",
            SetType = AchievementSetType.Core,
            Achievements = new List<Achievement>
            {
                new Achievement { Id = 1, Points = 10, DateEarned = DateTime.Now },
                new Achievement { Id = 2, Points = 20, DateEarned = null }
            }
        };

        var progress = AchievementSetProgress.FromAchievementSet(achievementSet);

        Assert.Multiple(() =>
        {
            Assert.That(progress.SetId, Is.EqualTo(100));
            Assert.That(progress.SetName, Is.EqualTo("Core Set"));
            Assert.That(progress.SetType, Is.EqualTo(AchievementSetType.Core));
            Assert.That(progress.IsCore, Is.True);
            Assert.That(progress.TotalAchievements, Is.EqualTo(2));
            Assert.That(progress.EarnedAchievements, Is.EqualTo(1));
            Assert.That(progress.TotalPoints, Is.EqualTo(30));
            Assert.That(progress.EarnedPoints, Is.EqualTo(10));
            Assert.That(progress.CompletionPercentage, Is.EqualTo(50.0));
            Assert.That(progress.IsCompleted, Is.False);
        });
    }

    #endregion

    #region RecentAchievement Model Tests

    [Test]
    public void RecentAchievement_ToAchievement_MapsCorrectly()
    {
        var recent = new RecentAchievement
        {
            AchievementId = 123,
            Title = "Test Achievement",
            Description = "Test Description",
            Points = 10,
            TruePoints = 25,
            BadgeUrl = "http://badge.url",
            GameId = 456,
            GameTitle = "Test Game",
            EarnedAt = new DateTime(2024, 1, 15, 12, 0, 0)
        };

        var achievement = recent.ToAchievement();

        Assert.Multiple(() =>
        {
            Assert.That(achievement.Id, Is.EqualTo(123));
            Assert.That(achievement.Title, Is.EqualTo("Test Achievement"));
            Assert.That(achievement.Description, Is.EqualTo("Test Description"));
            Assert.That(achievement.Points, Is.EqualTo(10));
            Assert.That(achievement.TrueRatio, Is.EqualTo(25));
            Assert.That(achievement.BadgeUri, Is.EqualTo("http://badge.url"));
            Assert.That(achievement.GameId, Is.EqualTo(456));
            Assert.That(achievement.GameTitle, Is.EqualTo("Test Game"));
            Assert.That(achievement.DateEarned, Is.EqualTo(new DateTime(2024, 1, 15, 12, 0, 0)));
        });
    }

    #endregion

    #region AchievementUnlockEvent Tests

    [Test]
    public void AchievementUnlockEvent_FromAchievement_CreatesCorrectly()
    {
        var achievement = new Achievement
        {
            Id = 123,
            Title = "Test Achievement",
            Points = 10,
            GameId = 456,
            GameTitle = "Test Game",
            DateEarned = new DateTime(2024, 1, 15)
        };

        var unlockEvent = AchievementUnlockEvent.FromAchievement(achievement, TestUsername);

        Assert.Multiple(() =>
        {
            Assert.That(unlockEvent.Achievement, Is.EqualTo(achievement));
            Assert.That(unlockEvent.GameId, Is.EqualTo(456));
            Assert.That(unlockEvent.GameTitle, Is.EqualTo("Test Game"));
            Assert.That(unlockEvent.UnlockedAt, Is.EqualTo(new DateTime(2024, 1, 15)));
            Assert.That(unlockEvent.UserId, Is.EqualTo(TestUsername));
        });
    }

    #endregion

    #region ProgressStateTracker Tests

    [Test]
    public void ProgressStateTracker_DetectChanges_DetectsNewUnlocks()
    {
        var tracker = new ProgressStateTracker();

        // Initialize with first state
        var initialProgress = new UserGameProgress
        {
            UserId = TestUsername,
            GameId = 1234,
            TotalAchievements = 3,
            Achievements = new List<Achievement>
            {
                new Achievement { Id = 1, DateEarned = DateTime.Now },
                new Achievement { Id = 2, DateEarned = null },
                new Achievement { Id = 3, DateEarned = null }
            }
        };
        tracker.InitializeWithProgress(initialProgress);

        // Update with new unlock
        var updatedProgress = new UserGameProgress
        {
            UserId = TestUsername,
            GameId = 1234,
            TotalAchievements = 3,
            Achievements = new List<Achievement>
            {
                new Achievement { Id = 1, Title = "First", DateEarned = DateTime.Now },
                new Achievement { Id = 2, Title = "Second", DateEarned = DateTime.Now },
                new Achievement { Id = 3, DateEarned = null }
            }
        };

        var result = tracker.DetectChanges(updatedProgress);

        Assert.Multiple(() =>
        {
            Assert.That(result.HasNewUnlocks, Is.True);
            Assert.That(result.NewUnlocks, Has.Count.EqualTo(1));
            Assert.That(result.NewUnlocks[0].Achievement.Id, Is.EqualTo(2));
            Assert.That(result.GameMastered, Is.False);
            Assert.That(result.JustMastered, Is.False);
        });
    }

    [Test]
    public void ProgressStateTracker_DetectChanges_DetectsMastery()
    {
        var tracker = new ProgressStateTracker();

        // Initialize with one locked
        var initialProgress = new UserGameProgress
        {
            UserId = TestUsername,
            GameId = 1234,
            TotalAchievements = 2,
            Achievements = new List<Achievement>
            {
                new Achievement { Id = 1, DateEarned = DateTime.Now },
                new Achievement { Id = 2, DateEarned = null }
            }
        };
        tracker.InitializeWithProgress(initialProgress);

        // Update with mastery
        var masteredProgress = new UserGameProgress
        {
            UserId = TestUsername,
            GameId = 1234,
            TotalAchievements = 2,
            Achievements = new List<Achievement>
            {
                new Achievement { Id = 1, Title = "First", DateEarned = DateTime.Now },
                new Achievement { Id = 2, Title = "Second", DateEarned = DateTime.Now }
            }
        };

        var result = tracker.DetectChanges(masteredProgress);

        Assert.Multiple(() =>
        {
            Assert.That(result.HasNewUnlocks, Is.True);
            Assert.That(result.GameMastered, Is.True);
            Assert.That(result.JustMastered, Is.True);
            Assert.That(result.NewUnlocks.Last().TriggeredMastery, Is.True);
        });
    }

    [Test]
    public void ProgressStateTracker_DetectChanges_NoUnlocksWhenSameState()
    {
        var tracker = new ProgressStateTracker();

        var progress = new UserGameProgress
        {
            UserId = TestUsername,
            GameId = 1234,
            TotalAchievements = 2,
            Achievements = new List<Achievement>
            {
                new Achievement { Id = 1, DateEarned = DateTime.Now },
                new Achievement { Id = 2, DateEarned = null }
            }
        };

        tracker.InitializeWithProgress(progress);
        var result = tracker.DetectChanges(progress);

        Assert.That(result.HasNewUnlocks, Is.False);
    }

    [Test]
    public void ProgressStateTracker_ResetAll_ClearsState()
    {
        var tracker = new ProgressStateTracker();

        var progress = new UserGameProgress
        {
            UserId = TestUsername,
            GameId = 1234,
            TotalAchievements = 1,
            Achievements = new List<Achievement>
            {
                new Achievement { Id = 1, DateEarned = DateTime.Now }
            }
        };

        tracker.InitializeWithProgress(progress);
        tracker.ResetAll();

        Assert.That(tracker.PreviousGameId, Is.Null);
    }

    #endregion

    #region V2ProgressService Tests

    [Test]
    public async Task V2ProgressService_GetUserRecentAchievements_ReturnsAchievements()
    {
        var jsonResponse = @"{
            ""data"": [
                {
                    ""type"": ""achievements"",
                    ""id"": ""123"",
                    ""attributes"": {
                        ""title"": ""Test Achievement"",
                        ""description"": ""Test Description"",
                        ""points"": 10,
                        ""pointsWeighted"": 25,
                        ""badgeUrl"": ""http://badge.url"",
                        ""earnedAt"": ""2024-01-15T12:00:00Z""
                    },
                    ""relationships"": {
                        ""game"": {
                            ""data"": { ""type"": ""games"", ""id"": ""456"" }
                        }
                    }
                }
            ],
            ""included"": [
                {
                    ""type"": ""games"",
                    ""id"": ""456"",
                    ""attributes"": { ""title"": ""Test Game"" }
                }
            ]
        }";

        SetupMockResponse(jsonResponse);

        using var service = new V2ProgressService(_v2Client);
        var achievements = await service.GetUserRecentAchievementsAsync(TestUsername, 10);

        Assert.Multiple(() =>
        {
            Assert.That(achievements, Has.Count.EqualTo(1));
            Assert.That(achievements[0].AchievementId, Is.EqualTo(123));
            Assert.That(achievements[0].Title, Is.EqualTo("Test Achievement"));
            Assert.That(achievements[0].Points, Is.EqualTo(10));
            Assert.That(achievements[0].GameId, Is.EqualTo(456));
            Assert.That(achievements[0].GameTitle, Is.EqualTo("Test Game"));
        });
    }

    [Test]
    public async Task V2ProgressService_GetUserRecentlyPlayedGames_ReturnsGames()
    {
        var jsonResponse = @"{
            ""data"": [
                {
                    ""type"": ""games"",
                    ""id"": ""456"",
                    ""attributes"": {
                        ""title"": ""Test Game"",
                        ""badgeUrl"": ""http://badge.url"",
                        ""achievementsUnlocked"": 5,
                        ""achievementsTotal"": 10,
                        ""lastPlayedAt"": ""2024-01-15T12:00:00Z""
                    },
                    ""relationships"": {
                        ""system"": {
                            ""data"": { ""type"": ""systems"", ""id"": ""1"" }
                        }
                    }
                }
            ],
            ""included"": [
                {
                    ""type"": ""systems"",
                    ""id"": ""1"",
                    ""attributes"": { ""name"": ""Genesis"" }
                }
            ]
        }";

        SetupMockResponse(jsonResponse);

        using var service = new V2ProgressService(_v2Client);
        var games = await service.GetUserRecentlyPlayedGamesAsync(TestUsername, 10);

        Assert.Multiple(() =>
        {
            Assert.That(games, Has.Count.EqualTo(1));
            Assert.That(games[0].GameId, Is.EqualTo(456));
            Assert.That(games[0].Title, Is.EqualTo("Test Game"));
            Assert.That(games[0].EarnedAchievements, Is.EqualTo(5));
            Assert.That(games[0].TotalAchievements, Is.EqualTo(10));
            Assert.That(games[0].ConsoleName, Is.EqualTo("Genesis"));
            Assert.That(games[0].CompletionPercentage, Is.EqualTo(50.0));
        });
    }

    [Test]
    public void V2ProgressService_DetectNewUnlocks_DetectsChanges()
    {
        using var service = new V2ProgressService(TestApiKey);

        var previousProgress = new UserGameProgress
        {
            UserId = TestUsername,
            Achievements = new List<Achievement>
            {
                new Achievement { Id = 1, DateEarned = DateTime.Now }
            }
        };

        var currentProgress = new UserGameProgress
        {
            UserId = TestUsername,
            TotalAchievements = 2,
            Achievements = new List<Achievement>
            {
                new Achievement { Id = 1, Title = "First", DateEarned = DateTime.Now },
                new Achievement { Id = 2, Title = "Second", DateEarned = DateTime.Now }
            }
        };

        var result = service.DetectNewUnlocks(currentProgress, previousProgress);

        Assert.Multiple(() =>
        {
            Assert.That(result.HasNewUnlocks, Is.True);
            Assert.That(result.NewUnlocks, Has.Count.EqualTo(1));
            Assert.That(result.NewUnlocks[0].Achievement.Id, Is.EqualTo(2));
        });
    }

    [Test]
    public void V2ProgressService_DetectNewUnlocksFromRecent_FiltersCorrectly()
    {
        using var service = new V2ProgressService(TestApiKey);

        var recentAchievements = new List<RecentAchievement>
        {
            new RecentAchievement { AchievementId = 1, Title = "Old" },
            new RecentAchievement { AchievementId = 2, Title = "New1" },
            new RecentAchievement { AchievementId = 3, Title = "New2" }
        };

        var previousUnlockedIds = new HashSet<int> { 1 };

        var newUnlocks = service.DetectNewUnlocksFromRecent(recentAchievements, previousUnlockedIds, TestUsername);

        Assert.Multiple(() =>
        {
            Assert.That(newUnlocks, Has.Count.EqualTo(2));
            Assert.That(newUnlocks.All(u => u.Achievement.Id != 1), Is.True);
        });
    }

    #endregion

    #region HybridProgressService Tests

    [Test]
    public void HybridProgressService_UsesV1WhenV2Disabled()
    {
        var featureFlags = new FeatureFlagService(
            useV2ForProgress: false,
            enableV1Fallback: true
        );

        using var service = new HybridProgressService(TestUsername, TestApiKey, featureFlags);

        // Service should be created without errors
        Assert.That(service, Is.Not.Null);
    }

    [Test]
    public void HybridProgressService_UsesV2WhenEnabled()
    {
        var featureFlags = new FeatureFlagService(
            useV2ForProgress: true,
            enableV1Fallback: true
        );

        using var service = new HybridProgressService(TestUsername, TestApiKey, featureFlags);

        // Service should be created without errors
        Assert.That(service, Is.Not.Null);
    }

    [Test]
    public void HybridProgressService_DetectNewUnlocks_WorksWithoutApiCall()
    {
        var featureFlags = new FeatureFlagService(
            useV2ForProgress: false,
            enableV1Fallback: true
        );

        using var service = new HybridProgressService(TestUsername, TestApiKey, featureFlags);

        var previousProgress = new UserGameProgress
        {
            UserId = TestUsername,
            Achievements = new List<Achievement>
            {
                new Achievement { Id = 1, DateEarned = DateTime.Now }
            }
        };

        var currentProgress = new UserGameProgress
        {
            UserId = TestUsername,
            TotalAchievements = 2,
            Achievements = new List<Achievement>
            {
                new Achievement { Id = 1, Title = "First", DateEarned = DateTime.Now },
                new Achievement { Id = 2, Title = "Second", DateEarned = DateTime.Now }
            }
        };

        var result = service.DetectNewUnlocks(currentProgress, previousProgress);

        Assert.That(result.HasNewUnlocks, Is.True);
    }

    #endregion

    #region FeatureFlag Tests

    [Test]
    public void FeatureFlagService_DefaultValues_AreCorrect()
    {
        var featureFlags = new FeatureFlagService();

        Assert.Multiple(() =>
        {
            Assert.That(featureFlags.UseV2ForMetadata, Is.True);
            Assert.That(featureFlags.UseV2ForProgress, Is.False);
            Assert.That(featureFlags.UseV2ForUserLookup, Is.False);
            Assert.That(featureFlags.EnableMultiSet, Is.False);
            Assert.That(featureFlags.EnableV1Fallback, Is.True);
        });
    }

    [Test]
    public void FeatureFlagService_CustomValues_AreSet()
    {
        var featureFlags = new FeatureFlagService(
            useV2ForProgress: true,
            enableMultiSet: true
        );

        Assert.Multiple(() =>
        {
            Assert.That(featureFlags.UseV2ForProgress, Is.True);
            Assert.That(featureFlags.EnableMultiSet, Is.True);
        });
    }

    #endregion

    #region ProgressResult Tests

    [Test]
    public void ProgressResult_Ok_CreatesSuccessResult()
    {
        var data = new UserGameProgress { GameId = 1234 };
        var result = ProgressResult<UserGameProgress>.Ok(data, ApiVersion.V2);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.EqualTo(data));
            Assert.That(result.ApiVersionUsed, Is.EqualTo(ApiVersion.V2));
            Assert.That(result.UsedFallback, Is.False);
        });
    }

    [Test]
    public void ProgressResult_Fail_CreatesFailureResult()
    {
        var result = ProgressResult<UserGameProgress>.Fail("Test error", ApiVersion.V2);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Test error"));
            Assert.That(result.ApiVersionUsed, Is.EqualTo(ApiVersion.V2));
        });
    }

    [Test]
    public void ProgressResult_Ok_WithFallback_SetsFlag()
    {
        var data = new UserGameProgress { GameId = 1234 };
        var result = ProgressResult<UserGameProgress>.Ok(data, ApiVersion.V1, usedFallback: true);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.ApiVersionUsed, Is.EqualTo(ApiVersion.V1));
            Assert.That(result.UsedFallback, Is.True);
        });
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
