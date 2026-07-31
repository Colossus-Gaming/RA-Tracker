using System.Net;

namespace RATracker.WPF.Services;

/// <summary>
/// Manages browser session state for V2 API authentication.
/// Holds cookies extracted from WebView2 login that bypass Cloudflare.
/// </summary>
public class SessionService
{
    private static readonly Lazy<SessionService> _instance = new(() => new SessionService());
    private readonly object _lock = new();

    private CookieContainer? _cookieContainer;
    private string? _userAgent;
    private DateTime? _expiresAt;

    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static SessionService Instance => _instance.Value;

    private SessionService() { }

    /// <summary>
    /// Fires when a session is successfully established.
    /// </summary>
    public event EventHandler? SessionEstablished;

    /// <summary>
    /// Fires when the session expires or is cleared.
    /// </summary>
    public event EventHandler? SessionExpired;

    /// <summary>
    /// Whether we have valid session cookies.
    /// </summary>
    public bool IsAuthenticated
    {
        get
        {
            lock (_lock)
            {
                return _cookieContainer != null
                    && (_expiresAt == null || _expiresAt > DateTime.UtcNow);
            }
        }
    }

    /// <summary>
    /// The cookie container with session cookies from WebView2.
    /// </summary>
    public CookieContainer? CookieContainer
    {
        get { lock (_lock) return _cookieContainer; }
    }

    /// <summary>
    /// The User-Agent string from WebView2. Must match for cf_clearance cookie.
    /// </summary>
    public string? UserAgent
    {
        get { lock (_lock) return _userAgent; }
    }

    /// <summary>
    /// When the session expires.
    /// </summary>
    public DateTime? ExpiresAt
    {
        get { lock (_lock) return _expiresAt; }
    }

    /// <summary>
    /// Human-readable session status.
    /// </summary>
    public string StatusText
    {
        get
        {
            lock (_lock)
            {
                if (_cookieContainer == null)
                    return "Not connected";

                if (_expiresAt != null && _expiresAt <= DateTime.UtcNow)
                    return "Session expired";

                if (_expiresAt != null)
                {
                    var remaining = _expiresAt.Value - DateTime.UtcNow;
                    if (remaining.TotalDays >= 1)
                        return $"Session active ({(int)remaining.TotalDays}d remaining)";
                    if (remaining.TotalHours >= 1)
                        return $"Session active ({(int)remaining.TotalHours}h remaining)";
                    return $"Session active ({(int)remaining.TotalMinutes}m remaining)";
                }

                return "Session active";
            }
        }
    }

    /// <summary>
    /// Stores session cookies extracted from WebView2 after login.
    /// </summary>
    /// <param name="cookies">The cookie container populated from WebView2.</param>
    /// <param name="userAgent">The WebView2 User-Agent string (required for cf_clearance).</param>
    /// <param name="expiresAt">When the session expires.</param>
    public void SetSession(CookieContainer cookies, string userAgent, DateTime? expiresAt = null)
    {
        lock (_lock)
        {
            _cookieContainer = cookies ?? throw new ArgumentNullException(nameof(cookies));
            _userAgent = userAgent ?? throw new ArgumentNullException(nameof(userAgent));
            _expiresAt = expiresAt;
        }

        System.Diagnostics.Debug.WriteLine(
            $"[Session] Session established. Expires: {expiresAt?.ToString("g") ?? "unknown"}");

        SessionEstablished?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Clears the current session.
    /// </summary>
    public void ClearSession()
    {
        bool wasAuthenticated;
        lock (_lock)
        {
            wasAuthenticated = _cookieContainer != null;
            _cookieContainer = null;
            _userAgent = null;
            _expiresAt = null;
        }

        if (wasAuthenticated)
        {
            System.Diagnostics.Debug.WriteLine("[Session] Session cleared.");
            SessionExpired?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Marks the session as expired (e.g., after a 401/403 from the API).
    /// </summary>
    public void MarkExpired()
    {
        lock (_lock)
        {
            _expiresAt = DateTime.UtcNow;
        }

        System.Diagnostics.Debug.WriteLine("[Session] Session marked as expired.");
        SessionExpired?.Invoke(this, EventArgs.Empty);
    }
}
