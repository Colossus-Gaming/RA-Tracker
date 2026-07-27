using System.Diagnostics;
using Velopack;
using Velopack.Sources;

namespace RATracker.WPF.Services;

/// <summary>
/// Checks GitHub Releases for a newer build, downloads it in the background, and stages it to be
/// applied the next time the app closes.
///
/// <para><b>It never restarts the app.</b> This tool runs alongside OBS during a broadcast, and
/// <c>ApplyUpdatesAndRestart</c> would tear down every overlay mid-stream. Instead the update is
/// staged with <see cref="UpdateManager.WaitExitThenApplyUpdates"/> and swapped in after the user
/// closes the app on their own terms.</para>
///
/// <para>Everything here is best effort: a failed update check must never interfere with tracking,
/// so all failures are logged and swallowed.</para>
/// </summary>
public sealed class UpdateService
{
    /// <summary>
    /// Repository whose Releases feed the updater. Must be readable without credentials — Velopack's
    /// GitHub source needs an access token for private repos, and shipping a token inside the app
    /// would hand every tester read access to the source. Use a public repository (either this one
    /// or a dedicated releases-only repo).
    /// </summary>
    public const string RepositoryUrl = "https://github.com/Colossus-Gaming/RA-Tracker";

    /// <summary>Raised with a short human-readable status suitable for the UI.</summary>
    public event EventHandler<string>? StatusChanged;

    /// <summary>True once an update has been downloaded and staged for the next restart.</summary>
    public bool UpdatePending { get; private set; }

    /// <summary>The version staged for install, if any.</summary>
    public string? PendingVersion { get; private set; }

    /// <summary>
    /// Looks for an update and stages it. Safe to call at startup; returns quietly when the app
    /// isn't a Velopack install (developer runs from bin/, or the portable build), when updates are
    /// disabled, or when there's simply nothing new.
    /// </summary>
    public async Task CheckAndStageAsync(bool includePrereleases = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var manager = new UpdateManager(new GithubSource(RepositoryUrl, accessToken: null, prerelease: includePrereleases));

            // Running from a build output or the portable zip — there is no install to update.
            if (!manager.IsInstalled)
            {
                Log("not an installed build; skipping update check");
                return;
            }

            Log($"checking for updates (current {manager.CurrentVersion}, prerelease={includePrereleases})");
            var update = await manager.CheckForUpdatesAsync().ConfigureAwait(false);

            if (update == null)
            {
                Log("already up to date");
                return;
            }

            var version = update.TargetFullRelease.Version?.ToString() ?? "unknown";
            Report($"Downloading update {version}...");

            await manager.DownloadUpdatesAsync(update, cancelToken: cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            // Staged only. restart:false is the whole point — see the class remarks.
            manager.WaitExitThenApplyUpdates(update.TargetFullRelease, silent: true, restart: false);

            UpdatePending = true;
            PendingVersion = version;
            Report($"Update {version} ready — installs when you close the app");
            Log($"staged update {version}");
        }
        catch (OperationCanceledException)
        {
            Log("update check cancelled");
        }
        catch (Exception ex)
        {
            // An unreachable or misconfigured update feed must not affect achievement tracking.
            Log($"update check failed: {ex.GetType().Name}: {ex.Message}");
            CrashLogger.Write("UpdateService", ex);
        }
    }

    private void Report(string status)
    {
        Log(status);
        StatusChanged?.Invoke(this, status);
    }

    private static void Log(string message) => Debug.WriteLine($"[UpdateService] {message}");
}
