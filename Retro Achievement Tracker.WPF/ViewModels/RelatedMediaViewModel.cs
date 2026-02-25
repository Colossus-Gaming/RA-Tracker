using System.Windows;
using System.Windows.Media;
using RATracker.Models;

namespace RATracker.WPF.ViewModels;

/// <summary>
/// Media source options for the Related Media overlay.
/// </summary>
public enum MediaSource
{
    RetroAchievements,
    LaunchBox
}

/// <summary>
/// ViewModel for the Related Media Overlay window.
/// Displays game box art, screenshots, and title screens.
/// </summary>
public class RelatedMediaViewModel : ViewModelBase
{
    #region Private Fields

    private string _imageUri = string.Empty;
    private MediaSource _mediaSource = MediaSource.RetroAchievements;
    private string _launchBoxPath = string.Empty;
    private bool _isImageVisible = true;

    // Layout settings
    private double _windowWidth = 640;
    private double _windowHeight = 480;
    private double _containerCornerRadius = 8;
    private double _containerMargin = 5;
    private double _imageCornerRadius = 5;

    // Window settings
    private Brush _backgroundColor = new SolidColorBrush(Color.FromArgb(200, 50, 50, 50));
    private Brush _borderColor = Brushes.Gold;
    private bool _borderEnabled = true;

    // Cached paths for LaunchBox media
    private string _boxArtPath = string.Empty;
    private string _titleScreenPath = string.Empty;
    private string _screenshotPath = string.Empty;

    #endregion

    #region Content Properties

    public string ImageUri
    {
        get => _imageUri;
        set => SetProperty(ref _imageUri, value);
    }

    public MediaSource MediaSource
    {
        get => _mediaSource;
        set => SetProperty(ref _mediaSource, value);
    }

    public string LaunchBoxPath
    {
        get => _launchBoxPath;
        set => SetProperty(ref _launchBoxPath, value);
    }

    public bool IsImageVisible
    {
        get => _isImageVisible;
        set => SetProperty(ref _isImageVisible, value);
    }

    public string BoxArtPath
    {
        get => _boxArtPath;
        set => SetProperty(ref _boxArtPath, value);
    }

    public string TitleScreenPath
    {
        get => _titleScreenPath;
        set => SetProperty(ref _titleScreenPath, value);
    }

    public string ScreenshotPath
    {
        get => _screenshotPath;
        set => SetProperty(ref _screenshotPath, value);
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

    public double ImageCornerRadius
    {
        get => _imageCornerRadius;
        set => SetProperty(ref _imageCornerRadius, value);
    }

    public CornerRadius ImageCornerRadiusValue => new(_imageCornerRadius);

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

    #endregion

    #region Methods

    /// <summary>
    /// Sets the image from RetroAchievements game info.
    /// </summary>
    public void SetGameImage(GameInfo gameInfo)
    {
        if (MediaSource == MediaSource.RetroAchievements)
        {
            // Use RA box art
            ImageUri = gameInfo.ImageBoxArt;
        }
        else if (!string.IsNullOrEmpty(BoxArtPath))
        {
            // Use LaunchBox box art if available
            ImageUri = BoxArtPath;
        }
        else
        {
            // Fallback to RA
            ImageUri = gameInfo.ImageBoxArt;
        }

        IsImageVisible = !string.IsNullOrEmpty(ImageUri);
    }

    /// <summary>
    /// Sets the image from a direct URI.
    /// </summary>
    public void SetImage(string uri)
    {
        ImageUri = uri;
        IsImageVisible = !string.IsNullOrEmpty(uri);
    }

    /// <summary>
    /// Shows the box art image.
    /// </summary>
    public void ShowBoxArt()
    {
        if (MediaSource == MediaSource.LaunchBox && !string.IsNullOrEmpty(BoxArtPath))
        {
            ImageUri = BoxArtPath;
        }
    }

    /// <summary>
    /// Shows the title screen image.
    /// </summary>
    public void ShowTitleScreen()
    {
        if (MediaSource == MediaSource.LaunchBox && !string.IsNullOrEmpty(TitleScreenPath))
        {
            ImageUri = TitleScreenPath;
        }
    }

    /// <summary>
    /// Shows a screenshot image.
    /// </summary>
    public void ShowScreenshot()
    {
        if (MediaSource == MediaSource.LaunchBox && !string.IsNullOrEmpty(ScreenshotPath))
        {
            ImageUri = ScreenshotPath;
        }
    }

    /// <summary>
    /// Hides the image.
    /// </summary>
    public void HideImage()
    {
        IsImageVisible = false;
    }

    /// <summary>
    /// Shows the image.
    /// </summary>
    public void ShowImage()
    {
        IsImageVisible = true;
    }

    /// <summary>
    /// Sets sample data for demo/design purposes.
    /// </summary>
    public void SetSampleData()
    {
        ImageUri = "https://media.retroachievements.org/Images/000001.png";
        IsImageVisible = true;
    }

    /// <summary>
    /// Clears all cached media paths.
    /// </summary>
    public void ClearMediaPaths()
    {
        BoxArtPath = string.Empty;
        TitleScreenPath = string.Empty;
        ScreenshotPath = string.Empty;
        ImageUri = string.Empty;
    }

    #endregion
}
