using System;
using System.Windows.Data;
using System.Windows.Markup;
using RATracker.Core.Localization;

namespace RATracker.WPF.Localization;

/// <summary>
/// Resolves a localised string in XAML: <c>Text="{loc:Loc Dash_CurrentGame}"</c>.
/// </summary>
/// <remarks>
/// This produces a real <see cref="Binding"/> against <see cref="LocalizationService"/>'s
/// indexer rather than a one-shot lookup, so changing language re-reads every string in
/// place. The service raises PropertyChanged("Item[]"), which invalidates all indexer
/// bindings at once.
/// <para>
/// It also works inside a <c>Setter</c>: when the binding target is not a dependency
/// property, ProvideValue hands back the Binding itself, which is what Setter.Value wants.
/// </para>
/// </remarks>
[MarkupExtensionReturnType(typeof(string))]
public sealed class LocExtension : MarkupExtension
{
    public LocExtension() { }

    public LocExtension(string key) => Key = key;

    /// <summary>Resource key, matching a row in Localization/translations.tsv.</summary>
    public string Key { get; set; } = string.Empty;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var binding = new Binding($"[{Key}]")
        {
            Source = LocalizationService.Instance,
            Mode = BindingMode.OneWay
        };

        return binding.ProvideValue(serviceProvider);
    }
}
