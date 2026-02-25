# WinForms to WPF Migration Plan (Historical)

## Retro Achievement Tracker - Native Animation Migration

This is the original planning document for the WinForms + WebView2 to WPF migration.
The repository now runs on the WPF implementation, so this file is retained for historical context.

---

## Executive Summary

### Current Architecture
- **UI Framework**: Windows Forms (.NET 8.0-windows)
- **Overlay Rendering**: WebView2 (Chromium) with HTML/CSS/JS
- **Animations**: jQuery/JavaScript in HTML templates
- **Pattern**: Singleton Controllers + Forms

### Target Architecture
- **UI Framework**: WPF (.NET 8.0-windows)
- **Overlay Rendering**: Native WPF windows with XAML
- **Animations**: WPF Storyboards + Transforms (GPU accelerated)
- **Pattern**: MVVM (Model-View-ViewModel)

### Benefits of Migration
| Aspect | Current (WebView2) | Target (WPF Native) |
|--------|-------------------|---------------------|
| Memory Usage | ~150-300MB (Chromium per window) | ~30-50MB total |
| Startup Time | 2-4 seconds (WebView2 init) | <1 second |
| Animation Performance | Good (JS-based) | Excellent (GPU) |
| Complexity | HTML + CSS + JS + C# | XAML + C# |
| Debugging | Browser DevTools + VS | Visual Studio only |
| Click-through Overlays | Requires workarounds | Native support |

---

## Phase 1: Project Setup & Shared Code Extraction (Week 1)

### 1.1 Create New WPF Project

```
Solution/
??? Retro Achievement Tracker/           # Existing WinForms (keep for reference)
??? Retro Achievement Tracker.Core/      # NEW: Shared business logic
??? Retro Achievement Tracker.WPF/       # NEW: WPF application
??? Retro Achievement Tracker.Tests/     # Existing tests (update references)
```

**Tasks:**
- [ ] Create `Retro Achievement Tracker.Core` class library project (.NET 8.0)
- [ ] Create `Retro Achievement Tracker.WPF` WPF application project (.NET 8.0-windows)
- [ ] Set up project references

### 1.2 Extract Shared Code to Core Library

Move these components to the Core library (they have no UI dependencies):

| Component | Source Location | Notes |
|-----------|-----------------|-------|
| `Achievement.cs` | Models/ | Data model |
| `GameInfo.cs` | Models/ | Data model |
| `UserSummary.cs` | Models/ | Data model |
| `UserRankAndScore.cs` | Models/ | Data model |
| `NotificationRequest.cs` | Models/ | Data model |
| `Constants.cs` | Models/ | API URLs |
| `*Converter.cs` | Models/ | JSON converters |
| `RetroAchievementAPIClient.cs` | Http/ | API client |
| `AchievementTrackingService.cs` | Services/ | Business logic |
| `StreamLabelController.cs` | Controllers/ | File I/O only |
| `CredentialProtector.cs` | Services/ | Security |
| `SettingsService.cs` | Services/ | Settings abstraction |

**Tasks:**
- [ ] Create Core project with models and services
- [ ] Create `ISettingsProvider` interface for settings abstraction
- [ ] Move API client and converters
- [ ] Update namespaces
- [ ] Ensure tests still pass

### 1.3 WPF Project Structure

