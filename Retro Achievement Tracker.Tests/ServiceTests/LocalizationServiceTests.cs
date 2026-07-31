using System.Globalization;
using System.Linq;
using System.Resources;
using RATracker.Core.Localization;

namespace RATracker.Tests.ServiceTests;

/// <summary>
/// Guards the translation catalogue.
/// </summary>
/// <remarks>
/// The failure mode these protect against is silent: if a satellite assembly is missing
/// or a key is misspelled, ResourceManager quietly falls back to English (or to the raw
/// key) and the app still runs. Nothing surfaces the problem except a user noticing that
/// picking their language did nothing.
/// </remarks>
[TestFixture]
public class LocalizationServiceTests
{
    private static readonly ResourceManager Resources =
        new("RATracker.Core.Localization.Strings", typeof(LocalizationService).Assembly);

    /// <summary>Every language offered in the dropdown, minus the "system default" entry.</summary>
    private static string[] TranslatedLanguageCodes =>
        LocalizationService.AvailableLanguages
            .Select(l => l.Code)
            .Where(c => !string.IsNullOrEmpty(c))
            .ToArray()!;

    /// <summary>Languages that ship as a satellite assembly - i.e. everything but the neutral catalogue.</summary>
    private static string[] SatelliteLanguageCodes =>
        TranslatedLanguageCodes.Where(c => c != "en").ToArray();

    private static string[] AllKeys =>
        Resources.GetResourceSet(CultureInfo.InvariantCulture, createIfNotExists: true, tryParents: true)!
            .Cast<System.Collections.DictionaryEntry>()
            .Select(e => (string)e.Key)
            .ToArray();

    [Test]
    public void AvailableLanguages_IncludesTheLanguagesTestersAskedFor()
    {
        var codes = LocalizationService.AvailableLanguages.Select(l => l.Code).ToArray();

        Assert.That(codes, Does.Contain("de"), "German was requested by a tester.");
        Assert.That(codes, Does.Contain("pt"), "Portuguese was requested by a tester.");
        Assert.That(codes, Does.Contain(null).Or.Contain(string.Empty),
            "There must be a 'system default' option that follows the OS language.");
    }

    [TestCaseSource(nameof(TranslatedLanguageCodes))]
    public void EverySatelliteAssemblyResolvesEveryKey(string code)
    {
        var culture = new CultureInfo(code);

        var missing = AllKeys
            .Where(key => Resources.GetString(key, culture) is null)
            .ToArray();

        Assert.That(missing, Is.Empty,
            $"'{code}' has no value for: {string.Join(", ", missing)}. " +
            "Re-run RATracker.Core/Localization/generate-resx.ps1.");
    }

    [TestCaseSource(nameof(SatelliteLanguageCodes))]
    public void SatelliteAssemblyIsActuallyDeployed(string code)
    {
        // English is the neutral catalogue, so a *missing* satellite would still return
        // the English text rather than null. Comparing against English is what proves the
        // localised resources shipped at all.
        var culture = new CultureInfo(code);
        var differsFromEnglish = AllKeys.Count(
            key => Resources.GetString(key, culture) != Resources.GetString(key, CultureInfo.GetCultureInfo("en")));

        Assert.That(differsFromEnglish, Is.GreaterThan(0),
            $"No string differs from English for '{code}' - the satellite assembly is missing from the build output.");
    }

    [Test]
    public void SwitchingLanguage_NotifiesBoundControls()
    {
        var service = LocalizationService.Instance;
        var original = service.CurrentLanguage;
        var notifications = 0;

        void Handler(object? _, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // WPF treats "Item[]" as "every indexer binding is stale", which is what makes
            // the language switch apply without a restart.
            if (e.PropertyName == "Item[]") notifications++;
        }

        service.PropertyChanged += Handler;
        try
        {
            service.SetLanguage("de");
            Assert.Multiple(() =>
            {
                Assert.That(notifications, Is.GreaterThan(0), "Bound controls were never told to re-read.");
                Assert.That(service["Settings_Language"], Is.EqualTo("Sprache"));
            });
        }
        finally
        {
            service.PropertyChanged -= Handler;
            service.SetLanguage(original);
        }
    }

    [Test]
    public void UnknownKey_FallsBackToTheKeyRatherThanThrowing()
    {
        Assert.That(LocalizationService.Instance["No_Such_Key"], Is.EqualTo("No_Such_Key"));
    }

    /// <summary>
    /// "System default" has to keep meaning the OS language, not "whatever was picked last".
    /// </summary>
    /// <remarks>
    /// Apply() overwrites CurrentUICulture, so resolving the empty code by reading
    /// CurrentUICulture at call time would make this a no-op: a user who picked German and
    /// then chose "System default" would just stay in German with no way back.
    /// </remarks>
    [Test]
    public void SystemDefault_ReturnsToTheOsLanguage_NotTheLastSelectedOne()
    {
        var service = LocalizationService.Instance;
        var osLanguage = service.CurrentLanguage;   // nothing selected yet in this fixture

        try
        {
            service.SetLanguage("de");
            Assert.That(service.CurrentLanguage, Is.EqualTo("de"), "sanity: explicit selection applies");

            service.SetLanguage("");
            Assert.That(service.CurrentLanguage, Is.EqualTo(osLanguage),
                "'System default' stayed on the previously selected language instead of the OS language.");
        }
        finally
        {
            service.SetLanguage("");
        }
    }

    [Test]
    public void UnknownLanguageCode_FallsBackToTheOsLanguageRatherThanThrowing()
    {
        var service = LocalizationService.Instance;
        try
        {
            Assert.DoesNotThrow(() => service.SetLanguage("not-a-culture"));
            Assert.That(service.CurrentLanguage, Is.Not.Empty);
        }
        finally
        {
            service.SetLanguage("");
        }
    }
}
