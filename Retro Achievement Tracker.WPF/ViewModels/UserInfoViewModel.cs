using System.Windows;
using System.Windows.Media;
using RATracker.Models;

namespace RATracker.WPF.ViewModels;

/// <summary>
/// ViewModel for the User Info Overlay window.
/// Displays user rank, points, true points, and ratio.
/// </summary>
public class UserInfoViewModel : ViewModelBase
{
    #region Private Fields

    // Content fields
    private string _rankLabel = "Rank:";
    private string _rankValue = "#1234";
    private string _pointsLabel = "Points:";
    private string _pointsValue = "12,345";
    private string _truePointsLabel = "True Points:";
    private string _truePointsValue = "45,678";
    private string _ratioLabel = "Ratio:";
    private string _ratioValue = "3.70";

    // Visibility
    private bool _showRank = true;
    private bool _showPoints = true;
    private bool _showTruePoints = true;
    private bool _showRatio = true;

    // Layout settings
    private double _windowWidth = 400;
    private double _windowHeight = 200;
    private double _containerCornerRadius = 8;
    private double _containerMargin = 5;
    private double _rowSpacing = 8;

    // Font sizes
    private double _labelFontSize = 18;
    private double _valueFontSize = 24;

    // Simple mode settings
    private FontFamily _simpleFontFamily = new("Segoe UI");
    private Brush _simpleFontColor = Brushes.White;
    private Brush _simpleStrokeColor = Brushes.Black;
    private double _simpleStrokeSize = 2;
    private bool _simpleStrokeEnabled = true;

    // Advanced mode - Label
    private FontFamily _labelFontFamily = new("Segoe UI");
    private Brush _labelColor = Brushes.Gold;
    private Brush _labelStrokeColor = Brushes.Black;
    private double _labelStrokeSize = 2;
    private bool _labelStrokeEnabled = true;

    // Advanced mode - Value
    private FontFamily _valueFontFamily = new("Segoe UI");
    private Brush _valueColor = Brushes.White;
    private Brush _valueStrokeColor = Brushes.Black;
    private double _valueStrokeSize = 2;
    private bool _valueStrokeEnabled = true;

    // Window settings
    private Brush _backgroundColor = new SolidColorBrush(Color.FromArgb(200, 50, 50, 50));
    private Brush _borderColor = Brushes.Gold;
    private bool _borderEnabled = true;
    private bool _advancedSettingsEnabled;

    #endregion

    #region Content Properties

    public string RankLabel
    {
        get => _rankLabel;
        set => SetProperty(ref _rankLabel, value);
    }

    public string RankValue
    {
        get => _rankValue;
        set => SetProperty(ref _rankValue, value);
    }

    public string PointsLabel
    {
        get => _pointsLabel;
        set => SetProperty(ref _pointsLabel, value);
    }

    public string PointsValue
    {
        get => _pointsValue;
        set => SetProperty(ref _pointsValue, value);
    }

    public string TruePointsLabel
    {
        get => _truePointsLabel;
        set => SetProperty(ref _truePointsLabel, value);
    }

    public string TruePointsValue
    {
        get => _truePointsValue;
        set => SetProperty(ref _truePointsValue, value);
    }

    public string RatioLabel
    {
        get => _ratioLabel;
        set => SetProperty(ref _ratioLabel, value);
    }

    public string RatioValue
    {
        get => _ratioValue;
        set => SetProperty(ref _ratioValue, value);
    }

    #endregion

    #region Visibility Properties

    public bool ShowRank
    {
        get => _showRank;
        set => SetProperty(ref _showRank, value);
    }

    public bool ShowPoints
    {
        get => _showPoints;
        set => SetProperty(ref _showPoints, value);
    }

    public bool ShowTruePoints
    {
        get => _showTruePoints;
        set => SetProperty(ref _showTruePoints, value);
    }

    public bool ShowRatio
    {
        get => _showRatio;
        set => SetProperty(ref _showRatio, value);
    }

    #endregion

    #region Layout Properties

    public double WindowWidth
    {
        get => _windowWidth;
        set => SetProperty(ref _windowWidth, value);
    }

    public double WindowHeight
    {
        get => _windowHeight;
        set => SetProperty(ref _windowHeight, value);
    }

    public double ContainerCornerRadius
    {
        get => _containerCornerRadius;
        set => SetProperty(ref _containerCornerRadius, value);
    }

    public CornerRadius ContainerCornerRadiusValue => new(_containerCornerRadius);

    public double ContainerMargin
    {
        get => _containerMargin;
        set => SetProperty(ref _containerMargin, value);
    }

    public Thickness ContainerMarginValue => new(_containerMargin);

    public double RowSpacing
    {
        get => _rowSpacing;
        set => SetProperty(ref _rowSpacing, value);
    }

    public Thickness RowSpacingValue => new(0, _rowSpacing / 2, 0, _rowSpacing / 2);

    public double LabelFontSize
    {
        get => _labelFontSize;
        set => SetProperty(ref _labelFontSize, value);
    }

    public double ValueFontSize
    {
        get => _valueFontSize;
        set => SetProperty(ref _valueFontSize, value);
    }

