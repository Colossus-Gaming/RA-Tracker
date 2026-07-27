using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace RATracker.WPF.Converters;

/// <summary>
/// Produces a rounded <see cref="RectangleGeometry"/> from [size, cornerRadius] for use as an
/// <c>Image.Clip</c>, so a badge image's corners actually round.
/// (A Border's <c>CornerRadius</c> + <c>ClipToBounds</c> only clips a child to the SQUARE bounds, so
/// it cannot round a child image — the image must be clipped to the rounded geometry directly.)
/// Re-evaluates live whenever the bound size or corner-radius scalar changes.
/// </summary>
public class SizeRadiusToRoundedRectConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        double size = values.Length > 0 && values[0] is double s ? s : 0;
        double radius = values.Length > 1 && values[1] is double r ? r : 0;
        if (size <= 0) return null;

        radius = Math.Max(0, Math.Min(radius, size / 2)); // clamp so the geometry stays valid
        var geometry = new RectangleGeometry(new Rect(0, 0, size, size), radius, radius);
        geometry.Freeze();
        return geometry;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