```
Retro Achievement Tracker.WPF/
??? App.xaml                           # Application resources, themes
??? App.xaml.cs                        # Startup, DI container
??? MainWindow.xaml                    # Main control panel
??? MainWindow.xaml.cs
?
??? Views/                             # XAML views
?   ??? Overlays/
?   ?   ??? FocusOverlay.xaml          # Achievement focus display
?   ?   ??? AlertOverlay.xaml          # Notification popups
?   ?   ??? UserInfoOverlay.xaml       # User stats
?   ?   ??? GameInfoOverlay.xaml       # Game details
?   ?   ??? GameProgressOverlay.xaml   # Progress stats
?   ?   ??? RecentUnlocksOverlay.xaml  # Recent achievements
?   ?   ??? AchievementListOverlay.xaml# Full list
?   ?   ??? RelatedMediaOverlay.xaml   # Box art/screenshots
?   ??? Controls/
?       ??? AchievementCard.xaml       # Reusable achievement display
?       ??? AnimatedProgressBar.xaml   # Custom progress bar
?       ??? OutlinedTextBlock.xaml     # Text with stroke effect
?
??? ViewModels/                        # MVVM ViewModels
?   ??? MainViewModel.cs
?   ??? FocusViewModel.cs
?   ??? AlertsViewModel.cs
?   ??? UserInfoViewModel.cs
?   ??? GameInfoViewModel.cs
?   ??? GameProgressViewModel.cs
?   ??? RecentUnlocksViewModel.cs
?   ??? AchievementListViewModel.cs
?   ??? RelatedMediaViewModel.cs
?
??? Services/                          # WPF-specific services
?   ??? WindowService.cs               # Window management
?   ??? NotificationService.cs         # Toast/overlay coordination
?   ??? WpfSettingsProvider.cs         # Settings.Default wrapper
?
??? Converters/                        # XAML value converters
?   ??? ColorToSolidBrushConverter.cs
?   ??? BoolToVisibilityConverter.cs
?   ??? StringToFontFamilyConverter.cs
?
??? Animations/                        # Animation resources
?   ??? SlideAnimations.xaml           # Slide in/out storyboards
?   ??? FadeAnimations.xaml            # Fade effects
?   ??? BounceAnimations.xaml          # Bounce/elastic effects
?
??? Themes/                            # Styling
?   ??? Colors.xaml                    # Color palette
?   ??? Fonts.xaml                     # Typography
?   ??? Controls.xaml                  # Control templates
?
??? Resources/
?   ??? Images/                        # Static images
?
??? Properties/
    ??? Settings.settings              # User settings
```

---

## Phase 2: Core Infrastructure (Week 2)

### 2.1 MVVM Foundation

**Base ViewModel Class:**
```csharp
// ViewModels/ViewModelBase.cs
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
```

**Relay Command:**
```csharp
// ViewModels/RelayCommand.cs
public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Predicate<object?>? _canExecute;
    
    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }
    
    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
    public void Execute(object? parameter) => _execute(parameter);
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}
```

### 2.2 Outlined Text Control (Replace CSS text-stroke)

```xml
<!-- Views/Controls/OutlinedTextBlock.xaml -->
<UserControl x:Class="RetroAchievementTracker.WPF.Views.Controls.OutlinedTextBlock">
    <Grid>
        <!-- Stroke layer (rendered 8 times offset) -->
        <TextBlock x:Name="StrokeText" 
                   Text="{Binding Text, RelativeSource={RelativeSource AncestorType=UserControl}}"
                   FontFamily="{Binding FontFamily, RelativeSource={RelativeSource AncestorType=UserControl}}"
                   FontSize="{Binding FontSize, RelativeSource={RelativeSource AncestorType=UserControl}}"
                   Foreground="{Binding StrokeColor, RelativeSource={RelativeSource AncestorType=UserControl}}">
            <TextBlock.Effect>
                <BlurEffect Radius="2"/>
            </TextBlock.Effect>
        </TextBlock>
        
        <!-- Main text layer -->
        <TextBlock Text="{Binding Text, RelativeSource={RelativeSource AncestorType=UserControl}}"
                   FontFamily="{Binding FontFamily, RelativeSource={RelativeSource AncestorType=UserControl}}"
                   FontSize="{Binding FontSize, RelativeSource={RelativeSource AncestorType=UserControl}}"
                   Foreground="{Binding TextColor, RelativeSource={RelativeSource AncestorType=UserControl}}"/>
    </Grid>
</UserControl>
```

