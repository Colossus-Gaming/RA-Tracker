using System.Net;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace RATracker.WPF.Views;

/// <summary>
/// Login window that uses WebView2 to log in to RetroAchievements behind the scenes.
/// Starts minimized and auto-submits credentials. Only becomes visible if
/// auto-login fails (e.g., Cloudflare challenge, CAPTCHA, bad credentials).
/// Extracts session cookies after successful login for V2 API access.
/// </summary>
public partial class LoginWindow : Window
{
    private readonly string? _username;
    private readonly string? _password;
    private bool _loginDetected;
    private string? _userAgent;
    private bool _autoSubmitted;
    private int _loginPageLoadCount;
    private System.Windows.Threading.DispatcherTimer? _timeoutTimer;

    public bool LoginSucceeded { get; private set; }
    public CookieContainer? ResultCookieContainer { get; private set; }
    public DateTime? SessionExpiresAt { get; private set; }
    public string? CapturedUserAgent { get; private set; }
    public string? ExtractedApiKey { get; private set; }

    public LoginWindow(string? username = null, string? password = null)
    {
        InitializeComponent();
        _username = username;
        _password = password;
        Loaded += Window_Loaded;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            Log("Initializing WebView2...");
            StatusText.Text = "Logging in...";
            await LoginWebView.EnsureCoreWebView2Async();

            // Capture User-Agent (critical for cf_clearance cookie matching)
            var uaResult = await LoginWebView.CoreWebView2.ExecuteScriptAsync("navigator.userAgent");
            _userAgent = uaResult?.Trim('"');
            CapturedUserAgent = _userAgent;
            Log($"Captured User-Agent: {_userAgent?[..Math.Min(50, _userAgent?.Length ?? 0)]}...");

            LoginWebView.CoreWebView2.NavigationStarting += OnNavigationStarting;
            LoginWebView.CoreWebView2.NavigationCompleted += OnNavigationCompleted;

            // Start a timeout: if login doesn't complete in 15 seconds, show the window
            _timeoutTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(15)
            };
            _timeoutTimer.Tick += (_, _) =>
            {
                _timeoutTimer.Stop();
                if (!_loginDetected)
                {
                    Log("Auto-login timeout — showing window for manual login");
                    ShowWindowForManualLogin();
                }
            };
            _timeoutTimer.Start();

