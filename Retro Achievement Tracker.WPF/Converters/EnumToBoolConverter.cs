using System.Globalization;
using System.Windows.Data;

namespace RATracker.WPF.Converters;

/// <summary>
/// Converts an enum value to a boolean for use with RadioButton IsChecked binding.
/// The ConverterParameter should be the string representation of the enum value.
/// </summary>
public class EnumToBoolConverter : IValueConverter
{
    /// <summary>
    /// Converts an enum value to true if it matches the parameter, false otherwise.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        var enumValue = value.ToString();
        var targetValue = parameter.ToString();

        return enumValue?.Equals(targetValue, StringComparison.OrdinalIgnoreCase) ?? false;
    }

    /// <summary>
    /// Converts true back to the enum value specified in the parameter.
    /// </summary>
    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isChecked && isChecked && parameter != null)
        {
            var targetValue = parameter.ToString();
            if (targetType.IsEnum && targetValue != null)
            {
                return Enum.Parse(targetType, targetValue, ignoreCase: true);
            }
        }

        return Binding.DoNothing;
    }
}