### 2.3 Animation Library

```xml
<!-- Animations/SlideAnimations.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <!-- Slide In From Top -->
    <Storyboard x:Key="SlideInFromTop">
        <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(TranslateTransform.Y)"
                         From="-200" To="0" Duration="0:0:0.5">
            <DoubleAnimation.EasingFunction>
                <CubicEase EasingMode="EaseOut"/>
            </DoubleAnimation.EasingFunction>
        </DoubleAnimation>
        <DoubleAnimation Storyboard.TargetProperty="Opacity"
                         From="0" To="1" Duration="0:0:0.3"/>
    </Storyboard>
    
    <!-- Slide Out To Top -->
    <Storyboard x:Key="SlideOutToTop">
        <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(TranslateTransform.Y)"
                         From="0" To="-200" Duration="0:0:0.5">
            <DoubleAnimation.EasingFunction>
                <CubicEase EasingMode="EaseIn"/>
            </DoubleAnimation.EasingFunction>
        </DoubleAnimation>
        <DoubleAnimation Storyboard.TargetProperty="Opacity"
                         From="1" To="0" Duration="0:0:0.3" BeginTime="0:0:0.2"/>
    </Storyboard>
    
    <!-- Slide In From Left -->
    <Storyboard x:Key="SlideInFromLeft">
        <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(TranslateTransform.X)"
                         From="-300" To="0" Duration="0:0:0.6">
            <DoubleAnimation.EasingFunction>
                <BackEase EasingMode="EaseOut" Amplitude="0.3"/>
            </DoubleAnimation.EasingFunction>
        </DoubleAnimation>
        <DoubleAnimation Storyboard.TargetProperty="Opacity"
                         From="0" To="1" Duration="0:0:0.3"/>
    </Storyboard>
    
    <!-- Slide In From Right -->
    <Storyboard x:Key="SlideInFromRight">
        <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(TranslateTransform.X)"
                         From="300" To="0" Duration="0:0:0.6">
            <DoubleAnimation.EasingFunction>
                <BackEase EasingMode="EaseOut" Amplitude="0.3"/>
            </DoubleAnimation.EasingFunction>
        </DoubleAnimation>
        <DoubleAnimation Storyboard.TargetProperty="Opacity"
                         From="0" To="1" Duration="0:0:0.3"/>
    </Storyboard>
    
    <!-- Bounce In -->
    <Storyboard x:Key="BounceIn">
        <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleX)"
                         From="0" To="1" Duration="0:0:0.5">
            <DoubleAnimation.EasingFunction>
                <ElasticEase EasingMode="EaseOut" Oscillations="1" Springiness="5"/>
            </DoubleAnimation.EasingFunction>
        </DoubleAnimation>
        <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleY)"
                         From="0" To="1" Duration="0:0:0.5">
            <DoubleAnimation.EasingFunction>
                <ElasticEase EasingMode="EaseOut" Oscillations="1" Springiness="5"/>
            </DoubleAnimation.EasingFunction>
        </DoubleAnimation>
    </Storyboard>
    
</ResourceDictionary>
```

---

## Phase 3: Overlay Windows Migration (Weeks 3-4)

### 3.1 Focus Overlay (Priority: High)

**Current Implementation:**
- `FocusWindow.cs` (WinForms) + `focus-window.html` (WebView2)
- `FocusController.cs` (Singleton)
- jQuery animations for slide-in effects

**WPF Implementation:**

