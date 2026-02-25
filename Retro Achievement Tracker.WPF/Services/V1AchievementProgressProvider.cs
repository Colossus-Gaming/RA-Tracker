using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using RATracker.Models;

namespace RATracker.WPF.Services;

/// <summary>
/// Implementation of IAchievementProgressProvider using the V1 API.
/// This provider wraps the existing V1 endpoints for user progress data.
/// </summary>
public class V1AchievementProgressProvider : IAchievementProgressProvider, IDisposable
{
    private const string BaseUrl = "https://retroachievements.org";
    
    // V1 API endpoints
    private const string UserSummaryEndpoint = "/API/API_GetUserSummary.php?z={0}&y={1}&u={2}&g=1&a=1";
    private const string GameInfoAndProgressEndpoint = "/API/API_GetGameInfoAndUserProgress.php?z={0}&y={1}&u={2}&g={3}";
    private const string RecentlyPlayedEndpoint = "/API/API_GetUserRecentlyPlayedGames.php?z={0}&y={1}&u={2}&c={3}";
    private const string RecentAchievementsEndpoint = "/API/API_GetUserRecentAchievements.php?z={0}&y={1}&u={2}&m={3}";
    private const string RankAndScoreEndpoint = "/API/API_GetUserRankAndScore.php?z={0}&y={1}&u={2}";

    private readonly HttpClient _httpClient;
    private readonly string _authUsername;
    private readonly string _apiKey;
    private readonly bool _ownsHttpClient;

    /// <summary>
    /// Creates a new V1AchievementProgressProvider.
    /// </summary>
    /// <param name="authUsername">The authenticated username for API calls.</param>
    /// <param name="apiKey">The RetroAchievements API key.</param>
    public V1AchievementProgressProvider(string authUsername, string apiKey)
        : this(authUsername, apiKey, new HttpClient())
    {
        _ownsHttpClient = true;
    }

    /// <summary>
    /// Creates a new V1AchievementProgressProvider with an existing HttpClient.
    /// </summary>
    /// <param name="authUsername">The authenticated username for API calls.</param>
    /// <param name="apiKey">The RetroAchievements API key.</param>
    /// <param name="httpClient">The HttpClient to use for requests.</param>
    public V1AchievementProgressProvider(string authUsername, string apiKey, HttpClient httpClient)
    {
        if (string.IsNullOrWhiteSpace(authUsername))
            throw new ArgumentException("Username is required", nameof(authUsername));
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key is required", nameof(apiKey));

        _authUsername = authUsername;
        _apiKey = apiKey;
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _ownsHttpClient = false;

        _httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
        {
            NoCache = true
        };
    }

    #region User Progress

    /// <inheritdoc />
    public async Task<UserSummary?> GetUserSummaryAsync(string username, bool includeRecentAchievements = true, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            return null;

        var url = string.Format(BaseUrl + UserSummaryEndpoint, _authUsername, _apiKey, username);
        
        var response = await _httpClient.GetAsync(url, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<UserSummary>(content);
    }

    /// <inheritdoc />
    public async Task<UserRankAndScore?> GetUserRankAndScoreAsync(string username, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            return null;

        var url = string.Format(BaseUrl + RankAndScoreEndpoint, _authUsername, _apiKey, username);
        
        var response = await _httpClient.GetAsync(url, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        
        // V1 returns a different structure, map to our model
        var v1Result = JsonConvert.DeserializeObject<V1RankAndScoreResponse>(content);
        if (v1Result == null)
            return null;

        return new UserRankAndScore
        {
            Rank = v1Result.Rank,
            TotalPoints = v1Result.Score,
            TotalTruePoints = v1Result.SoftcoreScore // V1 may not have true points in this endpoint
        };
    }

    #endregion

    #region Game Progress

    /// <inheritdoc />
    public async Task<GameInfo?> GetGameInfoAndProgressAsync(string username, long gameId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || gameId <= 0)
            return null;

        var url = string.Format(BaseUrl + GameInfoAndProgressEndpoint, _authUsername, _apiKey, username, gameId);
        
        var response = await _httpClient.GetAsync(url, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<GameInfo>(content);
    }

    /// <inheritdoc />
    public async Task<List<GameInfo>> GetRecentlyPlayedGamesAsync(string username, int count = 10, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            return new List<GameInfo>();

        var url = string.Format(BaseUrl + RecentlyPlayedEndpoint, _authUsername, _apiKey, username, count);
        
        var response = await _httpClient.GetAsync(url, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            return new List<GameInfo>();
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<List<GameInfo>>(content) ?? new List<GameInfo>();
    }

    #endregion

    #region Achievement Progress

    /// <inheritdoc />
    public async Task<List<Achievement>> GetRecentAchievementsAsync(string username, int minutes = 60, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            return new List<Achievement>();

        var url = string.Format(BaseUrl + RecentAchievementsEndpoint, _authUsername, _apiKey, username, minutes);
        
        var response = await _httpClient.GetAsync(url, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            return new List<Achievement>();
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<List<Achievement>>(content) ?? new List<Achievement>();
    }

    /// <inheritdoc />
    public async Task<List<Achievement>> GetGameAchievementsWithProgressAsync(string username, long gameId, CancellationToken cancellationToken = default)
    {
        // Use the game info and progress endpoint which includes achievements with unlock status
        var gameInfo = await GetGameInfoAndProgressAsync(username, gameId, cancellationToken);
        return gameInfo?.Achievements ?? new List<Achievement>();
    }

    #endregion

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    /// <summary>
    /// Internal class for deserializing V1 rank and score response.
    /// </summary>
    private class V1RankAndScoreResponse
    {
        [JsonProperty("Rank")]
        public int Rank { get; set; }

        [JsonProperty("Score")]
        public int Score { get; set; }

        [JsonProperty("SoftcoreScore")]
        public int SoftcoreScore { get; set; }
    }
}
