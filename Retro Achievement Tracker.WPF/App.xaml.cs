using System.Diagnostics;
using System.IO;
using System.Windows;

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

        try
        {
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

    protected override void OnExit(ExitEventArgs e)
    {
        _fileListener?.Flush();
        _fileListener?.Dispose();
        base.OnExit(e);
    }
}

