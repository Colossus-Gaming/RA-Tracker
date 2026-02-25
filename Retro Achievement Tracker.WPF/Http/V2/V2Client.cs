using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using RATracker.WPF.Http.V2.JsonApi;

namespace RATracker.WPF.Http.V2;

/// <summary>
/// HTTP client for the RetroAchievements V2 API (JSON:API).
/// </summary>
public class V2Client : IDisposable
{
    private const string DefaultBaseUrl = "https://retroachievements.org";
    private const string ApiPath = "/api/v2";
    private const string JsonApiMediaType = "application/vnd.api+json";

    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly bool _ownsHttpClient;
    private readonly IV2ApiLogger _logger;

    /// <summary>
    /// Creates a new V2Client with the specified API key.
    /// </summary>
    /// <param name="apiKey">The RetroAchievements API key.</param>
    /// <param name="baseUrl">The base URL for the API (optional, defaults to production).</param>
    /// <param name="logger">Optional logger for observability.</param>
    public V2Client(string apiKey, string? baseUrl = null, IV2ApiLogger? logger = null)
        : this(apiKey, new HttpClient(), baseUrl, logger)
    {
        _ownsHttpClient = true;
    }

    /// <summary>
    /// Creates a new V2Client with the specified API key and HttpClient.
    /// </summary>
    /// <param name="apiKey">The RetroAchievements API key.</param>
    /// <param name="httpClient">The HttpClient to use for requests.</param>
    /// <param name="baseUrl">The base URL for the API (optional, defaults to production).</param>
    /// <param name="logger">Optional logger for observability.</param>
    public V2Client(string apiKey, HttpClient httpClient, string? baseUrl = null, IV2ApiLogger? logger = null)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key is required", nameof(apiKey));

        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _baseUrl = (baseUrl ?? DefaultBaseUrl).TrimEnd('/');
        _ownsHttpClient = false;
        _logger = logger ?? NullApiLogger.Instance;

