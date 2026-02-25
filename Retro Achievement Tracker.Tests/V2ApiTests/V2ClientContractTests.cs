using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using RATracker.WPF.Http.V2;
using RATracker.WPF.Http.V2.JsonApi;
using System.Net;

namespace RATracker.Tests.V2ApiTests;

/// <summary>
/// Contract tests for V2Client to verify JSON:API response parsing and required fields.
/// These tests use mocked HTTP responses to validate the client's contract expectations.
/// </summary>
[TestFixture]
public class V2ClientContractTests
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

    #region Systems Endpoint Contract Tests

    [Test]
    public async Task GetSystemsAsync_ReturnsValidJsonApiDocument_WithCorrectShape()
    {
        // Arrange
        var jsonResponse = @"{
            ""data"": [
                {
                    ""type"": ""systems"",
                    ""id"": ""1"",
                    ""attributes"": {
                        ""name"": ""Mega Drive/Genesis"",
                        ""nameShort"": ""MD"",
                        ""nameFull"": ""Sega Genesis"",
                        ""iconUrl"": ""https://static.retroachievements.org/assets/images/system/md.png"",
                        ""active"": true,
                        ""gamesPublished"": 500,
                        ""achievementsPublished"": 10000
                    },
                    ""links"": {
                        ""self"": ""https://retroachievements.org/api/v2/systems/1""
                    }
                }
            ],
            ""links"": {
                ""self"": ""https://retroachievements.org/api/v2/systems"",
                ""first"": ""https://retroachievements.org/api/v2/systems?page[number]=1"",
                ""last"": ""https://retroachievements.org/api/v2/systems?page[number]=1""
            }
        }";

        SetupMockResponse(jsonResponse);

        // Act
        var result = await _client.GetSystemsAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.HasErrors, Is.False);
            Assert.That(result.Data, Is.Not.Null);

            var resources = result.GetResourceCollection();
            Assert.That(resources, Has.Count.GreaterThanOrEqualTo(1));

            var system = resources.First();
            Assert.That(system.Type, Is.EqualTo("systems"));
            Assert.That(system.Id, Is.Not.Empty);

            // Verify required attributes
            Assert.That(system.GetAttribute<string>("name"), Is.Not.Null.And.Not.Empty);
            Assert.That(system.GetAttribute<bool?>("active"), Is.Not.Null);
        });
    }

    [Test]
    public async Task GetSystemAsync_ReturnsValidSingleResource()
    {
        // Arrange
        var jsonResponse = @"{
            ""data"": {
                ""type"": ""systems"",
                ""id"": ""1"",
                ""attributes"": {
                    ""name"": ""Mega Drive/Genesis"",
                    ""nameShort"": ""MD"",
                    ""active"": true,
                    ""gamesPublished"": 500,
                    ""achievementsPublished"": 10000
                }
            }
        }";

        SetupMockResponse(jsonResponse);

        // Act
        var result = await _client.GetSystemAsync("1");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.HasErrors, Is.False);

            var system = result.GetSingleResource();
            Assert.That(system, Is.Not.Null);
            Assert.That(system!.Type, Is.EqualTo("systems"));
            Assert.That(system.Id, Is.EqualTo("1"));
        });
    }

    #endregion

    #region Games Endpoint Contract Tests

    [Test]
    public async Task GetGamesAsync_ReturnsValidJsonApiDocument_WithGameResources()
    {
        // Arrange
        var jsonResponse = @"{
            ""data"": [
                {
                    ""type"": ""games"",
                    ""id"": ""1234"",
                    ""attributes"": {
                        ""title"": ""Sonic the Hedgehog"",
                        ""badgeUrl"": ""https://media.retroachievements.org/Images/000001.png"",
                        ""imageTitle"": ""https://media.retroachievements.org/Images/000001.png"",
                        ""imageIngame"": ""https://media.retroachievements.org/Images/000002.png"",
                        ""imageBoxArt"": ""https://media.retroachievements.org/Images/000003.png"",
                        ""publisher"": ""Sega"",
                        ""developer"": ""Sonic Team"",
                        ""genre"": ""Platformer"",
                        ""releasedAt"": ""1991""
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
                    ""attributes"": {
                        ""name"": ""Mega Drive/Genesis""
                    }
                }
            ]
        }";

        SetupMockResponse(jsonResponse);

        // Act
        var result = await _client.GetGamesAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.HasErrors, Is.False);

            var resources = result.GetResourceCollection();
            Assert.That(resources, Has.Count.GreaterThanOrEqualTo(1));

            var game = resources.First();
            Assert.That(game.Type, Is.EqualTo("games"));
            Assert.That(game.Id, Is.Not.Empty);

            // Verify required attributes
            Assert.That(game.GetAttribute<string>("title"), Is.Not.Null.And.Not.Empty);

            // Verify system relationship exists
            var systemRelationship = game.GetRelationship("system");
            Assert.That(systemRelationship, Is.Not.Null);
        });
    }

    [Test]
    public async Task GetGameAsync_WithIncludes_ReturnsIncludedResources()
    {
        // Arrange
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
                    },
                    ""achievementSets"": {
                        ""data"": [
                            { ""type"": ""achievement-sets"", ""id"": ""100"" }
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
                    ""attributes"": { ""name"": ""Core Set"" }
                }
            ]
        }";

        SetupMockResponse(jsonResponse);

        var query = V2QueryBuilder.Create()
            .Include("system")
            .Include("achievementSets");

        // Act
        var result = await _client.GetGameAsync("1234", query);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Included, Is.Not.Null);
            Assert.That(result.Included, Has.Count.EqualTo(2));

            var includedIndex = result.GetIncludedIndex();
            Assert.That(includedIndex.ContainsKey(("systems", "1")), Is.True);
            Assert.That(includedIndex.ContainsKey(("achievement-sets", "100")), Is.True);
        });
    }

    #endregion

    #region Users Endpoint Contract Tests

    [Test]
    public async Task GetUserAsync_ReturnsValidUserResource()
    {
        // Arrange
        var jsonResponse = @"{
            ""data"": {
                ""type"": ""users"",
                ""id"": ""01J5Q8V1ABCDEFGHJKLMNPQRST"",
                ""attributes"": {
                    ""username"": ""TestUser"",
                    ""displayName"": ""TestUser"",
                    ""avatarUrl"": ""https://retroachievements.org/UserPic/TestUser.png"",
                    ""motto"": ""Hello World"",
                    ""points"": 15000,
                    ""pointsWeighted"": 45000,
                    ""playerRank"": 1234,
                    ""lastGameId"": 5678
                }
            }
        }";

        SetupMockResponse(jsonResponse);

        // Act
        var result = await _client.GetUserAsync("TestUser");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.HasErrors, Is.False);

            var user = result.GetSingleResource();
            Assert.That(user, Is.Not.Null);
            Assert.That(user!.Type, Is.EqualTo("users"));

            // Verify required attributes
            Assert.That(user.GetAttribute<string>("username") ?? user.GetAttribute<string>("displayName"), Is.Not.Null.And.Not.Empty);
        });
    }

    [Test]
    public async Task GetUsersAsync_WithSearch_ReturnsFilteredResults()
    {
        // Arrange
        var jsonResponse = @"{
            ""data"": [
                {
                    ""type"": ""users"",
                    ""id"": ""01J5Q8V1ABCDEFGHJKLMNPQRST"",
                    ""attributes"": {
                        ""username"": ""TestUser"",
                        ""displayName"": ""TestUser"",
                        ""points"": 15000
                    }
                }
            ],
            ""links"": {
                ""self"": ""https://retroachievements.org/api/v2/users?filter[user]=Test""
            }
        }";

        SetupMockResponse(jsonResponse);

        var query = V2QueryBuilder.Create()
            .Filter("user", "Test");

        // Act
        var result = await _client.GetUsersAsync(query);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.HasErrors, Is.False);
            Assert.That(result.GetResourceCollection(), Has.Count.GreaterThanOrEqualTo(1));
        });
    }

    #endregion

    #region Error Handling Contract Tests

    [Test]
    public void GetAsync_OnNotFound_ThrowsV2ApiExceptionWithCorrectStatusCode()
    {
        // Arrange
        var errorResponse = @"{
            ""errors"": [
                {
                    ""status"": ""404"",
                    ""title"": ""Not Found"",
                    ""detail"": ""The requested resource could not be found.""
                }
            ]
        }";

        SetupMockResponse(errorResponse, HttpStatusCode.NotFound);

        // Act & Assert
        var exception = Assert.ThrowsAsync<V2ApiException>(async () =>
            await _client.GetResourceAsync("games", "999999"));

        Assert.Multiple(() =>
        {
            Assert.That(exception!.StatusCode, Is.EqualTo(404));
            Assert.That(exception.IsNotFound, Is.True);
        });
    }

    [Test]
    public void GetAsync_OnUnauthorized_ThrowsV2ApiExceptionWithAuthenticationError()
    {
        // Arrange
        var errorResponse = @"{
            ""errors"": [
                {
                    ""status"": ""401"",
                    ""title"": ""Unauthorized"",
                    ""detail"": ""Invalid API key provided.""
                }
            ]
        }";

        SetupMockResponse(errorResponse, HttpStatusCode.Unauthorized);

        // Act & Assert
        var exception = Assert.ThrowsAsync<V2ApiException>(async () =>
            await _client.GetSystemsAsync());

        Assert.Multiple(() =>
        {
            Assert.That(exception!.StatusCode, Is.EqualTo(401));
            Assert.That(exception.IsAuthenticationError, Is.True);
        });
    }

    [Test]
    public void GetAsync_OnRateLimited_ThrowsV2RateLimitException()
    {
        // Arrange
        var errorResponse = @"{
            ""errors"": [
                {
                    ""status"": ""429"",
                    ""title"": ""Too Many Requests"",
                    ""detail"": ""Rate limit exceeded. Please wait before making more requests.""
                }
            ]
        }";

        var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
        {
            Content = new StringContent(errorResponse)
        };
        response.Headers.Add("Retry-After", "60");

        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act & Assert
        var exception = Assert.ThrowsAsync<V2RateLimitException>(async () =>
            await _client.GetSystemsAsync());

        Assert.Multiple(() =>
        {
            Assert.That(exception!.StatusCode, Is.EqualTo(429));
            Assert.That(exception.IsRateLimited, Is.True);
            Assert.That(exception.RetryAfterSeconds, Is.EqualTo(60));
        });
    }

    [Test]
    public void GetAsync_OnServerError_ThrowsV2ApiException_WithIsTransientTrue()
    {
        // Arrange
        SetupMockResponse(@"{""errors"": []}", HttpStatusCode.InternalServerError);

        // Act & Assert
        var exception = Assert.ThrowsAsync<V2ApiException>(async () =>
            await _client.GetSystemsAsync());

        Assert.Multiple(() =>
        {
            Assert.That(exception!.StatusCode, Is.EqualTo(500));
            Assert.That(exception.IsTransient, Is.True);
        });
    }

    #endregion

    #region JSON:API Document Structure Tests

    [Test]
    public async Task JsonApiDocument_ParsesMetaCorrectly()
    {
        // Arrange
        var jsonResponse = @"{
            ""data"": [],
            ""meta"": {
                ""total"": 100,
                ""page"": 1,
                ""perPage"": 50
            }
        }";

        SetupMockResponse(jsonResponse);

        // Act
        var result = await _client.GetSystemsAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Meta, Is.Not.Null);
            Assert.That(result.Meta!["total"]?.ToObject<int>(), Is.EqualTo(100));
        });
    }

    [Test]
    public async Task JsonApiDocument_ParsesLinksCorrectly()
    {
        // Arrange
        var jsonResponse = @"{
            ""data"": [],
            ""links"": {
                ""self"": ""https://retroachievements.org/api/v2/systems?page[number]=2"",
                ""first"": ""https://retroachievements.org/api/v2/systems?page[number]=1"",
                ""prev"": ""https://retroachievements.org/api/v2/systems?page[number]=1"",
                ""next"": ""https://retroachievements.org/api/v2/systems?page[number]=3"",
                ""last"": ""https://retroachievements.org/api/v2/systems?page[number]=5""
            }
        }";

        SetupMockResponse(jsonResponse);

        // Act
        var result = await _client.GetSystemsAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Links, Is.Not.Null);
            Assert.That(result.Links!.Self, Does.Contain("page[number]=2"));
            Assert.That(result.Links.First, Does.Contain("page[number]=1"));
            Assert.That(result.Links.Prev, Does.Contain("page[number]=1"));
            Assert.That(result.Links.Next, Does.Contain("page[number]=3"));
            Assert.That(result.Links.Last, Does.Contain("page[number]=5"));
        });
    }

    [Test]
    public async Task JsonApiResource_ParsesRelationshipsCorrectly()
    {
        // Arrange
        var jsonResponse = @"{
            ""data"": {
                ""type"": ""games"",
                ""id"": ""1234"",
                ""attributes"": { ""title"": ""Test Game"" },
                ""relationships"": {
                    ""system"": {
                        ""links"": {
                            ""related"": ""https://retroachievements.org/api/v2/systems/1""
                        },
                        ""data"": { ""type"": ""systems"", ""id"": ""1"" }
                    },
                    ""achievementSets"": {
                        ""data"": [
                            { ""type"": ""achievement-sets"", ""id"": ""100"" },
                            { ""type"": ""achievement-sets"", ""id"": ""101"" }
                        ]
                    }
                }
            }
        }";

        SetupMockResponse(jsonResponse);

        // Act
        var result = await _client.GetGameAsync("1234");
        var game = result.GetSingleResource();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(game, Is.Not.Null);

            var systemRel = game!.GetRelationship("system");
            Assert.That(systemRel, Is.Not.Null);
            Assert.That(systemRel!.GetSingleIdentifier()?.Type, Is.EqualTo("systems"));
            Assert.That(systemRel.GetSingleIdentifier()?.Id, Is.EqualTo("1"));

            var setsRel = game.GetRelationship("achievementSets");
            Assert.That(setsRel, Is.Not.Null);
            var setIds = setsRel!.GetIdentifierCollection();
            Assert.That(setIds, Has.Count.EqualTo(2));
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
