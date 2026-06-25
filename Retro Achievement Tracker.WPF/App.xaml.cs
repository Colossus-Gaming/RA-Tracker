using System.Diagnostics;
using System.IO;
using System.Windows;
using RATracker.WPF.Http.V2;
using RATracker.WPF.Services;

namespace RATracker.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private TextWriterTraceListener? _fileListener;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // --log-to-file <path>: redirect all Debug/Trace output to a file (for integration tests)
        var args = e.Args;
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--log-to-file")
            {
                var logPath = args[i + 1];
                var dir = Path.GetDirectoryName(logPath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                // Open with FileShare.Read so integration tests can read the log while the app writes
                var logStream = new FileStream(logPath, FileMode.Create, FileAccess.Write, FileShare.Read);
                _fileListener = new TextWriterTraceListener(logStream) { TraceOutputOptions = TraceOptions.Timestamp };
                // Debug.WriteLine routes through Trace.Listeners in .NET 8
                Trace.Listeners.Add(_fileListener);
                Trace.AutoFlush = true;
                Debug.WriteLine($"[App] File logging enabled: {logPath}");
                break;
            }
        }

        // Global crash logging: capture unhandled exceptions from the UI dispatcher, background
        // threads, and faulted tasks so they land in the log instead of silently killing the app.
        DispatcherUnhandledException += (_, args) =>
        {
            LogExceptionChain("DispatcherUnhandledException", args.Exception);
            args.Handled = true; // keep the app alive instead of crashing
        };
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            LogExceptionChain("AppDomain.UnhandledException", args.ExceptionObject as Exception);
        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            LogExceptionChain("UnobservedTaskException", args.Exception);
            args.SetObserved();
        };

        // --probe-v2 "<path1>;<path2>": GET raw v2 endpoints with the saved API key and log the bodies,
        // then exit. Diagnostic tool for discovering live JSON:API response shapes (no login needed).
        var probeIdx = Array.IndexOf(args, "--probe-v2");
        if (probeIdx >= 0 && probeIdx < args.Length - 1)
        {
            RunV2Probe(args[probeIdx + 1]);
            Shutdown();
            return;
        }

        // --probe-game "<id1>;<id2>;...": runs the full Hybrid progress flow for each game id and
        // prints a structured summary (sets, types, achievement counts, unlocks). Used to validate
        // multiset wiring against real games without playing them.
        var gameIdx = Array.IndexOf(args, "--probe-game");
        if (gameIdx >= 0 && gameIdx < args.Length - 1)
        {
            RunGameProbe(args[gameIdx + 1]);
            Shutdown();
            return;
        }

        try
        {
            // DEBUG: pin the tracked game to Final Fantasy VIII (11270) for subset testing.
            // Remove this line to restore normal "currently playing" tracking.
            AchievementTrackingService.DebugForceGameId = 11270;

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            // Log the full exception chain
            var current = ex;
            int depth = 0;
            while (current != null)
            {
                Debug.WriteLine($"Exception depth {depth}: {current.GetType().Name}");
                Debug.WriteLine($"Message: {current.Message}");
                Debug.WriteLine($"Stack: {current.StackTrace}");
                Debug.WriteLine("---");
                current = current.InnerException;
                depth++;
            }
            throw;
        }
    }

    private static void LogExceptionChain(string source, Exception? ex)
    {
        Debug.WriteLine($"[CRASH] {source}: {ex?.GetType().FullName}: {ex?.Message}");
        var current = ex;
        int depth = 0;
        while (current != null)
        {
            Debug.WriteLine($"[CRASH]   depth {depth} {current.GetType().FullName}: {current.Message}");
            Debug.WriteLine($"[CRASH]   stack: {current.StackTrace}");
            current = current.InnerException;
            depth++;
        }
    }

    /// <summary>
    /// Diagnostic: issues raw GETs against the v2 API (semicolon-separated relative paths) using the
    /// saved/env API key and logs the response bodies via the [V2BODY] logger.
    /// </summary>
    private static void RunV2Probe(string pathsArg)
    {
        var apiKey = EnvironmentCredentials.GetApiKey() ?? SettingsService.Instance.GetApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Debug.WriteLine("[Probe] No API key available (set RA_API_KEY or save one in settings).");
            return;
        }

        // Run off the UI thread: blocking on async HTTP from the WPF UI thread deadlocks.
        Task.Run(async () =>
        {
            using var client = new V2Client(apiKey, logger: new DebugApiLogger(enabled: true));
            foreach (var path in pathsArg.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                try
                {
                    Debug.WriteLine($"[Probe] GET {path}");
                    await client.GetAsync(path);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Probe] {path} -> {ex.GetType().Name}: {ex.Message}");
                }
            }
        }).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Diagnostic: runs the full HybridProgressService flow for each semicolon-separated game id
    /// and prints a structured summary (sets, types, achievement counts, unlocks per set).
    /// </summary>
    private static void RunGameProbe(string idsArg)
    {
        var username = EnvironmentCredentials.GetUsername() ?? SettingsService.Instance.Settings.Username;
        var apiKey = EnvironmentCredentials.GetApiKey() ?? SettingsService.Instance.GetApiKey();
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(apiKey))
        {
            Debug.WriteLine("[GameProbe] No username/apiKey available (set RA_USERNAME/RA_API_KEY or save credentials).");
            return;
        }

        Task.Run(async () =>
        {
            var flags = new FeatureFlagService(
                useV2ForMetadata: true,
                useV2ForProgress: true,
                useV2ForUserLookup: true,
                enableMultiSet: true,
                enableV1Fallback: true,
                enableApiLogging: true);
            using var hybrid = new HybridProgressService(username!, apiKey!, flags,
                DebugProgressServiceLogger.Instance, new DebugApiLogger(enabled: true));
            // Use the tracking service so we exercise the same code path the UI uses
            // (CreateGameInfoFromProgress builds GameInfo.AchievementSets with per-set Achievements lists).
            using var tracking = new AchievementTrackingService(hybrid, username!);

            foreach (var token in idsArg.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!long.TryParse(token, out var gameId))
                {
                    Debug.WriteLine($"[GameProbe] Skip invalid id: {token}");
                    continue;
                }

                Debug.WriteLine($"[GameProbe] === Game {gameId} ===");
                try
                {
                    var game = await tracking.GetGameByIdAsync(gameId);
                    if (game == null)
                    {
                        Debug.WriteLine($"[GameProbe] Game {gameId}: GetGameByIdAsync returned null");
                        continue;
                    }

                    Debug.WriteLine($"[GameProbe] Game {gameId}: \"{game.Title}\" ({game.ConsoleName})");
                    Debug.WriteLine($"[GameProbe]   Meta: Developer=\"{game.Developer}\" Publisher=\"{game.Publisher}\" Genre=\"{game.Genre}\" Released=\"{game.Released}\"");
                    Debug.WriteLine($"[GameProbe]   Images: Badge=\"{game.BadgeUri}\" BoxArt=\"{game.ImageBoxArt}\" Title=\"{game.ImageTitle}\" Ingame=\"{game.ImageIngame}\"");
                    Debug.WriteLine($"[GameProbe]   HasMultipleSets={game.HasMultipleSets}; AllAchievements={game.AllAchievements.Count}; AllEarned={game.TotalAchievementsEarnedAllSets}/{game.TotalAchievementsAllSets}");

                    if (game.HasMultipleSets)
                    {
                        Debug.WriteLine($"[GameProbe]   AchievementSets ({game.AchievementSets.Count}) — per-set view (UI dropdown switches between these):");
                        foreach (var s in game.AchievementSets)
                        {
                            var firstLocked = s.Achievements.FirstOrDefault(a => !a.DateEarned.HasValue);
                            var focusTitle = firstLocked?.Title ?? "(all unlocked)";
                            Debug.WriteLine($"[GameProbe]     [{s.SetType,-10} id={s.Id,-6} \"{s.Name}\"] earned={s.AchievementsEarned}/{s.AchievementCount} pts={s.PointsEarned}/{s.PointsTotal} | Focus=\"{focusTitle}\"");
                        }
                    }
                    else
                    {
                        var firstLocked = game.Achievements.FirstOrDefault(a => !a.DateEarned.HasValue);
                        var focusTitle = firstLocked?.Title ?? "(all unlocked / no achievements)";
                        Debug.WriteLine($"[GameProbe]   Single-set: earned={game.AchievementsEarned}/{game.AchievementsPossible} pts={game.GamePointsEarned}/{game.GamePointsPossible} | Focus=\"{focusTitle}\"");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[GameProbe] Game {gameId} ERROR: {ex.GetType().Name}: {ex.Message}");
                }
            }
        }).GetAwaiter().GetResult();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _fileListener?.Flush();
        _fileListener?.Dispose();
        base.OnExit(e);
    }
}

