using System.Windows;
using System.Windows.Media;
using RATracker.Models;

namespace RATracker.WPF.ViewModels;

/// <summary>
/// ViewModel for the Game Info Overlay window.
/// Displays game title, console, developer, publisher, genre, and release date.
/// </summary>
public class GameInfoViewModel : ViewModelBase
{
    #region Private Fields

    // Content fields
    private string _titleLabel = "Title:";
    private string _titleValue = "Super Mario Bros.";
    private string _consoleLabel = "Console:";
    private string _consoleValue = "Nintendo Entertainment System";
    private string _developerLabel = "Developer:";
    private string _developerValue = "Nintendo";
    private string _publisherLabel = "Publisher:";
    private string _publisherValue = "Nintendo";
    private string _genreLabel = "Genre:";
    private string _genreValue = "Platformer";
    private string _releasedLabel = "Released:";
    private string _releasedValue = "1985";
    private string _badgeUri = string.Empty;

    // Visibility
    private bool _showBadge = true;
    private bool _showTitle = true;
    private bool _showConsole = true;
    private bool _showDeveloper = true;
    private bool _showPublisher = true;
    private bool _showGenre = true;
    private bool _showReleased = true;

    // Layout settings
    private double _windowWidth = 500;
    private double _windowHeight = 320;
    private double _containerCornerRadius = 8;
    private double _containerMargin = 5;
    private double _rowSpacing = 6;
    private double _badgeSize = 96;
    private double _badgeCornerRadius = 5;

    // Font sizes
    private double _labelFontSize = 16;
    private double _valueFontSize = 18;
    private double _titleValueFontSize = 22;

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

    public string TitleLabel
    {
        get => _titleLabel;
        set => SetProperty(ref _titleLabel, value);
    }

    public string TitleValue
    {
        get => _titleValue;
        set => SetProperty(ref _titleValue, value);
    }

    public string ConsoleLabel
    {
        get => _consoleLabel;
        set => SetProperty(ref _consoleLabel, value);
    }

    public string ConsoleValue
    {
        get => _consoleValue;
        set => SetProperty(ref _consoleValue, value);
    }

    public string DeveloperLabel
    {
        get => _developerLabel;
        set => SetProperty(ref _developerLabel, value);
    }

    public string DeveloperValue
    {
        get => _developerValue;
        set => SetProperty(ref _developerValue, value);
    }

    public string PublisherLabel
    {
        get => _publisherLabel;
        set => SetProperty(ref _publisherLabel, value);
    }

    public string PublisherValue
    {
        get => _publisherValue;
        set => SetProperty(ref _publisherValue, value);
    }

    public string GenreLabel
    {
        get => _genreLabel;
        set => SetProperty(ref _genreLabel, value);
    }

    public string GenreValue
    {
        get => _genreValue;
        set => SetProperty(ref _genreValue, value);
    }

    public string ReleasedLabel
    {
        get => _releasedLabel;
        set => SetProperty(ref _releasedLabel, value);
    }

    public string ReleasedValue
    {
        get => _releasedValue;
        set => SetProperty(ref _releasedValue, value);
    }

    public string BadgeUri
    {
        get => _badgeUri;
        set => SetProperty(ref _badgeUri, value);
    }

    #endregion

    #region Visibility Properties

    public bool ShowBadge
    {
        get => _showBadge;
        set => SetProperty(ref _showBadge, value);
    }

    public bool ShowTitle
    {
        get => _showTitle;
        set => SetProperty(ref _showTitle, value);
    }

    public bool ShowConsole
    {
        get => _showConsole;
        set => SetProperty(ref _showConsole, value);
    }

    public bool ShowDeveloper
    {
        get => _showDeveloper;
        set => SetProperty(ref _showDeveloper, value);
    }

    public bool ShowPublisher
    {
        get => _showPublisher;
        set => SetProperty(ref _showPublisher, value);
    }

    public bool ShowGenre
    {
        get => _showGenre;
        set => SetProperty(ref _showGenre, value);
    }

    public bool ShowReleased
    {
        get => _showReleased;
        set => SetProperty(ref _showReleased, value);
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

    public double BadgeSize
    {
        get => _badgeSize;
        set => SetProperty(ref _badgeSize, value);
    }

    public double BadgeCornerRadius
    {
        get => _badgeCornerRadius;
        set => SetProperty(ref _badgeCornerRadius, value);
    }

    public CornerRadius BadgeCornerRadiusValue => new(_badgeCornerRadius);

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

    public double TitleValueFontSize
    {
        get => _titleValueFontSize;
        set => SetProperty(ref _titleValueFontSize, value);
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
    /// Updates the game info display from a GameInfo object.
    /// </summary>
    public void SetGameInfo(GameInfo gameInfo)
    {
        TitleValue = FormatTitle(gameInfo.Title);
        ConsoleValue = gameInfo.ConsoleName;
        DeveloperValue = gameInfo.Developer;
        PublisherValue = gameInfo.Publisher;
        GenreValue = gameInfo.Genre;
        ReleasedValue = gameInfo.Released;
        BadgeUri = gameInfo.BadgeUri;
    }

    /// <summary>
    /// Sets sample data for demo/design purposes.
    /// </summary>
    public void SetSampleData()
    {
        TitleValue = "Super Mario Bros.";
        ConsoleValue = "Nintendo Entertainment System";
        DeveloperValue = "Nintendo";
        PublisherValue = "Nintendo";
        GenreValue = "Platformer";
        ReleasedValue = "1985";
        BadgeUri = "https://media.retroachievements.org/Images/000001.png";
    }

    /// <summary>
    /// Formats the title, handling ", The" suffix.
    /// </summary>
    private static string FormatTitle(string title)
    {
        if (title.Contains(", The"))
        {
            var index = title.IndexOf(", The");
            title = "The " + title[..index] + title[(index + 5)..];
        }
        return title;
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
