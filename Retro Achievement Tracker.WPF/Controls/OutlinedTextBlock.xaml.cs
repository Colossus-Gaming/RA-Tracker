using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RATracker.WPF.Controls;

/// <summary>
/// A text block that displays text with an outline/stroke effect.
/// Mimics the CSS text-stroke effect from the original HTML overlays.
/// </summary>
public partial class OutlinedTextBlock : UserControl
{
    #region Dependency Properties

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(OutlinedTextBlock),
            new PropertyMetadata(string.Empty, OnVisualPropertyChanged));

    public static readonly DependencyProperty TextColorProperty =
        DependencyProperty.Register(nameof(TextColor), typeof(Brush), typeof(OutlinedTextBlock),
            new PropertyMetadata(Brushes.White, OnVisualPropertyChanged));

    public static readonly DependencyProperty StrokeColorProperty =
        DependencyProperty.Register(nameof(StrokeColor), typeof(Brush), typeof(OutlinedTextBlock),
            new PropertyMetadata(Brushes.Black, OnVisualPropertyChanged));

    public static readonly DependencyProperty StrokeThicknessProperty =
        DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(OutlinedTextBlock),
            new PropertyMetadata(2.0, OnVisualPropertyChanged));

    public static readonly DependencyProperty StrokeEnabledProperty =
        DependencyProperty.Register(nameof(StrokeEnabled), typeof(bool), typeof(OutlinedTextBlock),
            new PropertyMetadata(true, OnVisualPropertyChanged));

    public static readonly DependencyProperty TextFontFamilyProperty =
        DependencyProperty.Register(nameof(TextFontFamily), typeof(FontFamily), typeof(OutlinedTextBlock),
            new PropertyMetadata(new FontFamily("Segoe UI"), OnVisualPropertyChanged));

    public static readonly DependencyProperty TextFontSizeProperty =
        DependencyProperty.Register(nameof(TextFontSize), typeof(double), typeof(OutlinedTextBlock),
            new PropertyMetadata(24.0, OnVisualPropertyChanged));

    public static readonly DependencyProperty TextFontWeightProperty =
        DependencyProperty.Register(nameof(TextFontWeight), typeof(FontWeight), typeof(OutlinedTextBlock),
            new PropertyMetadata(FontWeights.Bold, OnVisualPropertyChanged));

    public static readonly DependencyProperty TextWrappingProperty =
        DependencyProperty.Register(nameof(TextWrapping), typeof(TextWrapping), typeof(OutlinedTextBlock),
            new PropertyMetadata(TextWrapping.NoWrap, OnVisualPropertyChanged));

    public static readonly DependencyProperty TextAlignmentProperty =
        DependencyProperty.Register(nameof(TextAlignment), typeof(TextAlignment), typeof(OutlinedTextBlock),
            new PropertyMetadata(TextAlignment.Left, OnVisualPropertyChanged));

    public static readonly DependencyProperty VerticalTextAlignmentProperty =
        DependencyProperty.Register(nameof(VerticalTextAlignment), typeof(VerticalAlignment), typeof(OutlinedTextBlock),
            new PropertyMetadata(VerticalAlignment.Center, OnVisualPropertyChanged));

    #endregion

    #region Properties

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public Brush TextColor
    {
        get => (Brush)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public Brush StrokeColor
    {
        get => (Brush)GetValue(StrokeColorProperty);
        set => SetValue(StrokeColorProperty, value);
    }

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public bool StrokeEnabled
    {
        get => (bool)GetValue(StrokeEnabledProperty);
        set => SetValue(StrokeEnabledProperty, value);
    }

    public FontFamily TextFontFamily
    {
        get => (FontFamily)GetValue(TextFontFamilyProperty);
        set => SetValue(TextFontFamilyProperty, value);
    }

    public double TextFontSize
    {
        get => (double)GetValue(TextFontSizeProperty);
        set => SetValue(TextFontSizeProperty, value);
    }

    public FontWeight TextFontWeight
    {
        get => (FontWeight)GetValue(TextFontWeightProperty);
        set => SetValue(TextFontWeightProperty, value);
    }

    public TextWrapping TextWrapping
    {
        get => (TextWrapping)GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }

    public TextAlignment TextAlignment
    {
        get => (TextAlignment)GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    public VerticalAlignment VerticalTextAlignment
    {
        get => (VerticalAlignment)GetValue(VerticalTextAlignmentProperty);
        set => SetValue(VerticalTextAlignmentProperty, value);
    }

    #endregion

    public OutlinedTextBlock()
    {
        InitializeComponent();
        Loaded += (s, e) => RebuildVisuals();
    }

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is OutlinedTextBlock control && control.IsLoaded)
        {
            control.RebuildVisuals();
        }
    }

    private void RebuildVisuals()
    {
        LayoutRoot.Children.Clear();

        if (StrokeEnabled && StrokeThickness > 0)
        {
            // Create stroke effect by rendering text at 8 positions around the center
            var offsets = new (double x, double y)[]
            {
                (-StrokeThickness, -StrokeThickness),
                (0, -StrokeThickness),
                (StrokeThickness, -StrokeThickness),
                (-StrokeThickness, 0),
                (StrokeThickness, 0),
                (-StrokeThickness, StrokeThickness),
                (0, StrokeThickness),
                (StrokeThickness, StrokeThickness),
            };

            foreach (var (x, y) in offsets)
            {
                var strokeText = CreateTextBlock(StrokeColor);
                strokeText.RenderTransform = new TranslateTransform(x, y);
                LayoutRoot.Children.Add(strokeText);
            }
        }

        // Add the main text on top
        var mainText = CreateTextBlock(TextColor);
        LayoutRoot.Children.Add(mainText);
    }

    private TextBlock CreateTextBlock(Brush foreground)
    {
        return new TextBlock
        {
            Text = Text,
            Foreground = foreground,
            FontFamily = TextFontFamily,
            FontSize = TextFontSize,
            FontWeight = TextFontWeight,
            TextWrapping = TextWrapping,
            TextAlignment = TextAlignment,
            VerticalAlignment = VerticalTextAlignment,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
    }
}
