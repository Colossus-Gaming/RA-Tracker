using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RATracker.WPF.Http.V2.JsonApi;

/// <summary>
/// Represents a JSON:API document response.
/// </summary>
public class JsonApiDocument
{
    /// <summary>
    /// The primary data of the response. Can be a single resource or a collection.
    /// </summary>
    [JsonProperty("data")]
    public JToken? Data { get; set; }

    /// <summary>
    /// An array of resource objects related to the primary data and/or each other.
    /// </summary>
    [JsonProperty("included")]
    public List<JsonApiResource>? Included { get; set; }

    /// <summary>
    /// An array of error objects.
    /// </summary>
    [JsonProperty("errors")]
    public List<JsonApiError>? Errors { get; set; }

    /// <summary>
    /// Non-standard meta information about the response.
    /// </summary>
    [JsonProperty("meta")]
    public JObject? Meta { get; set; }

    /// <summary>
    /// Links related to the primary data.
    /// </summary>
    [JsonProperty("links")]
    public JsonApiLinks? Links { get; set; }

    /// <summary>
    /// Returns true if the response contains errors.
    /// </summary>
    public bool HasErrors => Errors != null && Errors.Count > 0;

    /// <summary>
    /// Returns the primary data as a single resource.
    /// </summary>
    public JsonApiResource? GetSingleResource()
    {
        if (Data == null || Data.Type == JTokenType.Array)
            return null;

        return Data.ToObject<JsonApiResource>();
    }

    /// <summary>
    /// Returns the primary data as a collection of resources.
    /// </summary>
    public List<JsonApiResource> GetResourceCollection()
    {
        if (Data == null)
            return new List<JsonApiResource>();

        if (Data.Type == JTokenType.Array)
            return Data.ToObject<List<JsonApiResource>>() ?? new List<JsonApiResource>();

        // Single resource, wrap in list
        var resource = Data.ToObject<JsonApiResource>();
        return resource != null ? new List<JsonApiResource> { resource } : new List<JsonApiResource>();
    }

    /// <summary>
    /// Creates an index of included resources by (type, id) for efficient lookup.
    /// </summary>
    public Dictionary<(string Type, string Id), JsonApiResource> GetIncludedIndex()
    {
        var index = new Dictionary<(string Type, string Id), JsonApiResource>();

        if (Included == null)
            return index;

        foreach (var resource in Included)
        {
            if (!string.IsNullOrEmpty(resource.Type) && !string.IsNullOrEmpty(resource.Id))
            {
                index[(resource.Type, resource.Id)] = resource;
            }
        }

        return index;
    }

    /// <summary>
    /// Finds an included resource by type and id.
    /// </summary>
    public JsonApiResource? FindIncluded(string type, string id)
    {
        return Included?.FirstOrDefault(r => r.Type == type && r.Id == id);
    }
}

/// <summary>
/// Represents a JSON:API resource object.
/// </summary>
public class JsonApiResource
{
    /// <summary>
    /// The type of the resource.
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// The unique identifier of the resource.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The attributes of the resource.
    /// </summary>
    [JsonProperty("attributes")]
    public JObject? Attributes { get; set; }

    /// <summary>
    /// The relationships of the resource.
    /// </summary>
    [JsonProperty("relationships")]
    public Dictionary<string, JsonApiRelationship>? Relationships { get; set; }

    /// <summary>
    /// Links related to the resource.
    /// </summary>
    [JsonProperty("links")]
    public JsonApiLinks? Links { get; set; }

    /// <summary>
    /// Gets an attribute value by name.
    /// </summary>
    public T? GetAttribute<T>(string name)
    {
        if (Attributes == null)
            return default;

        var token = Attributes[name];
        if (token == null)
            return default;

        return token.ToObject<T>();
    }

    /// <summary>
    /// Gets a relationship by name.
    /// </summary>
    public JsonApiRelationship? GetRelationship(string name)
    {
        if (Relationships == null)
            return null;

        return Relationships.TryGetValue(name, out var relationship) ? relationship : null;
    }
}

/// <summary>
/// Represents a JSON:API relationship.
/// </summary>
public class JsonApiRelationship
{
    /// <summary>
    /// Links related to the relationship.
    /// </summary>
    [JsonProperty("links")]
    public JsonApiLinks? Links { get; set; }

    /// <summary>
    /// The relationship data. Can be a single resource identifier or a collection.
    /// </summary>
    [JsonProperty("data")]
    public JToken? Data { get; set; }

    /// <summary>
    /// Gets a single resource identifier from the relationship data.
    /// </summary>
    public JsonApiResourceIdentifier? GetSingleIdentifier()
    {
        if (Data == null || Data.Type == JTokenType.Array)
            return null;

        return Data.ToObject<JsonApiResourceIdentifier>();
    }

    /// <summary>
    /// Gets a collection of resource identifiers from the relationship data.
    /// </summary>
    public List<JsonApiResourceIdentifier> GetIdentifierCollection()
    {
        if (Data == null)
            return new List<JsonApiResourceIdentifier>();

        if (Data.Type == JTokenType.Array)
            return Data.ToObject<List<JsonApiResourceIdentifier>>() ?? new List<JsonApiResourceIdentifier>();

        // Single identifier, wrap in list
        var identifier = Data.ToObject<JsonApiResourceIdentifier>();
        return identifier != null ? new List<JsonApiResourceIdentifier> { identifier } : new List<JsonApiResourceIdentifier>();
    }
}

/// <summary>
/// Represents a JSON:API resource identifier (type + id only).
/// </summary>
public class JsonApiResourceIdentifier
{
    /// <summary>
    /// The type of the resource.
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// The unique identifier of the resource.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
}

/// <summary>
/// Represents JSON:API links.
/// </summary>
public class JsonApiLinks
{
    [JsonProperty("self")]
    public string? Self { get; set; }

    [JsonProperty("related")]
    public string? Related { get; set; }

    [JsonProperty("first")]
    public string? First { get; set; }

    [JsonProperty("last")]
    public string? Last { get; set; }

    [JsonProperty("prev")]
    public string? Prev { get; set; }

    [JsonProperty("next")]
    public string? Next { get; set; }
}

/// <summary>
/// Represents a JSON:API error object.
/// </summary>
public class JsonApiError
{
    /// <summary>
    /// A unique identifier for this particular occurrence of the problem.
    /// </summary>
    [JsonProperty("id")]
    public string? Id { get; set; }

    /// <summary>
    /// The HTTP status code applicable to this problem.
    /// </summary>
    [JsonProperty("status")]
    public string? Status { get; set; }

    /// <summary>
    /// An application-specific error code.
    /// </summary>
    [JsonProperty("code")]
    public string? Code { get; set; }

    /// <summary>
    /// A short, human-readable summary of the problem.
    /// </summary>
    [JsonProperty("title")]
    public string? Title { get; set; }

    /// <summary>
    /// A human-readable explanation specific to this occurrence of the problem.
    /// </summary>
    [JsonProperty("detail")]
    public string? Detail { get; set; }

    /// <summary>
    /// An object containing references to the source of the error.
    /// </summary>
    [JsonProperty("source")]
    public JsonApiErrorSource? Source { get; set; }
}

/// <summary>
/// Represents the source of a JSON:API error.
/// </summary>
public class JsonApiErrorSource
{
    /// <summary>
    /// A JSON Pointer to the value in the request document that caused the error.
    /// </summary>
    [JsonProperty("pointer")]
    public string? Pointer { get; set; }

    /// <summary>
    /// A string indicating which query parameter caused the error.
    /// </summary>
    [JsonProperty("parameter")]
    public string? Parameter { get; set; }
}
