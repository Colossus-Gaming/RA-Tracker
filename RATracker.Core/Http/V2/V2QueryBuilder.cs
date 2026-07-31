using System.Web;

namespace RATracker.WPF.Http.V2;

/// <summary>
/// Builder for constructing V2 API query parameters following JSON:API conventions.
/// </summary>
public class V2QueryBuilder
{
    private readonly List<string> _includes = new();
    private readonly Dictionary<string, string> _filters = new();
    private readonly List<string> _sorts = new();
    private int? _pageNumber;
    private int? _pageSize;

    /// <summary>
    /// Adds an include parameter for requesting related resources.
    /// </summary>
    /// <param name="relationship">The relationship name to include (e.g., "system", "achievementSets").</param>
    /// <returns>The builder instance for chaining.</returns>
    public V2QueryBuilder Include(string relationship)
    {
        if (!string.IsNullOrWhiteSpace(relationship) && !_includes.Contains(relationship))
        {
            _includes.Add(relationship);
        }
        return this;
    }

    /// <summary>
    /// Adds multiple include parameters.
    /// </summary>
    /// <param name="relationships">The relationship names to include.</param>
    /// <returns>The builder instance for chaining.</returns>
    public V2QueryBuilder Include(params string[] relationships)
    {
        foreach (var relationship in relationships)
        {
            Include(relationship);
        }
        return this;
    }

    /// <summary>
    /// Adds a filter parameter.
    /// </summary>
    /// <param name="field">The field to filter on (e.g., "systemId", "active").</param>
    /// <param name="value">The filter value.</param>
    /// <returns>The builder instance for chaining.</returns>
    public V2QueryBuilder Filter(string field, string value)
    {
        if (!string.IsNullOrWhiteSpace(field) && !string.IsNullOrWhiteSpace(value))
        {
            _filters[field] = value;
        }
        return this;
    }

    /// <summary>
    /// Adds a filter parameter with a numeric value.
    /// </summary>
    public V2QueryBuilder Filter(string field, long value) => Filter(field, value.ToString());

    /// <summary>
    /// Adds a filter parameter with a boolean value.
    /// </summary>
    public V2QueryBuilder Filter(string field, bool value) => Filter(field, value ? "1" : "0");

    /// <summary>
    /// Adds a sort parameter (ascending).
    /// </summary>
    /// <param name="field">The field to sort by.</param>
    /// <returns>The builder instance for chaining.</returns>
    public V2QueryBuilder SortAscending(string field)
    {
        if (!string.IsNullOrWhiteSpace(field))
        {
            _sorts.Add(field);
        }
        return this;
    }

    /// <summary>
    /// Adds a sort parameter (descending).
    /// </summary>
    /// <param name="field">The field to sort by.</param>
    /// <returns>The builder instance for chaining.</returns>
    public V2QueryBuilder SortDescending(string field)
    {
        if (!string.IsNullOrWhiteSpace(field))
        {
            _sorts.Add($"-{field}");
        }
        return this;
    }

    /// <summary>
    /// Sets the page number for pagination.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <returns>The builder instance for chaining.</returns>
    public V2QueryBuilder Page(int pageNumber)
    {
        if (pageNumber > 0)
        {
            _pageNumber = pageNumber;
        }
        return this;
    }

    /// <summary>
    /// Sets the page size for pagination.
    /// </summary>
    /// <param name="pageSize">The number of items per page (max 100).</param>
    /// <returns>The builder instance for chaining.</returns>
    public V2QueryBuilder PageSize(int pageSize)
    {
        if (pageSize > 0)
        {
            _pageSize = Math.Min(pageSize, 100); // Enforce max page size
        }
        return this;
    }

    /// <summary>
    /// Builds the query string for the request.
    /// </summary>
    /// <returns>The query string including the leading "?" if parameters exist, or empty string if none.</returns>
    public string Build()
    {
        var parameters = new List<string>();

        // Include parameters (comma-separated)
        if (_includes.Count > 0)
        {
            parameters.Add($"include={HttpUtility.UrlEncode(string.Join(",", _includes))}");
        }

        // Filter parameters
        foreach (var filter in _filters)
        {
            parameters.Add($"filter[{HttpUtility.UrlEncode(filter.Key)}]={HttpUtility.UrlEncode(filter.Value)}");
        }

        // Sort parameters (comma-separated)
        if (_sorts.Count > 0)
        {
            parameters.Add($"sort={HttpUtility.UrlEncode(string.Join(",", _sorts))}");
        }

        // Pagination parameters
        if (_pageNumber.HasValue)
        {
            parameters.Add($"page[number]={_pageNumber.Value}");
        }

        if (_pageSize.HasValue)
        {
            parameters.Add($"page[size]={_pageSize.Value}");
        }

        if (parameters.Count == 0)
        {
            return string.Empty;
        }

        return "?" + string.Join("&", parameters);
    }

    /// <summary>
    /// Creates a new V2QueryBuilder instance.
    /// </summary>
    public static V2QueryBuilder Create() => new();

    /// <summary>
    /// Implicit conversion to string for convenience.
    /// </summary>
    public static implicit operator string(V2QueryBuilder builder) => builder.Build();

    /// <summary>
    /// Returns the built query string.
    /// </summary>
    public override string ToString() => Build();
}

/// <summary>
/// Represents a page request for pagination.
/// </summary>
public class PageRequest
{
    /// <summary>
    /// The page number (1-based).
    /// </summary>
    public int Number { get; set; } = 1;

    /// <summary>
    /// The number of items per page.
    /// </summary>
    public int Size { get; set; } = 50;

    /// <summary>
    /// Creates a new PageRequest with default values.
    /// </summary>
    public static PageRequest Default => new();

    /// <summary>
    /// Creates a new PageRequest with the specified page number and size.
    /// </summary>
    public static PageRequest Create(int number = 1, int size = 50) => new() { Number = number, Size = size };
}
