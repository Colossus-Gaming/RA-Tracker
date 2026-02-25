using System.IO;

namespace RATracker.Models
{
    class MediaHelper
    {
        // Default video width used when actual dimensions cannot be determined
        private const int DefaultVideoWidth = 1028;

        internal static string HexConverter(Color color)
        {
            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }

        /// <summary>
        /// Gets the video width for scaling purposes.
        /// Returns default width since MediaToolkit is not compatible with .NET 8.
        /// Custom notification videos will use their scale factor applied to the default width.
        /// </summary>
        /// <param name="input">Path to the video file</param>
        /// <returns>Default video width if file exists, 0 otherwise</returns>
        internal static decimal GetVideoWidth(string input)
        {
            if (File.Exists(input))
            {
                // MediaToolkit was removed due to .NET 8 incompatibility.
                // Return default width - the scale factor will be applied by the caller.
                return DefaultVideoWidth;
            }
            return 0;
        }

        /// <summary>
        /// Gets the video duration in milliseconds.
        /// Returns 0 since MediaToolkit is not compatible with .NET 8.
        /// This method is kept for API compatibility but video duration 
        /// should be configured manually via the UI settings.
        /// </summary>
        /// <param name="input">Path to the video file</param>
        /// <returns>0 (duration should be configured via UI settings)</returns>
        internal static int GetVideoDuration(string input)
        {
            // MediaToolkit was removed due to .NET 8 incompatibility.
            // Video duration should be configured via the application settings.
            return 0;
        }
    }
}
