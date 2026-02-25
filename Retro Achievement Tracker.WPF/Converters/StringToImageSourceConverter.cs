using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RATracker.WPF.Converters;

/// <summary>
/// Converts a string URI to an ImageSource.
/// Returns null for null or empty strings, preventing binding errors.
/// </summary>
public class StringToImageSourceConverter : IValueConverter
{
    /// <summary>
    /// Converts a string URI to an ImageSource.
    /// Returns null if the string is null, empty, or whitespace.
    /// </summary>
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string uri || string.IsNullOrWhiteSpace(uri))
            return null;

        try
        {
            return new BitmapImage(new Uri(uri, UriKind.RelativeOrAbsolute));
        }
        catch
        {
            // Return null if the URI is invalid
            return null;
        }
    }

    /// <summary>
    /// Not supported - returns null.
    /// </summary>
    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}
