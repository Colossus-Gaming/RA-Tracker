using Retro_Achievement_Tracker.Services;
using System;
using System.Windows.Forms;

namespace Retro_Achievement_Tracker
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Initialize settings service early to trigger migration if needed
            var settings = SettingsService.Instance;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }
    }
}
