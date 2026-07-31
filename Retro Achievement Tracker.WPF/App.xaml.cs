using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using Newtonsoft.Json.Linq;
using RATracker.WPF.Http.V2;
using RATracker.WPF.Services;
using Velopack;

namespace RATracker.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
#if DEV_TOOLS
    private TextWriterTraceListener? _fileListener;
#endif

    protected override void OnStartup(StartupEventArgs e)
    {
        // MUST be the first thing that runs. Velopack re-invokes this exe with its own hook
        // arguments during install, update and uninstall; if anything else runs first (argument
        // parsing, showing a window) those hooks misbehave. The packaging step also refuses to
        // build unless this call is present.
        VelopackApp.Build().Run();

        base.OnStartup(e);

        // Apply the saved UI language before any window is created, so the first frame is already
        // in the right language rather than flashing English.
        RATracker.Core.Localization.LocalizationService.Instance
            .SetLanguage(SettingsService.Instance.Settings.Language);

        var args = e.Args;

#if DEV_TOOLS
        // Set up file logging first so anything the probes or a crash produce is captured.
        ConfigureFileLogging(args);
#endif

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

#if DEV_TOOLS
        // Diagnostic entry points. Each runs instead of the app and then exits.
        if (TryRunDeveloperTool(args)) return;
#endif

        try
        {
            // No game pin: the tracker follows the most recently played game. To pin one while
            // developing, set AchievementTrackingService.DebugForceGameId here (Debug builds only —
            // see the DEV_TOOLS block below) and remove it again before committing.
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

    /// <summary>
    /// Records an unhandled exception. Goes to the debug trace (useful with --log-to-file during
    /// development) and to <see cref="CrashLogger"/>, which writes to disk in every configuration so
    /// a Production build still leaves something a tester can send back.
    /// </summary>
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

        CrashLogger.Write(source, ex);
    }

#if DEV_TOOLS

    #region Developer tools (Debug builds only)

    /// <summary>
    /// <c>--log-to-file &lt;path&gt;</c>: redirects Debug/Trace output to a file. Used by the
    /// integration tests and by the probes below.
    /// </summary>
    private void ConfigureFileLogging(string[] args)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] != "--log-to-file") continue;

            var logPath = args[i + 1];
            var dir = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            // FileShare.Read so a test harness can read the log while the app is still writing it.
            var logStream = new FileStream(logPath, FileMode.Create, FileAccess.Write, FileShare.Read);
            _fileListener = new TextWriterTraceListener(logStream) { TraceOutputOptions = TraceOptions.Timestamp };
            // Debug.WriteLine routes through Trace.Listeners in .NET 8.
            Trace.Listeners.Add(_fileListener);
            Trace.AutoFlush = true;
            Debug.WriteLine($"[App] File logging enabled: {logPath}");
            return;
        }
    }

    /// <summary>
    /// Dispatches the diagnostic command-line entry points. Returns true when one ran, meaning the
    /// app should not start normally.
    /// <list type="bullet">
    /// <item><c>--probe-v2 "&lt;path&gt;;&lt;path&gt;"</c> — raw v2 GETs, logs response bodies.</item>
    /// <item><c>--probe-game "&lt;id&gt;;&lt;id&gt;"</c> — full progress flow per game id.</item>
    /// <item><c>--probe-alerts "&lt;kind&gt;;&lt;video&gt;;…"</c> — fires one custom alert.</item>
    /// <item><c>--find-game "&lt;text&gt;"</c> — searches the user's library by title.</item>
    /// </list>
    /// </summary>
    private bool TryRunDeveloperTool(string[] args)
    {
        var probeIdx = Array.IndexOf(args, "--probe-v2");
        if (probeIdx >= 0 && probeIdx < args.Length - 1)
        {
            RunV2Probe(args[probeIdx + 1]);
            Shutdown();
            return true;
        }

        // Leaves the overlay up rather than shutting down, so the alert can be watched.
        var alertsIdx = Array.IndexOf(args, "--probe-alerts");
        if (alertsIdx >= 0 && alertsIdx < args.Length - 1)
        {
            RunAlertsProbe(args[alertsIdx + 1]);
            return true;
        }

        var findIdx = Array.IndexOf(args, "--find-game");
        if (findIdx >= 0 && findIdx < args.Length - 1)
        {
            RunFindGameProbe(args[findIdx + 1]);
            Shutdown();
            return true;
        }

        var gameIdx = Array.IndexOf(args, "--probe-game");
        if (gameIdx >= 0 && gameIdx < args.Length - 1)
        {
            RunGameProbe(args[gameIdx + 1]);
            Shutdown();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Diagnostic: finds games in the user's library whose title contains <paramref name="search"/>,
    /// reporting each match's game id and unlock progress.
    /// </summary>
    private static void RunFindGameProbe(string search)
    {
        var username = EnvironmentCredentials.GetUsername() ?? SettingsService.Instance.Settings.Username;
        var apiKey = EnvironmentCredentials.GetApiKey() ?? SettingsService.Instance.GetApiKey();
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(apiKey))
        {
            Debug.WriteLine("[FindGame] No username/apiKey available.");
            return;
        }

        Task.Run(async () =>
        {
            using var client = new V2Client(apiKey!);
            int page = 1, scanned = 0, matches = 0;

            while (page <= 50) // safety cap; 50 * 100 covers any realistic library
            {
                var query = V2QueryBuilder.Create()
                    .Include("game")
                    .PageSize(100)
                    .Page(page)
                    .SortDescending("lastPlayedAt");

                var document = await client.GetRelationshipAsync("users", username!, "player-games", query);
                if (document.HasErrors) break;

                var included = document.GetIncludedIndex();
                foreach (var record in document.GetResourceCollection())
                {
                    var gameRef = record.GetRelationship("game")?.GetSingleIdentifier();
                    if (gameRef == null) continue;

                    included.TryGetValue((gameRef.Type, gameRef.Id), out var game);
                    var title = game?.GetAttribute<string>("title") ?? string.Empty;
                    scanned++;

                    if (title.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0) continue;

                    // player-games does NOT carry an unlock count (verified live: the attribute is
                    // absent, so a "?? 0" here would report every game as 0 earned). Print "?" and
                    // leave the authoritative count to --probe-game, which sources it from v1.
                    var earned = record.GetAttribute<int?>("achievementsUnlocked")
                                 ?? record.GetAttribute<int?>("achievementsUnlockedHardcore");
                    var total = game?.GetAttribute<int?>("achievementsPublished") ?? 0;
                    var lastPlayed = record.GetAttribute<string>("lastPlayedAt") ?? "?";

                    Debug.WriteLine($"[FindGame] id={gameRef.Id} \"{title}\" " +
                                    $"earned={(earned.HasValue ? earned.Value.ToString(CultureInfo.InvariantCulture) : "?")}/{total} " +
                                    $"lastPlayed={lastPlayed}");
                    matches++;
                }

                var lastPage = document.Meta?["page"]?["lastPage"]?.Value<int>() ?? page;
                if (page >= lastPage) break;
                page++;
            }

            Debug.WriteLine($"[FindGame] {matches} match(es) for \"{search}\" across {scanned} played game(s)");
        }).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Diagnostic: shows the Alerts overlay and plays a single custom alert with an explicit
    /// configuration. Applies the settings to the overlay's view model only — nothing is persisted,
    /// so probing never disturbs the saved configuration.
    /// </summary>
    private static void RunAlertsProbe(string spec)
    {
        var parts = spec.Split(';', StringSplitOptions.TrimEntries);
        string Part(int i, string fallback) => parts.Length > i && parts[i].Length > 0 ? parts[i] : fallback;

        var isMastery = Part(0, "achievement").Equals("mastery", StringComparison.OrdinalIgnoreCase);
        var video = Part(1, string.Empty);
        var inv = CultureInfo.InvariantCulture;

        var overlay = new Views.AlertsOverlay();
        var vm = overlay.ViewModel;

        var x = double.Parse(Part(2, "0"), inv);
        var y = double.Parse(Part(3, "0"), inv);
        var scale = double.Parse(Part(4, "1"), inv);
        var inMs = int.Parse(Part(5, "0"), inv);
        var outMs = int.Parse(Part(6, "5200"), inv);
        var inDir = Enum.Parse<ViewModels.AnimationDirection>(Part(7, "Static"), ignoreCase: true);
        var outDir = Enum.Parse<ViewModels.AnimationDirection>(Part(8, "Up"), ignoreCase: true);

        if (isMastery)
        {
            vm.CustomMasteryEnabled = true;
            vm.CustomMasteryVideoPath = video;
            vm.CustomMasteryX = x;
            vm.CustomMasteryY = y;
            vm.CustomMasteryScale = scale;
            vm.CustomMasteryInTime = inMs;
            vm.CustomMasteryOutTime = outMs;
            vm.MasteryInDirection = inDir;
            vm.MasteryOutDirection = outDir;
        }
        else
        {
            vm.CustomAchievementEnabled = true;
            vm.CustomAchievementVideoPath = video;
            vm.CustomAchievementX = x;
            vm.CustomAchievementY = y;
            vm.CustomAchievementScale = scale;
            vm.CustomAchievementInTime = inMs;
            vm.CustomAchievementOutTime = outMs;
            vm.AchievementInDirection = inDir;
            vm.AchievementOutDirection = outDir;
        }

        Debug.WriteLine($"[AlertsProbe] kind={(isMastery ? "mastery" : "achievement")} video=\"{video}\" " +
                        $"x={x} y={y} scale={scale} in={inMs}ms out={outMs}ms inDir={inDir} outDir={outDir}");

        overlay.Loaded += async (_, _) =>
        {
            overlay.Left = 300;
            overlay.Top = 200;

            if (isMastery)
            {
                await overlay.ShowTestMasteryNotification();
            }
            else
            {
                await overlay.ShowTestAchievementNotification();
            }

            Debug.WriteLine("[AlertsProbe] alert finished");
        };
        overlay.Show();
    }

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

    #endregion

#endif

    protected override void OnExit(ExitEventArgs e)
    {
        Debug.WriteLine($"[App] OnExit reached (code {e.ApplicationExitCode})");
#if DEV_TOOLS
        _fileListener?.Flush();
        _fileListener?.Dispose();
#endif
        base.OnExit(e);
    }
}