```xml
<!-- Views/Overlays/FocusOverlay.xaml -->
<Window x:Class="RetroAchievementTracker.WPF.Views.Overlays.FocusOverlay"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:controls="clr-namespace:RetroAchievementTracker.WPF.Views.Controls"
        Title="Focus" 
        Width="700" Height="165"
        WindowStyle="None" 
        AllowsTransparency="True"
        Background="Transparent"
        Topmost="True"
        ShowInTaskbar="False">
    
    <Window.Resources>
        <Storyboard x:Key="ShowFocus">
            <!-- Badge slides in from left -->
            <DoubleAnimation Storyboard.TargetName="Badge" 
                             Storyboard.TargetProperty="(UIElement.RenderTransform).(TranslateTransform.X)"
                             From="-200" To="0" Duration="0:0:0.7">
                <DoubleAnimation.EasingFunction>
                    <CubicEase EasingMode="EaseOut"/>
                </DoubleAnimation.EasingFunction>
            </DoubleAnimation>
            
            <!-- Title slides in from right (delayed) -->
            <DoubleAnimation Storyboard.TargetName="Title" 
                             Storyboard.TargetProperty="(UIElement.RenderTransform).(TranslateTransform.X)"
                             From="300" To="0" Duration="0:0:0.7" BeginTime="0:0:0.4">
                <DoubleAnimation.EasingFunction>
                    <CubicEase EasingMode="EaseOut"/>
                </DoubleAnimation.EasingFunction>
            </DoubleAnimation>
            
            <!-- Line slides in from right (delayed more) -->
            <DoubleAnimation Storyboard.TargetName="Line" 
                             Storyboard.TargetProperty="(UIElement.RenderTransform).(TranslateTransform.X)"
                             From="300" To="0" Duration="0:0:0.7" BeginTime="0:0:0.6">
                <DoubleAnimation.EasingFunction>
                    <CubicEase EasingMode="EaseOut"/>
                </DoubleAnimation.EasingFunction>
            </DoubleAnimation>
            
            <!-- Description slides in from right (delayed even more) -->
            <DoubleAnimation Storyboard.TargetName="Description" 
                             Storyboard.TargetProperty="(UIElement.RenderTransform).(TranslateTransform.X)"
                             From="300" To="0" Duration="0:0:0.7" BeginTime="0:0:0.8">
                <DoubleAnimation.EasingFunction>
                    <CubicEase EasingMode="EaseOut"/>
                </DoubleAnimation.EasingFunction>
            </DoubleAnimation>
            
            <!-- Points slides in from left -->
            <DoubleAnimation Storyboard.TargetName="Points" 
                             Storyboard.TargetProperty="(UIElement.RenderTransform).(TranslateTransform.X)"
                             From="-100" To="0" Duration="0:0:0.7" BeginTime="0:0:0.2">
                <DoubleAnimation.EasingFunction>
                    <CubicEase EasingMode="EaseOut"/>
                </DoubleAnimation.EasingFunction>
            </DoubleAnimation>
        </Storyboard>
    </Window.Resources>
    
    <Border x:Name="FocusContainer" 
            Background="{Binding BackgroundColor}" 
            CornerRadius="5">
        
        <!-- Optional border image -->
        <Border.Style>
            <Style TargetType="Border">
                <Style.Triggers>
                    <DataTrigger Binding="{Binding BorderEnabled}" Value="True">
                        <Setter Property="BorderThickness" Value="2"/>
                        <Setter Property="BorderBrush" Value="{Binding BorderColor}"/>
                    </DataTrigger>
                </Style.Triggers>
            </Style>
        </Border.Style>
        
        <Grid Margin="10">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="140"/>  <!-- Badge -->
                <ColumnDefinition Width="*"/>    <!-- Content -->
            </Grid.ColumnDefinitions>
            
            <!-- Achievement Badge -->
            <Image x:Name="Badge" 
                   Grid.Column="0"
                   Source="{Binding BadgeUri}"
                   Width="140" Height="140"
                   RenderTransformOrigin="0.5,0.5">
                <Image.RenderTransform>
                    <TranslateTransform/>
                </Image.RenderTransform>
            </Image>
            
            <!-- Content Area -->
            <Grid Grid.Column="1" Margin="10,0,0,0">
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>  <!-- Title -->
                    <RowDefinition Height="5"/>     <!-- Line -->
                    <RowDefinition Height="*"/>     <!-- Description -->
                </Grid.RowDefinitions>
                
                <!-- Title -->
                <controls:OutlinedTextBlock x:Name="Title"
                                            Grid.Row="0"
                                            Text="{Binding Title}"
                                            FontFamily="{Binding TitleFontFamily}"
                                            FontSize="24"
                                            TextColor="{Binding TitleColor}"
                                            StrokeColor="{Binding TitleStrokeColor}"
                                            StrokeEnabled="{Binding TitleStrokeEnabled}"
                                            RenderTransformOrigin="0.5,0.5">
                    <controls:OutlinedTextBlock.RenderTransform>
                        <TranslateTransform/>
                    </controls:OutlinedTextBlock.RenderTransform>
                </controls:OutlinedTextBlock>
                
                <!-- Separator Line -->
                <Rectangle x:Name="Line"
                           Grid.Row="1"
                           Height="3"
                           Fill="{Binding LineColor}"
                           Margin="0,5"
                           RenderTransformOrigin="0.5,0.5">
                    <Rectangle.RenderTransform>
                        <TranslateTransform/>
                    </Rectangle.RenderTransform>
                </Rectangle>
                
                <!-- Description -->
                <Grid Grid.Row="2">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="Auto"/>
                        <ColumnDefinition Width="*"/>
                    </Grid.ColumnDefinitions>
                    
                    <!-- Points -->
                    <controls:OutlinedTextBlock x:Name="Points"
                                                Grid.Column="0"
                                                Text="{Binding Points}"
                                                FontFamily="{Binding PointsFontFamily}"
                                                FontSize="32"
                                                TextColor="{Binding PointsColor}"
                                                StrokeColor="{Binding PointsStrokeColor}"
                                                VerticalAlignment="Bottom"
                                                RenderTransformOrigin="0.5,0.5">
                        <controls:OutlinedTextBlock.RenderTransform>
                            <TranslateTransform/>
                        </controls:OutlinedTextBlock.RenderTransform>
                    </controls:OutlinedTextBlock>
                    
                    <!-- Description Text -->
                    <controls:OutlinedTextBlock x:Name="Description"
                                                Grid.Column="1"
                                                Text="{Binding Description}"
                                                FontFamily="{Binding DescriptionFontFamily}"
                                                FontSize="16"
                                                TextColor="{Binding DescriptionColor}"
                                                StrokeColor="{Binding DescriptionStrokeColor}"
                                                TextWrapping="Wrap"
                                                VerticalAlignment="Center"
                                                Margin="10,0,0,0"
                                                RenderTransformOrigin="0.5,0.5">
                        <controls:OutlinedTextBlock.RenderTransform>
                            <TranslateTransform/>
                        </controls:OutlinedTextBlock.RenderTransform>
                    </controls:OutlinedTextBlock>
                </Grid>
            </Grid>
        </Grid>
    </Border>
</Window>
```