    #endregion

    #region Simple Mode Properties

    public FontFamily SimpleFontFamily
    {
        get => _simpleFontFamily;
        set { if (SetProperty(ref _simpleFontFamily, value)) NotifyFontPropertiesChanged(); }
    }

    public Brush SimpleFontColor
    {
        get => _simpleFontColor;
        set { if (SetProperty(ref _simpleFontColor, value)) NotifyColorPropertiesChanged(); }
    }

    public Brush SimpleStrokeColor
    {
        get => _simpleStrokeColor;
        set { if (SetProperty(ref _simpleStrokeColor, value)) NotifyColorPropertiesChanged(); }
    }

    public double SimpleStrokeSize
    {
        get => _simpleStrokeSize;
        set { if (SetProperty(ref _simpleStrokeSize, value)) NotifyColorPropertiesChanged(); }
    }

    public bool SimpleStrokeEnabled
    {
        get => _simpleStrokeEnabled;
        set { if (SetProperty(ref _simpleStrokeEnabled, value)) NotifyColorPropertiesChanged(); }
    }

    #endregion

    #region Advanced Mode - Label Properties

    public FontFamily LabelFontFamily
    {
        get => _advancedSettingsEnabled ? _labelFontFamily : _simpleFontFamily;
        set => SetProperty(ref _labelFontFamily, value);
    }

    public Brush LabelColor
    {
        get => _advancedSettingsEnabled ? _labelColor : _simpleFontColor;
        set => SetProperty(ref _labelColor, value);
    }

    public Brush LabelStrokeColor
    {
        get => _advancedSettingsEnabled ? _labelStrokeColor : _simpleStrokeColor;
        set => SetProperty(ref _labelStrokeColor, value);
    }

    public double LabelStrokeSize
    {
        get => _advancedSettingsEnabled ? _labelStrokeSize : _simpleStrokeSize;
        set => SetProperty(ref _labelStrokeSize, value);
    }

    public bool LabelStrokeEnabled
    {
        get => _advancedSettingsEnabled ? _labelStrokeEnabled : _simpleStrokeEnabled;
        set => SetProperty(ref _labelStrokeEnabled, value);
    }

    #endregion

    #region Advanced Mode - Value Properties

    public FontFamily ValueFontFamily
    {
        get => _advancedSettingsEnabled ? _valueFontFamily : _simpleFontFamily;
        set => SetProperty(ref _valueFontFamily, value);
    }

    public Brush ValueColor
    {
        get => _advancedSettingsEnabled ? _valueColor : _simpleFontColor;
        set => SetProperty(ref _valueColor, value);
    }

    public Brush ValueStrokeColor
    {
        get => _advancedSettingsEnabled ? _valueStrokeColor : _simpleStrokeColor;
        set => SetProperty(ref _valueStrokeColor, value);
    }

    public double ValueStrokeSize
    {
        get => _advancedSettingsEnabled ? _valueStrokeSize : _simpleStrokeSize;
        set => SetProperty(ref _valueStrokeSize, value);
    }

    public bool ValueStrokeEnabled
    {
        get => _advancedSettingsEnabled ? _valueStrokeEnabled : _simpleStrokeEnabled;
        set => SetProperty(ref _valueStrokeEnabled, value);
    }

    #endregion

    #region Window Properties

    public Brush BackgroundColor
    {
        get => _backgroundColor;
        set => SetProperty(ref _backgroundColor, value);
    }

    public Brush BorderColor
    {
        get => _borderColor;
        set => SetProperty(ref _borderColor, value);
    }

    public bool BorderEnabled
    {
        get => _borderEnabled;
        set => SetProperty(ref _borderEnabled, value);
    }

    public bool AdvancedSettingsEnabled
    {
        get => _advancedSettingsEnabled;
        set
        {
            if (SetProperty(ref _advancedSettingsEnabled, value))
            {
                NotifyFontPropertiesChanged();
                NotifyColorPropertiesChanged();
            }
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Updates the user info display from a UserSummary object.
    /// </summary>
    public void SetUserInfo(UserSummary userSummary)
    {
        RankValue = $"#{userSummary.Rank:N0}";
        PointsValue = userSummary.TotalPoints.ToString("N0");
        TruePointsValue = userSummary.TotalTruePoints.ToString("N0");
        RatioValue = userSummary.RetroRatio;
    }

    /// <summary>
    /// Sets sample data for demo/design purposes.
    /// </summary>
    public void SetSampleData()
    {
        RankValue = "#1,234";
        PointsValue = "12,345";
        TruePointsValue = "45,678";
        RatioValue = "3.70";
    }

    private void NotifyFontPropertiesChanged()
    {
        OnPropertiesChanged(nameof(LabelFontFamily), nameof(ValueFontFamily));
    }

    private void NotifyColorPropertiesChanged()
    {
        OnPropertiesChanged(
            nameof(LabelColor), nameof(LabelStrokeColor), nameof(LabelStrokeSize), nameof(LabelStrokeEnabled),
            nameof(ValueColor), nameof(ValueStrokeColor), nameof(ValueStrokeSize), nameof(ValueStrokeEnabled));
    }

    #endregion
}
