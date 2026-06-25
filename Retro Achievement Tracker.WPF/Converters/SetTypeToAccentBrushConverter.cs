using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using RATracker.Models;

namespace RATracker.WPF.Converters;

/// <summary>
/// Converts an <see cref="AchievementSetType"/> to its accent <see cref="Brush"/> for set badges.
/// Accepts an <see cref="AchievementSetType"/> directly, or an <see cref="AchievementSet"/> whose
/// SetType is used. Returns a neutral gray for null/unrecognized input.
/// </summary>
public class SetTypeToAccentBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var type = value switch
        {
            AchievementSetType t => t,
            AchievementSet set => set.SetType,
            _ => AchievementSetType.Unknown
        };
        return AchievementSetVisuals.AccentBrush(type);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