**ViewModel:**
```csharp
// ViewModels/FocusViewModel.cs
public class FocusViewModel : ViewModelBase
{
    private readonly ISettingsProvider _settings;
    
    private string _title = string.Empty;
    private string _description = string.Empty;
    private string _points = string.Empty;
    private string _badgeUri = string.Empty;
    
    public string Title { get => _title; set => SetProperty(ref _title, value); }
    public string Description { get => _description; set => SetProperty(ref _description, value); }
    public string Points { get => _points; set => SetProperty(ref _points, value); }
    public string BadgeUri { get => _badgeUri; set => SetProperty(ref _badgeUri, value); }
    
    // Font properties
    public FontFamily TitleFontFamily => new FontFamily(_settings.FocusTitleFontFamily);
    public FontFamily DescriptionFontFamily => new FontFamily(_settings.FocusDescriptionFontFamily);
    public FontFamily PointsFontFamily => new FontFamily(_settings.FocusPointsFontFamily);
    
    // Color properties (convert hex to Brush)
    public SolidColorBrush TitleColor => HexToBrush(_settings.FocusTitleColor);
    public SolidColorBrush DescriptionColor => HexToBrush(_settings.FocusDescriptionColor);
    public SolidColorBrush PointsColor => HexToBrush(_settings.FocusPointsColor);
    public SolidColorBrush LineColor => HexToBrush(_settings.FocusLineColor);
    public SolidColorBrush BackgroundColor => HexToBrush(_settings.FocusBackgroundColor);
    
    // Stroke properties
    public bool TitleStrokeEnabled => _settings.FocusTitleOutlineEnabled;
    public SolidColorBrush TitleStrokeColor => HexToBrush(_settings.FocusTitleOutlineColor);
    
    public void SetAchievement(Achievement achievement)
    {
        Title = achievement.Title;
        Description = achievement.Description;
        Points = achievement.Points.ToString();
        BadgeUri = achievement.BadgeUri;
    }
    
    public void SetGameInfo(GameInfo gameInfo)
    {
        Title = gameInfo.Title;
        Description = $"Cheevos: {gameInfo.AchievementsEarned}\nPoints: {gameInfo.GamePointsPossible}";
        Points = string.Empty;
        BadgeUri = gameInfo.BadgeUri;
    }
    
    private static SolidColorBrush HexToBrush(string hex)
    {
        var color = (Color)ColorConverter.ConvertFromString(hex);
        return new SolidColorBrush(color);
    }
}
```

