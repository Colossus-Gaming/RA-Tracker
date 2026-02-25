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
/// Tests for hub/collection support in the V2 API.
/// </summary>
[TestFixture]
public class HubTests
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

    #region HubInfo Model Tests

    [Test]
    public void HubInfo_DefaultValues_AreCorrect()
    {
        var hub = new HubInfo();

        Assert.Multiple(() =>
        {
            Assert.That(hub.Id, Is.EqualTo(0));
            Assert.That(hub.Name, Is.Empty);
            Assert.That(hub.Description, Is.Empty);
            Assert.That(hub.BadgeUrl, Is.Empty);
            Assert.That(hub.GameCount, Is.EqualTo(0));
            Assert.That(hub.Games, Is.Not.Null);
            Assert.That(hub.Games, Is.Empty);
            Assert.That(hub.Links, Is.Not.Null);
            Assert.That(hub.Links, Is.Empty);
            Assert.That(hub.ParentHubId, Is.Null);
        });
    }

    [Test]
    public void HubLink_DefaultValues_AreCorrect()
    {
        var link = new HubLink();

        Assert.Multiple(() =>
        {
            Assert.That(link.LinkedHubId, Is.EqualTo(0));
            Assert.That(link.LinkedHubName, Is.Empty);
            Assert.That(link.LinkType, Is.Empty);
        });
    }

    #endregion

    #region V2ResourceMapper Hub Tests

    [Test]
    public void MapToHubInfo_MapsBasicProperties()
    {
        var json = @"{
            ""type"": ""hubs"",
            ""id"": ""500"",
            ""attributes"": {
                ""name"": ""Mario Series"",
                ""description"": ""All Mario games"",
                ""badgeUrl"": ""https://media.retroachievements.org/Hubs/500.png"",
                ""gamesCount"": 150,
                ""achievementsCount"": 5000,
                ""pointsTotal"": 75000
            }
        }";

        var resource = JsonConvert.DeserializeObject<JsonApiResource>(json)!;
        var hub = V2ResourceMapper.MapToHubInfo(resource);

        Assert.Multiple(() =>
        {
            Assert.That(hub.Id, Is.EqualTo(500));
            Assert.That(hub.Name, Is.EqualTo("Mario Series"));
            Assert.That(hub.Description, Is.EqualTo("All Mario games"));
            Assert.That(hub.BadgeUrl, Is.EqualTo("https://media.retroachievements.org/Hubs/500.png"));
            Assert.That(hub.GameCount, Is.EqualTo(150));
            Assert.That(hub.AchievementCount, Is.EqualTo(5000));
            Assert.That(hub.PointsTotal, Is.EqualTo(75000));
        });
    }

    [Test]
    public void MapToHubInfo_MapsAlternateAttributeNames()
    {
        var json = @"{
            ""type"": ""hubs"",
            ""id"": ""500"",
            ""attributes"": {
                ""title"": ""Mario Series"",
                ""iconUrl"": ""https://media.retroachievements.org/Hubs/500.png"",
                ""gameCount"": 150,
                ""achievementCount"": 5000
            }
        }";

        var resource = JsonConvert.DeserializeObject<JsonApiResource>(json)!;
        var hub = V2ResourceMapper.MapToHubInfo(resource);

        Assert.Multiple(() =>
        {
            Assert.That(hub.Name, Is.EqualTo("Mario Series"));
            Assert.That(hub.BadgeUrl, Is.EqualTo("https://media.retroachievements.org/Hubs/500.png"));
            Assert.That(hub.GameCount, Is.EqualTo(150));
            Assert.That(hub.AchievementCount, Is.EqualTo(5000));
        });
    }

    [Test]
    public void MapToHubInfo_MapsParentRelationship()
    {
        var json = @"{
            ""type"": ""hubs"",
            ""id"": ""501"",
            ""attributes"": {
                ""name"": ""Super Mario World""
            },
            ""relationships"": {
                ""parent"": {
                    ""data"": { ""type"": ""hubs"", ""id"": ""500"" }
                }
            }
        }";

        var resource = JsonConvert.DeserializeObject<JsonApiResource>(json)!;
        var hub = V2ResourceMapper.MapToHubInfo(resource);

        Assert.That(hub.ParentHubId, Is.EqualTo(500));
    }

    [Test]
    public void MapToHubInfoList_MapsMultipleHubs()
    {
        var document = JsonConvert.DeserializeObject<JsonApiDocument>(@"{
            ""data"": [
                {
                    ""type"": ""hubs"",
                    ""id"": ""500"",
                    ""attributes"": { ""name"": ""Mario Series"" }
                },
                {
                    ""type"": ""hubs"",
                    ""id"": ""501"",
                    ""attributes"": { ""name"": ""Zelda Series"" }
                }
            ]
        }")!;

        var hubs = V2ResourceMapper.MapToHubInfoList(document);

        Assert.Multiple(() =>
        {
            Assert.That(hubs, Has.Count.EqualTo(2));
            Assert.That(hubs[0].Name, Is.EqualTo("Mario Series"));
            Assert.That(hubs[1].Name, Is.EqualTo("Zelda Series"));
        });
    }

    #endregion

    #region V2MetadataService Hub Integration Tests

    [Test]
    public async Task GetHubsAsync_ReturnsHubList()
    {
        var jsonResponse = @"{
            ""data"": [
                {
                    ""type"": ""hubs"",
                    ""id"": ""500"",
                    ""attributes"": {
                        ""name"": ""Mario Series"",
                        ""gamesCount"": 150
                    }
                },
                {
                    ""type"": ""hubs"",
                    ""id"": ""501"",
                    ""attributes"": {
                        ""name"": ""Zelda Series"",
                        ""gamesCount"": 75
                    }
                }
            ],
            ""links"": {
                ""self"": ""https://retroachievements.org/api/v2/hubs""
            }
        }";

        SetupMockResponse(jsonResponse);

        using var service = new V2MetadataService(_client);
        var hubs = await service.GetHubsAsync();

        Assert.Multiple(() =>
        {
            Assert.That(hubs, Has.Count.EqualTo(2));
            Assert.That(hubs[0].Name, Is.EqualTo("Mario Series"));
            Assert.That(hubs[0].GameCount, Is.EqualTo(150));
            Assert.That(hubs[1].Name, Is.EqualTo("Zelda Series"));
        });
    }

    [Test]
    public async Task GetHubAsync_ReturnsHubInfo()
    {
        var jsonResponse = @"{
            ""data"": {
                ""type"": ""hubs"",
                ""id"": ""500"",
                ""attributes"": {
                    ""name"": ""Mario Series"",
                    ""description"": ""All Mario games across all platforms"",
                    ""gamesCount"": 150,
                    ""achievementsCount"": 5000
                }
            }
        }";

        SetupMockResponse(jsonResponse);

        using var service = new V2MetadataService(_client);
        var hub = await service.GetHubAsync(500);

        Assert.Multiple(() =>
        {
            Assert.That(hub, Is.Not.Null);
            Assert.That(hub!.Id, Is.EqualTo(500));
            Assert.That(hub.Name, Is.EqualTo("Mario Series"));
            Assert.That(hub.Description, Is.EqualTo("All Mario games across all platforms"));
        });
    }

    [Test]
    public async Task GetHubAsync_ReturnsNullForNotFound()
    {
        SetupMockResponse(@"{""errors"": [{""status"": ""404"", ""title"": ""Not Found""}]}", HttpStatusCode.NotFound);

        using var service = new V2MetadataService(_client);
        var hub = await service.GetHubAsync(99999);
        
        Assert.That(hub, Is.Null);
    }

    [Test]
    public async Task GetHubGamesAsync_ReturnsGameList()
    {
        var jsonResponse = @"{
            ""data"": [
                {
                    ""type"": ""games"",
                    ""id"": ""1234"",
                    ""attributes"": {
                        ""title"": ""Super Mario World""
                    },
                    ""relationships"": {
                        ""system"": {
                            ""data"": { ""type"": ""systems"", ""id"": ""3"" }
                        }
                    }
                },
                {
                    ""type"": ""games"",
                    ""id"": ""1235"",
                    ""attributes"": {
                        ""title"": ""Super Mario Bros.""
                    },
                    ""relationships"": {
                        ""system"": {
                            ""data"": { ""type"": ""systems"", ""id"": ""7"" }
                        }
                    }
                }
            ],
            ""included"": [
                {
                    ""type"": ""systems"",
                    ""id"": ""3"",
                    ""attributes"": { ""name"": ""SNES"" }
                },
                {
                    ""type"": ""systems"",
                    ""id"": ""7"",
                    ""attributes"": { ""name"": ""NES"" }
                }
            ]
        }";

        SetupMockResponse(jsonResponse);

        using var service = new V2MetadataService(_client);
        var games = await service.GetHubGamesAsync(500);

        Assert.Multiple(() =>
        {
            Assert.That(games, Has.Count.EqualTo(2));
            Assert.That(games[0].Title, Is.EqualTo("Super Mario World"));
            Assert.That(games[1].Title, Is.EqualTo("Super Mario Bros."));
        });
    }

    [Test]
    public async Task GetHubGamesAsync_WithPagination_SendsCorrectParameters()
    {
        var jsonResponse = @"{
            ""data"": [],
            ""links"": { ""self"": ""https://retroachievements.org/api/v2/hubs/500/games?page[number]=2&page[size]=25"" }
        }";

        SetupMockResponse(jsonResponse);

        using var service = new V2MetadataService(_client);
        await service.GetHubGamesAsync(500, page: 2, pageSize: 25);

        _mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.RequestUri!.ToString().Contains("page[number]=2") &&
                req.RequestUri.ToString().Contains("page[size]=25")),
            ItExpr.IsAny<CancellationToken>());
    }

    #endregion

    #region V2Client Hub Endpoint Tests

    [Test]
    public async Task V2Client_GetHubsAsync_CallsCorrectEndpoint()
    {
        var jsonResponse = @"{ ""data"": [] }";
        SetupMockResponse(jsonResponse);

        await _client.GetHubsAsync();

        _mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.RequestUri!.ToString().Contains("/api/v2/hubs")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Test]
    public async Task V2Client_GetHubAsync_CallsCorrectEndpoint()
    {
        var jsonResponse = @"{ ""data"": { ""type"": ""hubs"", ""id"": ""500"" } }";
        SetupMockResponse(jsonResponse);

        await _client.GetHubAsync("500");

        _mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.RequestUri!.ToString().Contains("/api/v2/hubs/500")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Test]
    public async Task V2Client_GetHubGamesAsync_CallsCorrectEndpoint()
    {
        var jsonResponse = @"{ ""data"": [] }";
        SetupMockResponse(jsonResponse);

        await _client.GetHubGamesAsync("500");

        _mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.RequestUri!.ToString().Contains("/api/v2/hubs/500/games")),
            ItExpr.IsAny<CancellationToken>());
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
