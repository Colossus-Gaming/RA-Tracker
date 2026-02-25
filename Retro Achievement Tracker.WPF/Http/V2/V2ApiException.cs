using RATracker.WPF.Http.V2.JsonApi;

namespace RATracker.WPF.Http.V2;

/// <summary>
/// Exception thrown when a V2 API request fails.
/// </summary>
public class V2ApiException : Exception
{
    /// <summary>
    /// The HTTP status code of the response.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// The JSON:API errors from the response, if available.
    /// </summary>
    public List<JsonApiError>? Errors { get; }

    /// <summary>
    /// Indicates whether this is a transient error that may be retried.
    /// </summary>
    public bool IsTransient => StatusCode >= 500 || StatusCode == 429;

    /// <summary>
    /// Indicates whether this is an authentication error.
    /// </summary>
    public bool IsAuthenticationError => StatusCode == 401;

    /// <summary>
    /// Indicates whether this is an authorization error.
    /// </summary>
    public bool IsAuthorizationError => StatusCode == 403;

    /// <summary>
    /// Indicates whether this is a not found error.
    /// </summary>
    public bool IsNotFound => StatusCode == 404;

    /// <summary>
    /// Indicates whether this is a rate limiting error.
    /// </summary>
    public bool IsRateLimited => StatusCode == 429;

    /// <summary>
    /// Indicates whether this is a validation error.
    /// </summary>
    public bool IsValidationError => StatusCode == 400 || StatusCode == 422;

    public V2ApiException(string message, int statusCode, List<JsonApiError>? errors = null)
        : base(message)
    {
        StatusCode = statusCode;
        Errors = errors;
    }

    public V2ApiException(string message, int statusCode, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Gets a formatted error message from the JSON:API errors.
    /// </summary>
    public string GetFormattedErrors()
    {
        if (Errors == null || Errors.Count == 0)
            return Message;

        var errorMessages = Errors
            .Select(e => !string.IsNullOrEmpty(e.Detail) ? e.Detail : e.Title ?? "Unknown error")
            .Where(m => !string.IsNullOrEmpty(m));

        return string.Join("; ", errorMessages);
    }
}

/// <summary>
/// Exception thrown when a rate limit is exceeded.
/// </summary>
public class V2RateLimitException : V2ApiException
{
    /// <summary>
    /// The number of seconds to wait before retrying, if provided.
    /// </summary>
    public int? RetryAfterSeconds { get; }

    public V2RateLimitException(string message, int? retryAfterSeconds = null)
        : base(message, 429)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }
}
