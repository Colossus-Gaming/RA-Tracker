using System.Windows;
using System.Windows.Media;
using RATracker.Models;

namespace RATracker.WPF.ViewModels;

/// <summary>
/// ViewModel for the Game Progress Overlay window.
/// Displays achievements earned, points earned, true points, ratio, and completion percentage.
/// </summary>
public class GameProgressViewModel : ViewModelBase
{
    #region Private Fields

    // Content fields
    private string _achievementsLabel = "Achievements:";
    private string _achievementsValue = "12 / 24";
    private string _pointsLabel = "Points:";
    private string _pointsValue = "200 / 400";
    private string _truePointsLabel = "True Points:";
    private string _truePointsValue = "500 / 1,000";
    private string _ratioLabel = "Ratio:";
    private string _ratioValue = "2.50";
    private string _completedLabel = "Completed:";
    private string _completedValue = "50.00%";
    private string _dividerCharacter = "/";

    // Visibility
    private bool _showAchievements = true;
    private bool _showPoints = true;
    private bool _showTruePoints = true;
    private bool _showRatio = true;
    private bool _showCompleted = true;

    // Layout settings
    private double _windowWidth = 400;
    private double _windowHeight = 260;
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

    // Cached game info for divider updates
    private GameInfo? _currentGameInfo;

    #endregion

    #region Content Properties

    public string AchievementsLabel
    {
        get => _achievementsLabel;
        set => SetProperty(ref _achievementsLabel, value);
    }

    public string AchievementsValue
    {
        get => _achievementsValue;
        set => SetProperty(ref _achievementsValue, value);
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

    public string CompletedLabel
    {
        get => _completedLabel;
        set => SetProperty(ref _completedLabel, value);
    }

    public string CompletedValue
    {
        get => _completedValue;
        set => SetProperty(ref _completedValue, value);
    }

    public string DividerCharacter
    {
        get => _dividerCharacter;
        set
        {
            if (SetProperty(ref _dividerCharacter, value) && _currentGameInfo != null)
            {
                UpdateProgressValues(_currentGameInfo);
            }
        }
    }

    #endregion

    #region Visibility Properties

    public bool ShowAchievements
    {
        get => _showAchievements;
        set => SetProperty(ref _showAchievements, value);
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

    public bool ShowCompleted
    {
        get => _showCompleted;
        set => SetProperty(ref _showCompleted, value);
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
    /// Updates the game progress display from a GameInfo object.
    /// </summary>
    public void SetGameProgress(GameInfo gameInfo)
    {
        _currentGameInfo = gameInfo;
        UpdateProgressValues(gameInfo);
    }

    private void UpdateProgressValues(GameInfo gameInfo)
    {
        AchievementsValue = $"{gameInfo.AchievementsEarned:N0} {_dividerCharacter} {gameInfo.AchievementsPossible:N0}";
        PointsValue = $"{gameInfo.GamePointsEarned:N0} {_dividerCharacter} {gameInfo.GamePointsPossible:N0}";
        TruePointsValue = $"{gameInfo.GameTruePointsEarned:N0} {_dividerCharacter} {gameInfo.GameTruePointsPossible:N0}";

        // Calculate ratio
        if (gameInfo.GamePointsEarned > 0)
        {
            var ratio = (float)gameInfo.GameTruePointsEarned / gameInfo.GamePointsEarned;
            RatioValue = ratio.ToString("0.00");
        }
        else
        {
            RatioValue = "0.00";
        }

        CompletedValue = $"{gameInfo.PercentComplete}%";
    }

    /// <summary>
    /// Sets sample data for demo/design purposes.
    /// </summary>
    public void SetSampleData()
    {
        AchievementsValue = $"12 {_dividerCharacter} 24";
        PointsValue = $"200 {_dividerCharacter} 400";
        TruePointsValue = $"500 {_dividerCharacter} 1,000";
        RatioValue = "2.50";
        CompletedValue = "50.00%";
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