        // Set default headers
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonApiMediaType));
        _httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
    }

    /// <summary>
    /// Sends a GET request to the specified endpoint and returns the JSON:API document.
    /// </summary>
    /// <param name="endpoint">The API endpoint (e.g., "/systems", "/games/1234").</param>
    /// <param name="queryBuilder">Optional query builder for includes, filters, sorting, and pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The parsed JSON:API document.</returns>
    public async Task<JsonApiDocument> GetAsync(string endpoint, V2QueryBuilder? queryBuilder = null, CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(endpoint, queryBuilder);
        return await SendRequestAsync(HttpMethod.Get, url, cancellationToken);
    }

    /// <summary>
    /// Sends a GET request to fetch a collection of resources.
    /// </summary>
    /// <param name="resourceType">The resource type (e.g., "systems", "games", "users").</param>
    /// <param name="queryBuilder">Optional query builder for includes, filters, sorting, and pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The parsed JSON:API document.</returns>
    public Task<JsonApiDocument> GetCollectionAsync(string resourceType, V2QueryBuilder? queryBuilder = null, CancellationToken cancellationToken = default)
    {
        return GetAsync($"/{resourceType}", queryBuilder, cancellationToken);
    }

    /// <summary>
    /// Sends a GET request to fetch a single resource by ID.
    /// </summary>
    /// <param name="resourceType">The resource type (e.g., "systems", "games", "users").</param>
    /// <param name="id">The resource ID.</param>
    /// <param name="queryBuilder">Optional query builder for includes.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The parsed JSON:API document.</returns>
    public Task<JsonApiDocument> GetResourceAsync(string resourceType, string id, V2QueryBuilder? queryBuilder = null, CancellationToken cancellationToken = default)
    {
        return GetAsync($"/{resourceType}/{id}", queryBuilder, cancellationToken);
    }

    /// <summary>
    /// Sends a GET request to fetch a relationship endpoint.
    /// </summary>
    /// <param name="resourceType">The parent resource type.</param>
    /// <param name="id">The parent resource ID.</param>
    /// <param name="relationship">The relationship name (e.g., "games", "entries").</param>
    /// <param name="queryBuilder">Optional query builder for includes, filters, sorting, and pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The parsed JSON:API document.</returns>
    public Task<JsonApiDocument> GetRelationshipAsync(string resourceType, string id, string relationship, V2QueryBuilder? queryBuilder = null, CancellationToken cancellationToken = default)
    {
        return GetAsync($"/{resourceType}/{id}/{relationship}", queryBuilder, cancellationToken);
    }

    #region Convenience Methods for Common Resources

    /// <summary>
    /// Gets a list of systems.
    /// </summary>
    public Task<JsonApiDocument> GetSystemsAsync(V2QueryBuilder? queryBuilder = null, CancellationToken cancellationToken = default)
        => GetCollectionAsync("systems", queryBuilder, cancellationToken);

    /// <summary>
    /// Gets a single system by ID.
    /// </summary>
    public Task<JsonApiDocument> GetSystemAsync(string id, CancellationToken cancellationToken = default)
        => GetResourceAsync("systems", id, null, cancellationToken);

    /// <summary>
    /// Gets a list of games.
    /// </summary>
    public Task<JsonApiDocument> GetGamesAsync(V2QueryBuilder? queryBuilder = null, CancellationToken cancellationToken = default)
        => GetCollectionAsync("games", queryBuilder, cancellationToken);

    /// <summary>
    /// Gets a single game by ID with optional includes.
    /// </summary>
    public Task<JsonApiDocument> GetGameAsync(string id, V2QueryBuilder? queryBuilder = null, CancellationToken cancellationToken = default)
        => GetResourceAsync("games", id, queryBuilder, cancellationToken);

    /// <summary>
    /// Gets a list of users.
    /// </summary>
    public Task<JsonApiDocument> GetUsersAsync(V2QueryBuilder? queryBuilder = null, CancellationToken cancellationToken = default)
        => GetCollectionAsync("users", queryBuilder, cancellationToken);

    /// <summary>
    /// Gets a single user by identifier (ULID, username, or display name).
    /// </summary>
    public Task<JsonApiDocument> GetUserAsync(string identifier, CancellationToken cancellationToken = default)
        => GetResourceAsync("users", identifier, null, cancellationToken);

    /// <summary>
    /// Gets a list of achievement sets.
    /// </summary>
    public Task<JsonApiDocument> GetAchievementSetsAsync(V2QueryBuilder? queryBuilder = null, CancellationToken cancellationToken = default)
        => GetCollectionAsync("achievement-sets", queryBuilder, cancellationToken);

    /// <summary>
    /// Gets a single achievement set by ID.
    /// </summary>
    public Task<JsonApiDocument> GetAchievementSetAsync(string id, CancellationToken cancellationToken = default)
        => GetResourceAsync("achievement-sets", id, null, cancellationToken);

    /// <summary>
    /// Gets a list of hubs.
    /// </summary>
    public Task<JsonApiDocument> GetHubsAsync(V2QueryBuilder? queryBuilder = null, CancellationToken cancellationToken = default)
        => GetCollectionAsync("hubs", queryBuilder, cancellationToken);

    /// <summary>
    /// Gets a single hub by ID.
    /// </summary>
    public Task<JsonApiDocument> GetHubAsync(string id, CancellationToken cancellationToken = default)
        => GetResourceAsync("hubs", id, null, cancellationToken);

    /// <summary>
    /// Gets games for a hub (relationship endpoint).
    /// </summary>
    public Task<JsonApiDocument> GetHubGamesAsync(string hubId, V2QueryBuilder? queryBuilder = null, CancellationToken cancellationToken = default)
        => GetRelationshipAsync("hubs", hubId, "games", queryBuilder, cancellationToken);

    /// <summary>
    /// Gets links for a hub (relationship endpoint).
    /// </summary>
    public Task<JsonApiDocument> GetHubLinksAsync(string hubId, V2QueryBuilder? queryBuilder = null, CancellationToken cancellationToken = default)
        => GetRelationshipAsync("hubs", hubId, "links", queryBuilder, cancellationToken);

    /// <summary>
    /// Gets leaderboard entries (relationship endpoint).
    /// </summary>
    public Task<JsonApiDocument> GetLeaderboardEntriesAsync(string leaderboardId, V2QueryBuilder? queryBuilder = null, CancellationToken cancellationToken = default)
        => GetRelationshipAsync("leaderboards", leaderboardId, "entries", queryBuilder, cancellationToken);

    #endregion

    private string BuildUrl(string endpoint, V2QueryBuilder? queryBuilder)
    {
        var path = endpoint.StartsWith("/") ? endpoint : $"/{endpoint}";
        var queryString = queryBuilder?.Build() ?? string.Empty;
        return $"{_baseUrl}{ApiPath}{path}{queryString}";
    }

    private async Task<JsonApiDocument> SendRequestAsync(HttpMethod method, string url, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogRequest(method.Method, url);

        using var request = new HttpRequestMessage(method, url);
        HttpResponseMessage response;

        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(method.Method, url, ex.Message);
            throw new V2ApiException($"HTTP request failed: {ex.Message}", 0, ex);
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken != cancellationToken)
        {
            stopwatch.Stop();
            _logger.LogError(method.Method, url, "Request timed out", 408);
            throw new V2ApiException("Request timed out", 408, ex);
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        stopwatch.Stop();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogResponse(method.Method, url, (int)response.StatusCode, stopwatch.ElapsedMilliseconds, false);
            return await HandleErrorResponseAsync(response, content);
        }

        _logger.LogResponse(method.Method, url, (int)response.StatusCode, stopwatch.ElapsedMilliseconds, true);

        try
        {
            var document = JsonConvert.DeserializeObject<JsonApiDocument>(content);
            return document ?? new JsonApiDocument();
        }
        catch (JsonException ex)
        {
            _logger.LogError(method.Method, url, $"JSON parse error: {ex.Message}", (int)response.StatusCode);
            throw new V2ApiException($"Failed to parse JSON:API response: {ex.Message}", (int)response.StatusCode, ex);
        }
    }

    private async Task<JsonApiDocument> HandleErrorResponseAsync(HttpResponseMessage response, string content)
    {
        var statusCode = (int)response.StatusCode;
        List<JsonApiError>? errors = null;

        // Try to parse JSON:API error response
        if (!string.IsNullOrWhiteSpace(content))
        {
            try
            {
                var document = JsonConvert.DeserializeObject<JsonApiDocument>(content);
                errors = document?.Errors;
            }
            catch
            {
                // Ignore parsing errors for error responses
            }
        }

        // Handle rate limiting specially
        if (statusCode == 429)
        {
            int? retryAfter = null;
            if (response.Headers.TryGetValues("Retry-After", out var values))
            {
                if (int.TryParse(values.FirstOrDefault(), out var seconds))
                {
                    retryAfter = seconds;
                }
            }

            throw new V2RateLimitException("Rate limit exceeded", retryAfter);
        }

        var errorMessage = statusCode switch
        {
            400 => "Bad request",
            401 => "Authentication required",
            403 => "Access denied",
            404 => "Resource not found",
            409 => "Conflict",
            422 => "Validation failed",
            500 => "Internal server error",
            502 => "Bad gateway",
            503 => "Service unavailable",
            504 => "Gateway timeout",
            _ => $"HTTP {statusCode}"
        };

        throw new V2ApiException(errorMessage, statusCode, errors);
    }

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }
}