### 3.2 Alerts Overlay (Priority: High)

The alerts overlay is more complex due to the video playback and notification queue.

**Key Changes:**
- Replace `<video>` tags with `MediaElement` for video playback
- Use WPF animation system for in/out animations
- Maintain notification queue logic

```xml
<!-- Views/Overlays/AlertOverlay.xaml (simplified) -->
<Window x:Class="RetroAchievementTracker.WPF.Views.Overlays.AlertOverlay"
        WindowStyle="None" AllowsTransparency="True" Background="Transparent"
        Width="1028" Height="600" Topmost="True">
    
    <Grid>
        <!-- Video Background -->
        <MediaElement x:Name="NotificationVideo"
                      LoadedBehavior="Manual"
                      UnloadedBehavior="Manual"
                      MediaEnded="NotificationVideo_MediaEnded"/>
        
        <!-- Achievement Content Overlay -->
        <Grid x:Name="AchievementContent" 
              Visibility="Collapsed"
              RenderTransformOrigin="0.5,0.5">
            <Grid.RenderTransform>
                <TransformGroup>
                    <TranslateTransform x:Name="ContentTranslate"/>
                    <ScaleTransform x:Name="ContentScale"/>
                </TransformGroup>
            </Grid.RenderTransform>
            
            <!-- Badge + Title + Description layout -->
            <!-- Similar to FocusOverlay -->
        </Grid>
    </Grid>
</Window>
```

### 3.3 Migration Order for All Overlays

| Priority | Overlay | Complexity | Dependencies |
|----------|---------|------------|--------------|
| 1 | Focus | Medium | OutlinedTextBlock |
| 2 | Alerts | High | MediaElement, Animation queue |
| 3 | User Info | Low | Data binding only |
| 4 | Game Info | Low | Data binding only |
| 5 | Game Progress | Low | ProgressBar styling |
| 6 | Recent Unlocks | Medium | ItemsControl, animations |
| 7 | Achievement List | Medium | ListView virtualization |
| 8 | Related Media | Low | Image display |

---

## Phase 4: Main Window Migration (Week 5)

### 4.1 Main Window Structure

The main window is the control panel with tabs for each overlay's settings.