            StatusText.Text = "Loading login page...";
            LoginWebView.Source = new Uri("https://retroachievements.org/login");
        }
        catch (Exception ex)
        {
            Log($"WebView2 init error: {ex}");
            StatusText.Text = $"Error: {ex.Message}";
            ShowWindowForManualLogin();
        }
    }

    private void ShowWindowForManualLogin()
    {
        Log("Showing login window for manual interaction");
        WindowState = WindowState.Normal;
        ShowInTaskbar = true;
        if (Owner != null)
        {
            Left = Owner.Left + (Owner.Width - Width) / 2;
            Top = Owner.Top + (Owner.Height - Height) / 2;
        }
        Activate();
    }

    private void OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        var path = new Uri(e.Uri).AbsolutePath;
        StatusDetail.Text = path;
        Log($"Navigating to: {path}");
    }

    private async void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (_loginDetected) return;

        var url = LoginWebView.Source?.ToString() ?? "";
        Log($"Navigation completed: {url} (success={e.IsSuccess})");

        // If we're on the login page, auto-fill and auto-submit
        if (url.Contains("/login"))
        {
            _loginPageLoadCount++;
            StatusText.Text = "Login page ready";

            if (!_autoSubmitted && !string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password))
            {
                StatusDetail.Text = "Auto-submitting credentials...";
                await TryAutoFillAndSubmit();
            }
            else if (_autoSubmitted)
            {
                // We already tried auto-submit but ended up back on /login — credentials were wrong
                Log("Auto-submit failed (returned to login page) — showing window");
                StatusDetail.Text = "Login failed — please try manually";
                ShowWindowForManualLogin();
            }
            else
            {
                StatusDetail.Text = "Enter your credentials";
                ShowWindowForManualLogin();
            }
            return;
        }

        // If we navigated away from login, check for session cookie
        if (url.Contains("retroachievements.org") && !url.Contains("/login"))
        {
            StatusText.Text = "Checking session...";
            await CheckForSuccessfulLogin();
        }
    }

    private async Task TryAutoFillAndSubmit()
    {
        try
        {
            Log("Auto-filling credentials...");
            var safeUser = _username!.Replace("\\", "\\\\").Replace("'", "\\'");
            var safePass = _password!.Replace("\\", "\\\\").Replace("'", "\\'");

            // Fill credentials and submit the form
            var js = $@"
                (function() {{
                    var u = document.querySelector('input[name=""user""]') || document.querySelector('input[name=""username""]') || document.querySelector('input[name=""email""]');
                    var p = document.querySelector('input[type=""password""]');
                    if (u && p) {{
                        u.value = '{safeUser}';
                        u.dispatchEvent(new Event('input', {{bubbles:true}}));
                        p.value = '{safePass}';
                        p.dispatchEvent(new Event('input', {{bubbles:true}}));

                        // Try to find and click the submit button
                        var btn = document.querySelector('button[type=""submit""]') || document.querySelector('input[type=""submit""]');
                        if (btn) {{
                            btn.click();
                            return 'submitted';
                        }}

                        // Fallback: submit the form directly
                        var form = u.closest('form');
                        if (form) {{
                            form.submit();
                            return 'form-submitted';
                        }}
                        return 'no-submit-button';
                    }}
                    return 'fields-not-found';
                }})()";

            var result = await LoginWebView.CoreWebView2.ExecuteScriptAsync(js);
            _autoSubmitted = true;
            Log($"Auto-fill+submit result: {result}");

            if (result.Contains("not-found"))
            {
                Log("Could not find login form fields — showing window");
                ShowWindowForManualLogin();
            }
        }
        catch (Exception ex)
        {
            Log($"Auto-fill error: {ex.Message}");
            ShowWindowForManualLogin();
        }
    }

    private async Task CheckForSuccessfulLogin()
    {
        try
        {
            var cookies = await LoginWebView.CoreWebView2.CookieManager
                .GetCookiesAsync("https://retroachievements.org");

            CoreWebView2Cookie? sessionCookie = null;
            foreach (var c in cookies)
            {
                if (c.Name.Contains("session"))
                {
                    sessionCookie = c;
                    break;
                }
            }

            if (sessionCookie != null)
            {
                _loginDetected = true;
                _timeoutTimer?.Stop();
                Log("Session cookie found — login successful!");
                StatusText.Text = "Login successful!";
                StatusDetail.Text = "Extracting session...";

                ExtractCookies(cookies);

                // Try to extract API key from settings page
                await TryExtractApiKey();

                LoginSucceeded = true;
                DialogResult = true;
                Close();
            }
            else
            {
                Log("No session cookie found yet");
                StatusText.Text = "Navigating...";
                StatusDetail.Text = "Waiting for session cookie";
            }
        }
        catch (Exception ex)
        {
            Log($"Cookie check error: {ex.Message}");
        }
    }

    private void ExtractCookies(System.Collections.Generic.List<CoreWebView2Cookie> wv2Cookies)
    {
        ResultCookieContainer = new CookieContainer();

        foreach (var wv2Cookie in wv2Cookies)
        {
            try
            {
                var domain = wv2Cookie.Domain.StartsWith(".")
                    ? wv2Cookie.Domain
                    : "." + wv2Cookie.Domain;

                var cookie = new Cookie(wv2Cookie.Name, wv2Cookie.Value, wv2Cookie.Path, domain)
                {
                    HttpOnly = wv2Cookie.IsHttpOnly,
                    Secure = wv2Cookie.IsSecure
                };

                if (wv2Cookie.Expires > DateTime.MinValue)
                {
                    cookie.Expires = wv2Cookie.Expires;
                }

                ResultCookieContainer.Add(cookie);

                // Track session expiry
                if (wv2Cookie.Name.Contains("session") && wv2Cookie.Expires > DateTime.MinValue)
                {
                    SessionExpiresAt = wv2Cookie.Expires;
                }
            }
            catch (Exception ex)
            {
                Log($"Cookie extraction error for {wv2Cookie.Name}: {ex.Message}");
            }
        }

        Log($"Extracted {ResultCookieContainer.Count} cookies. Session expires: {SessionExpiresAt?.ToString("g") ?? "unknown"}");
    }

    private async Task TryExtractApiKey()
    {
        try
        {
            Log("Navigating to settings page to extract API key...");
            LoginWebView.Source = new Uri("https://retroachievements.org/settings");

            // Wait for navigation
            var tcs = new TaskCompletionSource<bool>();
            void handler(object? s, CoreWebView2NavigationCompletedEventArgs args)
            {
                LoginWebView.CoreWebView2.NavigationCompleted -= handler;
                tcs.TrySetResult(args.IsSuccess);
            }
            LoginWebView.CoreWebView2.NavigationCompleted += handler;

            var navigated = await Task.WhenAny(tcs.Task, Task.Delay(8000));
            if (navigated != tcs.Task || !tcs.Task.Result)
            {
                Log("Settings page navigation failed or timed out");
                return;
            }

            // Small delay for React/Inertia to hydrate
            await Task.Delay(1500);

            // RA uses Inertia.js (Laravel + React). The full API key is in the page props JSON
            // embedded in the <div id="app" data-page="..."> element.
            var result = await LoginWebView.CoreWebView2.ExecuteScriptAsync(@"
                (function() {
                    // Method 1: Extract from Inertia page props (most reliable)
                    var appEl = document.getElementById('app');
                    if (appEl) {
                        var pageData = appEl.getAttribute('data-page');
                        if (pageData) {
                            try {
                                var parsed = JSON.parse(pageData);
                                var key = parsed?.props?.userSettings?.apiKey;
                                if (key && key.length > 10) return key;
                            } catch(e) {}
                        }
                    }

                    // Method 2: Try __page global (Inertia stores page object)
                    if (window.__page && window.__page.props) {
                        var key2 = window.__page.props.userSettings?.apiKey;
                        if (key2 && key2.length > 10) return key2;
                    }

                    // Method 3: Scan for Inertia app instance
                    try {
                        var inertiaEl = document.querySelector('[data-page]');
                        if (inertiaEl) {
                            var pd = JSON.parse(inertiaEl.getAttribute('data-page'));
                            var key3 = pd?.props?.userSettings?.apiKey;
                            if (key3 && key3.length > 10) return key3;
                        }
                    } catch(e) {}

                    return '';
                })()
            ");

            var key = result?.Trim('"');
            if (!string.IsNullOrEmpty(key) && key.Length > 10)
            {
                ExtractedApiKey = key;
                Log($"Successfully extracted API key ({key.Length} chars): {key[..6]}...");
            }
            else
            {
                Log($"Could not extract API key from settings page (result: {result})");
            }
        }
        catch (Exception ex)
        {
            Log($"API key extraction error: {ex.Message}");
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        _timeoutTimer?.Stop();
        DialogResult = false;
        Close();
    }

    private static void Log(string message)
    {
        System.Diagnostics.Debug.WriteLine($"[LoginWindow] {message}");
    }
}
