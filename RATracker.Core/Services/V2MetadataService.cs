using RATracker.Models;
using RATracker.WPF.Http.V2;
using RATracker.WPF.Http.V2.JsonApi;
using RATracker.WPF.Http.V2.Mappers;

namespace RATracker.WPF.Services;

/// <summary>
/// Implementation of IMetadataService using the V2 API.
/// This service provides metadata-only operations (no user progress).
/// </summary>
public class V2MetadataService : IMetadataService, IDisposable
{
    private readonly V2Client _client;
    private readonly bool _ownsClient;

    /// <summary>
    /// Creates a new V2MetadataService with the specified API key.
    /// </summary>
    /// <param name="apiKey">The RetroAchievements API key.</param>
    /// <param name="logger">Optional logger for observability.</param>
    public V2MetadataService(string apiKey, IV2ApiLogger? logger = null)
    {
        _client = new V2Client(apiKey, logger: logger);
        _ownsClient = true;
    }

    /// <summary>
    /// Creates a new V2MetadataService with an existing V2Client.
    /// </summary>
    /// <param name="client">The V2Client to use.</param>
    public V2MetadataService(V2Client client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _ownsClient = false;
    }

    #region Systems

    /// <inheritdoc />
    public async Task<List<SystemInfo>> GetSystemsAsync(CancellationToken cancellationToken = default)
    {
        var query = V2QueryBuilder.Create()
            .Filter(V2Constants.Filters.Active, true)
            .SortAscending(V2Constants.SortFields.Name);

        var document = await _client.GetSystemsAsync(query, cancellationToken);

        if (document.HasErrors)
        {
            throw new V2ApiException("Failed to get systems", 0, document.Errors);
        }

        return document.GetResourceCollection()
            .Where(r => r.Type == V2Constants.ResourceTypes.Systems)
            .Select(MapToSystemInfo)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<SystemInfo?> GetSystemAsync(long systemId, CancellationToken cancellationToken = default)
    {
        try
        {
            var document = await _client.GetSystemAsync(systemId.ToString(), cancellationToken);

            if (document.HasErrors)
            {
                return null;
            }

            var resource = document.GetSingleResource();
            return resource != null ? MapToSystemInfo(resource) : null;
        }
        catch (V2ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    #endregion

    #region Games

    /// <inheritdoc />
    public async Task<List<GameInfo>> GetGamesForSystemAsync(long systemId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var query = V2QueryBuilder.Create()
            .Filter(V2Constants.Filters.SystemId, systemId)
            .Include(V2Constants.Relationships.System)
            .SortAscending(V2Constants.SortFields.Title)
            .Page(page)
            .PageSize(pageSize);

        var document = await _client.GetGamesAsync(query, cancellationToken);

        if (document.HasErrors)
        {
            throw new V2ApiException("Failed to get games", 0, document.Errors);
        }

        return V2ResourceMapper.MapToGameInfoList(document);
    }

    /// <inheritdoc />
    public async Task<GameInfo?> GetGameAsync(long gameId, bool includeAchievements = false, bool includeAchievementSets = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryBuilder = V2QueryBuilder.Create()
                .Include(V2Constants.Relationships.System);

            if (includeAchievements || includeAchievementSets)
            {
                queryBuilder.Include(V2Constants.Relationships.AchievementSets);
            }

            // If requesting achievement sets with achievements, we need to include the achievements relationship
            // This would typically be: include=achievementSets,achievementSets.achievements
            if (includeAchievementSets)
            {
                queryBuilder.Include($"{V2Constants.Relationships.AchievementSets}.achievements");
            }

            var document = await _client.GetGameAsync(gameId.ToString(), queryBuilder, cancellationToken);

            if (document.HasErrors)
            {
                return null;
            }

            var resource = document.GetSingleResource();
            if (resource == null)
            {
                return null;
            }

            var includedIndex = document.GetIncludedIndex();
            var gameInfo = V2ResourceMapper.MapToGameInfo(resource, includedIndex);

            // Extract achievement sets from included resources
            if (includeAchievementSets)
            {
                gameInfo.AchievementSets = V2ResourceMapper.ExtractAchievementSetsFromIncluded(resource, includedIndex);
                
                // Set the game ID and title on each achievement within the sets
                foreach (var set in gameInfo.AchievementSets)
                {
                    set.GameId = gameInfo.Id;
                    foreach (var achievement in set.Achievements)
                    {
                        achievement.GameId = (int)gameInfo.Id;
                        achievement.GameTitle = gameInfo.Title;
                    }
                }
            }
            // If just achievements requested (legacy behavior), extract from first/core set
            else if (includeAchievements)
            {
                var achievementSetRelationship = resource.GetRelationship(V2Constants.Relationships.AchievementSets);
                if (achievementSetRelationship != null)
                {
                    var achievementSetIds = achievementSetRelationship.GetIdentifierCollection();
                    // Note: Achievements would need to be fetched from the achievement sets
                    // For now, this is a placeholder - full achievement fetching would require additional calls
                    // or the achievements to be included in the response
                }
            }

            return gameInfo;
        }
        catch (V2ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<List<GameInfo>> SearchGamesAsync(string searchTerm, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new List<GameInfo>();
        }

        var query = V2QueryBuilder.Create()
            .Filter(V2Constants.Filters.Title, searchTerm)
            .Include(V2Constants.Relationships.System)
            .SortAscending(V2Constants.SortFields.Title)
            .Page(page)
            .PageSize(pageSize);

        var document = await _client.GetGamesAsync(query, cancellationToken);

        if (document.HasErrors)
        {
            throw new V2ApiException("Failed to search games", 0, document.Errors);
        }

        return V2ResourceMapper.MapToGameInfoList(document);
    }

    #endregion

    #region Users

    /// <inheritdoc />
    public async Task<UserSummary?> GetUserAsync(string identifier, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return null;
        }

        try
        {
            var document = await _client.GetUserAsync(identifier, cancellationToken);

            if (document.HasErrors)
            {
                return null;
            }

            var resource = document.GetSingleResource();
            return resource != null ? V2ResourceMapper.MapToUserSummary(resource) : null;
        }
        catch (V2ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<List<UserSummary>> SearchUsersAsync(string searchTerm, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new List<UserSummary>();
        }

        var query = V2QueryBuilder.Create()
            .Filter(V2Constants.Filters.User, searchTerm)
            .Page(page)
            .PageSize(pageSize);

        var document = await _client.GetUsersAsync(query, cancellationToken);

        if (document.HasErrors)
        {
            throw new V2ApiException("Failed to search users", 0, document.Errors);
        }

        return V2ResourceMapper.MapToUserSummaryList(document);
    }

    #endregion

    #region Hubs

    /// <inheritdoc />
    public async Task<List<HubInfo>> GetHubsAsync(int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var query = V2QueryBuilder.Create()
            .SortAscending(V2Constants.SortFields.Name)
            .Page(page)
            .PageSize(pageSize);

        var document = await _client.GetHubsAsync(query, cancellationToken);

        if (document.HasErrors)
        {
            throw new V2ApiException("Failed to get hubs", 0, document.Errors);
        }

        return V2ResourceMapper.MapToHubInfoList(document);
    }

    /// <inheritdoc />
    public async Task<HubInfo?> GetHubAsync(long hubId, CancellationToken cancellationToken = default)
    {
        try
        {
            var document = await _client.GetHubAsync(hubId.ToString(), cancellationToken);

            if (document.HasErrors)
            {
                return null;
            }

            var resource = document.GetSingleResource();
            if (resource == null)
            {
                return null;
            }

            var includedIndex = document.GetIncludedIndex();
            return V2ResourceMapper.MapToHubInfo(resource, includedIndex);
        }
        catch (V2ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<List<GameInfo>> GetHubGamesAsync(long hubId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var query = V2QueryBuilder.Create()
            .Include(V2Constants.Relationships.System)
            .SortAscending(V2Constants.SortFields.Title)
            .Page(page)
            .PageSize(pageSize);

        var document = await _client.GetHubGamesAsync(hubId.ToString(), query, cancellationToken);

        if (document.HasErrors)
        {
            throw new V2ApiException("Failed to get hub games", 0, document.Errors);
        }

        return V2ResourceMapper.MapToGameInfoList(document);
    }

    #endregion

    #region Private Helpers

    private static SystemInfo MapToSystemInfo(JsonApiResource resource)
    {
        return new SystemInfo
        {
            Id = long.TryParse(resource.Id, out var id) ? id : 0,
            Name = resource.GetAttribute<string>("name") ?? string.Empty,
            ShortName = resource.GetAttribute<string>("nameFull") ?? resource.GetAttribute<string>("nameShort") ?? string.Empty,
            IconUrl = resource.GetAttribute<string>("iconUrl") ?? string.Empty,
            IsActive = resource.GetAttribute<bool?>("active") ?? true,
            GameCount = resource.GetAttribute<int?>("gamesPublished") ?? 0,
            AchievementCount = resource.GetAttribute<int?>("achievementsPublished") ?? 0
        };
    }

    #endregion

    public void Dispose()
    {
        if (_ownsClient)
        {
            _client.Dispose();
        }
    }
}