```xml
<!-- MainWindow.xaml -->
<Window x:Class="RetroAchievementTracker.WPF.MainWindow"
        Title="Retro Achievement Tracker" 
        Width="800" Height="600">
    
    <DockPanel>
        <!-- Header with Start/Stop -->
        <StackPanel DockPanel.Dock="Top" Orientation="Horizontal" Margin="10">
            <TextBox Text="{Binding Username}" Width="150" Margin="0,0,10,0"/>
            <PasswordBox x:Name="ApiKeyBox" Width="200" Margin="0,0,10,0"/>
            <Button Content="Start" Command="{Binding StartCommand}" Width="80"/>
            <Button Content="Stop" Command="{Binding StopCommand}" Width="80" Margin="10,0,0,0"/>
        </StackPanel>
        
        <!-- Status -->
        <StatusBar DockPanel.Dock="Bottom">
            <TextBlock Text="{Binding StatusMessage}"/>
        </StatusBar>
        
        <!-- Tab Control for Settings -->
        <TabControl>
            <TabItem Header="Focus">
                <views:FocusSettingsView DataContext="{Binding FocusSettings}"/>
            </TabItem>
            <TabItem Header="Alerts">
                <views:AlertsSettingsView DataContext="{Binding AlertsSettings}"/>
            </TabItem>
            <TabItem Header="User Info">
                <views:UserInfoSettingsView DataContext="{Binding UserInfoSettings}"/>
            </TabItem>
            <!-- Additional tabs... -->
        </TabControl>
    </DockPanel>
</Window>
```

---

## Phase 5: Testing & Polish (Week 6)

### 5.1 Testing Checklist

- [ ] All overlays display correctly
- [ ] Animations match original timing and style
- [ ] Settings persist and load correctly
- [ ] Stream labels still work
- [ ] API polling functions correctly
- [ ] Memory usage is reduced
- [ ] Startup time is improved
- [ ] Click-through works on overlays
- [ ] Multiple monitor support

### 5.2 Performance Benchmarks

| Metric | Target | How to Measure |
|--------|--------|----------------|
| Memory (idle) | <100MB | Task Manager |
| Memory (all overlays) | <150MB | Task Manager |
| Startup time | <2 seconds | Stopwatch |
| Animation FPS | 60 FPS | Visual inspection |
| CPU (idle) | <1% | Task Manager |

---

## Phase 6: Deployment & Cleanup (Week 7)

### 6.1 Deployment Tasks

- [ ] Update installer project for WPF
- [ ] Update auto-updater configuration
- [ ] Create migration guide for users
- [ ] Archive WinForms project

### 6.2 Documentation Updates

- [ ] Update README.md
- [ ] Update copilot-instructions.md for WPF architecture
- [ ] Create new architecture diagram

---

## Risk Mitigation

| Risk | Mitigation |
|------|------------|
| Custom video files not working | Test MediaElement with various codecs; provide codec pack recommendation |
| Text rendering differences | Fine-tune OutlinedTextBlock; consider FormattedText for exact control |
| Settings migration | Create settings converter; maintain backward compatibility |
| Animation timing differences | Allow animation speed customization; A/B test with users |

---

## Estimated Timeline

| Phase | Duration | Deliverable |
|-------|----------|-------------|
| Phase 1: Setup | 1 week | Core library + empty WPF project |
| Phase 2: Infrastructure | 1 week | MVVM base + animation library |
| Phase 3: Overlays | 2 weeks | All 8 overlay windows |
| Phase 4: Main Window | 1 week | Control panel with settings |
| Phase 5: Testing | 1 week | Tested, polished application |
| Phase 6: Deployment | 1 week | Released WPF version |

**Total: 7 weeks**

---

## Getting Started

To begin the migration, I recommend starting with:

1. **Create the Core library** - Extract shared code
2. **Create the WPF project** - Set up project structure
3. **Build the FocusOverlay** - Simplest overlay with all animation patterns
4. **Iterate** - Apply patterns learned to other overlays

Would you like me to start implementing any of these phases?
