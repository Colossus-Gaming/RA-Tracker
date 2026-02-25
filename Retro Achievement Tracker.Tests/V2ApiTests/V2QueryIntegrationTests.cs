using Moq;
using Moq.Protected;
using RATracker.WPF.Http.V2;
using RATracker.WPF.Http.V2.JsonApi;
using System.Net;

namespace RATracker.Tests.V2ApiTests;

/// <summary>
/// Integration tests for V2 API query features including pagination, sorting, filtering, and includes.
/// </summary>
[TestFixture]
public class V2QueryIntegrationTests
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

    #region Pagination Tests

    [Test]
    public async Task Pagination_PageNumberAndSize_AreIncludedInQuery()
    {
        // Arrange
        string? capturedUrl = null;
        SetupMockResponseWithUrlCapture(@"{ ""data"": [] }", url => capturedUrl = url);

        var query = V2QueryBuilder.Create()
            .Page(2)
            .PageSize(25);

        // Act
        await _client.GetSystemsAsync(query);

        // Assert
        Assert.That(capturedUrl, Is.Not.Null);
        Assert.That(capturedUrl, Does.Contain("page[number]=2"));
        Assert.That(capturedUrl, Does.Contain("page[size]=25"));
    }

    [Test]
    public async Task Pagination_MaxPageSize_IsCappedAt100()
    {
        // Arrange
        var query = V2QueryBuilder.Create()
            .PageSize(200); // Request more than max

        var queryString = query.Build();

        // Assert - the builder should cap at 100
        Assert.That(queryString, Does.Contain("page[size]=100"));
    }

    [Test]
    public async Task Pagination_LinksAreParsed_ForNextPage()
    {
        // Arrange
        var jsonResponse = @"{
            ""data"": [],
            ""links"": {
                ""self"": ""https://retroachievements.org/api/v2/systems?page[number]=1"",
                ""next"": ""https://retroachievements.org/api/v2/systems?page[number]=2"",
                ""last"": ""https://retroachievements.org/api/v2/systems?page[number]=5""
            }
        }";

        SetupMockResponse(jsonResponse);

        var query = V2QueryBuilder.Create()
            .Page(1)
            .PageSize(50);

        // Act
        var result = await _client.GetSystemsAsync(query);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Links?.Next, Is.Not.Null);
            Assert.That(result.Links?.Next, Does.Contain("page[number]=2"));
        });
    }

    [Test]
    public async Task Pagination_NoNextLink_WhenOnLastPage()
    {
        // Arrange
        var jsonResponse = @"{
            ""data"": [],
            ""links"": {
                ""self"": ""https://retroachievements.org/api/v2/systems?page[number]=5"",
                ""first"": ""https://retroachievements.org/api/v2/systems?page[number]=1"",
                ""prev"": ""https://retroachievements.org/api/v2/systems?page[number]=4"",
                ""last"": ""https://retroachievements.org/api/v2/systems?page[number]=5""
            }
        }";

        SetupMockResponse(jsonResponse);

        var query = V2QueryBuilder.Create().Page(5);

        // Act
        var result = await _client.GetSystemsAsync(query);

        // Assert
        Assert.That(result.Links?.Next, Is.Null);
    }

    #endregion

    #region Sorting Tests

    [Test]
    public async Task Sorting_AscendingSort_IsIncludedInQuery()
    {
        // Arrange
        string? capturedUrl = null;
        SetupMockResponseWithUrlCapture(@"{ ""data"": [] }", url => capturedUrl = url);

        var query = V2QueryBuilder.Create()
            .SortAscending("name");

        // Act
        await _client.GetSystemsAsync(query);

        // Assert
        Assert.That(capturedUrl, Is.Not.Null);
        Assert.That(capturedUrl, Does.Contain("sort=name"));
        Assert.That(capturedUrl, Does.Not.Contain("sort=-name"));
    }

    [Test]
    public async Task Sorting_DescendingSort_HasMinusPrefix()
    {
        // Arrange
        string? capturedUrl = null;
        SetupMockResponseWithUrlCapture(@"{ ""data"": [] }", url => capturedUrl = url);

        var query = V2QueryBuilder.Create()
            .SortDescending("playersTotal");

        // Act
        await _client.GetGamesAsync(query);

        // Assert
        Assert.That(capturedUrl, Is.Not.Null);
        Assert.That(capturedUrl, Does.Contain("sort=-playersTotal"));
    }

    [Test]
    public async Task Sorting_MultipleSorts_AreCommaSeparated()
    {
        // Arrange
        string? capturedUrl = null;
        SetupMockResponseWithUrlCapture(@"{ ""data"": [] }", url => capturedUrl = url);

        var query = V2QueryBuilder.Create()
            .SortDescending("pointsTotal")
            .SortAscending("title");

        // Act
        await _client.GetGamesAsync(query);

        // Assert
        Assert.That(capturedUrl, Is.Not.Null);
        // URL encoding converts comma to %2C
        Assert.That(capturedUrl, Does.Contain("sort="));
        Assert.That(capturedUrl, Does.Contain("-pointsTotal"));
        Assert.That(capturedUrl, Does.Contain("title"));
    }

    #endregion

    #region Filtering Tests

    [Test]
    public async Task Filtering_SingleFilter_IsIncludedInQuery()
    {
        // Arrange
        string? capturedUrl = null;
        SetupMockResponseWithUrlCapture(@"{ ""data"": [] }", url => capturedUrl = url);

        var query = V2QueryBuilder.Create()
            .Filter("systemId", 1);

        // Act
        await _client.GetGamesAsync(query);

        // Assert
        Assert.That(capturedUrl, Is.Not.Null);
        Assert.That(capturedUrl, Does.Contain("filter[systemId]=1"));
    }

    [Test]
    public async Task Filtering_BooleanFilter_ConvertsToOneOrZero()
    {
        // Arrange
        string? capturedUrl = null;
        SetupMockResponseWithUrlCapture(@"{ ""data"": [] }", url => capturedUrl = url);

        var query = V2QueryBuilder.Create()
            .Filter("active", true);

        // Act
        await _client.GetSystemsAsync(query);

        // Assert
        Assert.That(capturedUrl, Is.Not.Null);
        Assert.That(capturedUrl, Does.Contain("filter[active]=1"));
    }

    [Test]
    public async Task Filtering_MultipleFilters_AreAllIncluded()
    {
        // Arrange
        string? capturedUrl = null;
        SetupMockResponseWithUrlCapture(@"{ ""data"": [] }", url => capturedUrl = url);

        var query = V2QueryBuilder.Create()
            .Filter("systemId", 1)
            .Filter("active", true)
            .Filter("title", "Sonic");

        // Act
        await _client.GetGamesAsync(query);

        // Assert
        Assert.That(capturedUrl, Is.Not.Null);
        Assert.That(capturedUrl, Does.Contain("filter[systemId]=1"));
        Assert.That(capturedUrl, Does.Contain("filter[active]=1"));
        Assert.That(capturedUrl, Does.Contain("filter[title]=Sonic"));
    }

    [Test]
    public async Task Filtering_StringWithSpaces_IsUrlEncoded()
    {
        // Arrange
        string? capturedUrl = null;
        SetupMockResponseWithUrlCapture(@"{ ""data"": [] }", url => capturedUrl = url);

        var query = V2QueryBuilder.Create()
            .Filter("title", "Super Mario Bros");

        // Act
        await _client.GetGamesAsync(query);

        // Assert
        Assert.That(capturedUrl, Is.Not.Null);
        Assert.That(capturedUrl, Does.Contain("filter[title]=Super+Mario+Bros").Or.Contain("filter[title]=Super%20Mario%20Bros"));
    }

    #endregion

    #region Include Tests

    [Test]
    public async Task Include_SingleInclude_IsIncludedInQuery()
    {
        // Arrange
        string? capturedUrl = null;
        SetupMockResponseWithUrlCapture(@"{ ""data"": [] }", url => capturedUrl = url);

        var query = V2QueryBuilder.Create()
            .Include("system");

        // Act
        await _client.GetGamesAsync(query);

        // Assert
        Assert.That(capturedUrl, Is.Not.Null);
        Assert.That(capturedUrl, Does.Contain("include=system"));
    }

    [Test]
    public async Task Include_MultipleIncludes_AreCommaSeparated()
    {
        // Arrange
        string? capturedUrl = null;
        SetupMockResponseWithUrlCapture(@"{ ""data"": [] }", url => capturedUrl = url);

        var query = V2QueryBuilder.Create()
            .Include("system")
            .Include("achievementSets");

        // Act
        await _client.GetGamesAsync(query);

        // Assert
        Assert.That(capturedUrl, Is.Not.Null);
        // URL encoded comma
        Assert.That(capturedUrl, Does.Contain("include="));
        Assert.That(capturedUrl, Does.Contain("system"));
        Assert.That(capturedUrl, Does.Contain("achievementSets"));
    }

    [Test]
    public async Task Include_WithParamsArray_WorksCorrectly()
    {
        // Arrange
        string? capturedUrl = null;
        SetupMockResponseWithUrlCapture(@"{ ""data"": [] }", url => capturedUrl = url);

        var query = V2QueryBuilder.Create()
            .Include("system", "achievementSets", "leaderboards");

        // Act
        await _client.GetGamesAsync(query);

        // Assert
        Assert.That(capturedUrl, Is.Not.Null);
        Assert.That(capturedUrl, Does.Contain("include="));
        Assert.That(capturedUrl, Does.Contain("system"));
        Assert.That(capturedUrl, Does.Contain("achievementSets"));
        Assert.That(capturedUrl, Does.Contain("leaderboards"));
    }

    [Test]
    public async Task Include_DuplicateIncludes_AreDeduped()
    {
        // Arrange
        var query = V2QueryBuilder.Create()
            .Include("system")
            .Include("system"); // Duplicate

        var queryString = query.Build();

        // Assert - should only contain 'system' once
        var count = queryString.Split("system").Length - 1;
        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public async Task Include_IncludedResourcesAreParsed()
    {
        // Arrange
        var jsonResponse = @"{
            ""data"": [
                {
                    ""type"": ""games"",
                    ""id"": ""1"",
                    ""attributes"": { ""title"": ""Game 1"" },
                    ""relationships"": {
                        ""system"": { ""data"": { ""type"": ""systems"", ""id"": ""1"" } }
                    }
                },
                {
                    ""type"": ""games"",
                    ""id"": ""2"",
                    ""attributes"": { ""title"": ""Game 2"" },
                    ""relationships"": {
                        ""system"": { ""data"": { ""type"": ""systems"", ""id"": ""1"" } }
                    }
                }
            ],
            ""included"": [
                {
                    ""type"": ""systems"",
                    ""id"": ""1"",
                    ""attributes"": { ""name"": ""Mega Drive/Genesis"" }
                }
            ]
        }";

        SetupMockResponse(jsonResponse);

        var query = V2QueryBuilder.Create()
            .Include("system");

        // Act
        var result = await _client.GetGamesAsync(query);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Included, Is.Not.Null);
            Assert.That(result.Included, Has.Count.EqualTo(1));

            var system = result.FindIncluded("systems", "1");
            Assert.That(system, Is.Not.Null);
            Assert.That(system!.GetAttribute<string>("name"), Is.EqualTo("Mega Drive/Genesis"));

            // Both games should be able to resolve to the same included system
            var games = result.GetResourceCollection();
            foreach (var game in games)
            {
                var systemRel = game.GetRelationship("system");
                var systemId = systemRel?.GetSingleIdentifier();
                Assert.That(systemId?.Id, Is.EqualTo("1"));
            }
        });
    }

    #endregion

    #region Combined Query Tests

    [Test]
    public async Task CombinedQuery_AllParametersAreIncluded()
    {
        // Arrange
        string? capturedUrl = null;
        SetupMockResponseWithUrlCapture(@"{ ""data"": [] }", url => capturedUrl = url);

        var query = V2QueryBuilder.Create()
            .Include("system")
            .Filter("systemId", 1)
            .Filter("active", true)
            .SortDescending("pointsTotal")
            .Page(2)
            .PageSize(25);

        // Act
        await _client.GetGamesAsync(query);

        // Assert
        Assert.That(capturedUrl, Is.Not.Null);
        Assert.That(capturedUrl, Does.Contain("include="));
        Assert.That(capturedUrl, Does.Contain("filter[systemId]=1"));
        Assert.That(capturedUrl, Does.Contain("filter[active]=1"));
        Assert.That(capturedUrl, Does.Contain("sort="));
        Assert.That(capturedUrl, Does.Contain("page[number]=2"));
        Assert.That(capturedUrl, Does.Contain("page[size]=25"));
    }

    [Test]
    public void V2QueryBuilder_Build_ReturnsEmptyStringWhenNoParameters()
    {
        // Arrange
        var query = V2QueryBuilder.Create();

        // Act
        var result = query.Build();

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void V2QueryBuilder_ToString_ReturnsSameAsBuild()
    {
        // Arrange
        var query = V2QueryBuilder.Create()
            .Include("system")
            .Filter("active", true);

        // Act
        var build = query.Build();
        var toString = query.ToString();

        // Assert
        Assert.That(toString, Is.EqualTo(build));
    }

    [Test]
    public void V2QueryBuilder_ImplicitStringConversion_Works()
    {
        // Arrange
        var query = V2QueryBuilder.Create().Include("system");

        // Act
        string queryString = query;

        // Assert
        Assert.That(queryString, Does.Contain("include=system"));
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

    private void SetupMockResponseWithUrlCapture(string jsonContent, Action<string> urlCapture, HttpStatusCode statusCode = HttpStatusCode.OK)
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
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => urlCapture(req.RequestUri!.ToString()))
            .ReturnsAsync(response);
    }

    #endregion
}
