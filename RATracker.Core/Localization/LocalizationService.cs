using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace RATracker.Core.Localization;

/// <summary>
/// Supplies translated UI strings and allows the language to be changed at runtime.
///
/// <para><b>How the UI consumes this.</b> Views bind through the indexer:</para>
/// <code>Text="{Binding [Dashboard_CurrentGame], Source={x:Static loc:LocalizationService.Instance}}"</code>
/// <para>Changing <see cref="CurrentLanguage"/> raises <c>PropertyChanged</c> for the indexer, which
/// makes every bound string re-read itself. That is what lets the language switch take effect
/// immediately instead of requiring a restart.</para>
///
/// <para>Lives in the shared core rather than the WPF project so a non-WPF shell can use the same
/// catalogue and the same saved preference.</para>
/// </summary>
public sealed class LocalizationService : INotifyPropertyChanged
{
    private static readonly ResourceManager Resources =
        new("RATracker.Core.Localization.Strings", typeof(LocalizationService).Assembly);

    public static LocalizationService Instance { get; } = new();

    private CultureInfo _culture = CultureInfo.CurrentUICulture;

    private LocalizationService() { }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Languages offered in the UI. English is the source language and always complete; the rest are
    /// machine-assisted translations that would benefit from a native speaker's review.
    /// </summary>
    public static IReadOnlyList<LanguageOption> AvailableLanguages { get; } = new[]
    {
        new LanguageOption("",   "System default"),
        new LanguageOption("en", "English"),
        new LanguageOption("de", "Deutsch"),
        new LanguageOption("pt", "Português"),
        new LanguageOption("es", "Español"),
        new LanguageOption("fr", "Français"),
        new LanguageOption("it", "Italiano"),
        new LanguageOption("nl", "Nederlands"),
        new LanguageOption("pl", "Polski"),    };

    /// <summary>The active culture. Empty string means "follow the operating system".</summary>
    public string CurrentLanguage
    {
        get => _culture.TwoLetterISOLanguageName;
        private set => Apply(value);
    }

    /// <summary>
    /// Switches language. Pass an empty string to follow the OS. Unknown codes fall back to the OS
    /// culture rather than throwing, so a bad saved value cannot prevent the app from starting.
    /// </summary>
    public void SetLanguage(string? languageCode)
    {
        Apply(languageCode);
    }

    private void Apply(string? languageCode)
    {
        CultureInfo culture;

        if (string.IsNullOrWhiteSpace(languageCode))
        {
            culture = CultureInfo.InstalledUICulture;
        }
        else
        {
            try
            {
                culture = new CultureInfo(languageCode);
            }
            catch (CultureNotFoundException)
            {
                culture = CultureInfo.InstalledUICulture;
            }
        }

        _culture = culture;

        // Affects date/number formatting as well as resource lookup, so a German user sees German
        // date separators too.
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        // "Item[]" is the WPF/Avalonia convention for "every indexer value changed".
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentLanguage)));
    }

    /// <summary>
    /// Looks up a string by key. An unknown key returns the key itself rather than empty, so a
    /// missing translation shows up plainly in the UI instead of silently blanking a label.
    /// </summary>
    public string this[string key]
    {
        get
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;

            try
            {
                return Resources.GetString(key, _culture) ?? key;
            }
            catch (MissingManifestResourceException)
            {
                return key;
            }
        }
    }

    /// <summary>Non-binding lookup, for code that builds strings directly.</summary>
    public static string Get(string key) => Instance[key];
}

/// <summary>A selectable language, shown in its own language rather than translated.</summary>
public sealed class LanguageOption
{
    public LanguageOption(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }

    /// <summary>Two-letter ISO code, or empty for "system default".</summary>
    public string Code { get; }

    public string DisplayName { get; }

    public override string ToString() => DisplayName;
}
