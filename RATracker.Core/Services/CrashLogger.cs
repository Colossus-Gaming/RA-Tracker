using System.IO;
using System.Text;

namespace RATracker.WPF.Services;

/// <summary>
/// Writes unhandled exceptions to a file on disk in every build configuration.
///
/// <para>The developer diagnostics (<c>--log-to-file</c> and the probe commands) are compiled out of
/// Release, which means a Production build otherwise routes <c>Debug.WriteLine</c> nowhere and a
/// tester hitting a crash has nothing to send back. This is deliberately crash-only — it is not a
/// general trace log, so it stays quiet and small during normal use.</para>
///
/// <para>Every method swallows its own failures: a logger that throws while reporting a crash would
/// turn a recoverable error into a fatal one.</para>
/// </summary>
public static class CrashLogger
{
    private const int MaxRetainedFiles = 10;
    private const long MaxFileBytes = 2 * 1024 * 1024; // 2 MB

    private static readonly object Gate = new();

    /// <summary>
    /// Folder holding the crash logs. Lives beside settings.json, outside the install directory, so
    /// it survives updates and is somewhere a tester can be pointed at.
    /// </summary>
    public static string LogDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RATracker",
        "logs");

    /// <summary>
    /// Appends an exception (and its full inner-exception chain) to today's crash log.
    /// </summary>
    public static void Write(string source, Exception? exception)
    {
        try
        {
            var report = new StringBuilder();
            // Plain ASCII only: this file gets opened in whatever editor a tester has and pasted
            // into a bug report, and non-ASCII separators come back as mojibake under the default
            // Windows encoding.
            report.Append("=== ").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                  .Append("  ").Append(source).AppendLine(" ===");
            report.Append("app ").Append(AppVersion).Append("   os ").AppendLine(Environment.OSVersion.VersionString);

            if (exception == null)
            {
                report.AppendLine("(no exception object supplied)");
            }
            else
            {
                var current = exception;
                var depth = 0;
                while (current != null)
                {
                    report.Append("[").Append(depth).Append("] ")
                          .Append(current.GetType().FullName).Append(": ").AppendLine(current.Message);
                    if (!string.IsNullOrWhiteSpace(current.StackTrace))
                    {
                        report.AppendLine(current.StackTrace);
                    }
                    current = current.InnerException;
                    depth++;
                }
            }

            report.AppendLine();
            Append(report.ToString());
        }
        catch
        {
            // Reporting a crash must never itself crash the app.
        }
    }

    /// <summary>Current assembly version, used to tie a report to a specific build.</summary>
    private static string AppVersion =>
        System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

    private static void Append(string text)
    {
        lock (Gate)
        {
            Directory.CreateDirectory(LogDirectory);

            var path = Path.Combine(LogDirectory, $"crash-{DateTime.Now:yyyyMMdd}.log");

            // A crash loop can write the same failure thousands of times; cap the file rather than
            // let it fill the user's disk.
            if (File.Exists(path) && new FileInfo(path).Length > MaxFileBytes) return;

            File.AppendAllText(path, text);
            Prune();
        }
    }

    /// <summary>Keeps only the newest few logs so the folder can't grow unbounded.</summary>
    private static void Prune()
    {
        try
        {
            var stale = new DirectoryInfo(LogDirectory)
                .GetFiles("crash-*.log")
                .OrderByDescending(f => f.LastWriteTimeUtc)
                .Skip(MaxRetainedFiles);

            foreach (var file in stale) file.Delete();
        }
        catch
        {
            // Pruning is best effort.
        }
    }
}
