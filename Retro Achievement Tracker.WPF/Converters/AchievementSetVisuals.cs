using System.Windows.Media;
using RATracker.Models;

namespace RATracker.WPF.Converters;

/// <summary>
/// Single source of truth for the per-set-type accent colors used by both the Alerts overlay
/// badge/border and the dashboard's Current Game set badge, so they stay visually consistent.
/// All brushes are frozen and cached, so repeated calls return the same instance.
/// </summary>
public static class AchievementSetVisuals
{
    private static readonly Brush CoreAccent = Frozen(0x5B, 0x61, 0x6E);      // slate (white text readable)
    private static readonly Brush BonusAccent = Frozen(0x9B, 0x59, 0xD6);     // amethyst
    private static readonly Brush SpecialtyAccent = Frozen(0x2E, 0xA3, 0xFF); // sky blue
    private static readonly Brush ExclusiveAccent = Frozen(0xFF, 0x57, 0x57); // red
    private static readonly Brush ChallengeAccent = Frozen(0xFF, 0xB3, 0x00); // amber
    private static readonly Brush UnknownAccent = Frozen(0x9E, 0x9E, 0x9E);   // gray

    /// <summary>
    /// Gets the accent brush for a set type. Core gets a neutral slate; Bonus/Specialty/Exclusive/
    /// Challenge each get a distinct hue; anything else gets a neutral gray.
    /// </summary>
    public static Brush AccentBrush(AchievementSetType type) => type switch
    {
        AchievementSetType.Core => CoreAccent,
        AchievementSetType.Bonus => BonusAccent,
        AchievementSetType.Specialty => SpecialtyAccent,
        AchievementSetType.Exclusive => ExclusiveAccent,
        AchievementSetType.Challenge => ChallengeAccent,
        _ => UnknownAccent
    };

    private static Brush Frozen(byte r, byte g, byte b)
    {
        var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }
}
