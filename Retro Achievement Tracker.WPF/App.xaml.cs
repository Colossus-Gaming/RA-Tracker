using System.Windows;

namespace RATracker.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
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
                System.Diagnostics.Debug.WriteLine($"Exception depth {depth}: {current.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"Message: {current.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack: {current.StackTrace}");
                System.Diagnostics.Debug.WriteLine("---");
                current = current.InnerException;
                depth++;
            }
            throw;
        }
    }
}

