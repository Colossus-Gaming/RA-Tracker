# Plan: Separate Achievement Notifications for Sub-Sets

## Overview

Add the ability to have **different notification settings** for achievements from different achievement sets (Core vs Bonus vs Specialty vs Exclusive). This allows streamers to visually differentiate when they unlock a "bonus" achievement versus a "core" achievement.

---

## Current State

| Component | Status | Notes |
|-----------|--------|-------|
| `AchievementSet.cs` | Exists | Has `AchievementSetType` enum (Core, Bonus, Specialty, Exclusive) |
| `Achievement.cs` | Needs Update | Missing `SetType` property to track which set an achievement belongs to |
| `AlertsViewModel.cs` | Needs Update | Has Achievement/Mastery settings, needs Sub-Set settings |
| `AlertsOverlay.xaml/.cs` | Needs Update | Needs to detect set type and apply different settings |
| `MainViewModel.cs` | Needs Update | Needs to pass set type info with notifications |
| `AppSettings.cs` | Needs Update | Needs persistence for sub-set notification settings |
| `MainWindow.xaml` | Needs Update | Needs UI for configuring sub-set notifications |

---

## Implementation Phases

### Phase 1: Extend Achievement Model (Start Here)
**Files:** `Retro Achievement Tracker.WPF/Models/Achievement.cs`

**Changes:**
- Add `AchievementSetType SetType { get; set; }` property
- Add `long? SetId { get; set; }` property  
- Add `string? SetName { get; set; }` property (for display purposes)
- Add computed properties: `IsCore`, `IsBonus`, `IsSubSet`

**Estimated Effort:** Small

---

### Phase 2: Update AlertsViewModel for Sub-Set Notifications
**Files:** `Retro Achievement Tracker.WPF/ViewModels/AlertsViewModel.cs`

**Changes:**
- Add sub-set notification type indicator: `IsSubSetNotification`
- Add sub-set animation settings:
  - `SubSetInDirection`, `SubSetOutDirection`
- Add sub-set position settings:
  - `SubSetLeft`, `SubSetTop`
- Add sub-set custom video settings:
  - `CustomSubSetVideoPath`, `CustomSubSetEnabled`
- Add sub-set visual differentiation:
  - `SubSetBorderColor` (default: purple/different from core)
- Add method: `SetSubSetAchievementNotification(Achievement achievement)`
- Add method: `SetSampleSubSetNotification()` for testing

**Estimated Effort:** Medium

---

### Phase 3: Update AlertsOverlay for Sub-Set Handling
**Files:** `Retro Achievement Tracker.WPF/Views/AlertsOverlay.xaml`, `Retro Achievement Tracker.WPF/Views/AlertsOverlay.xaml.cs`

**Changes:**
- Add `NotificationType.SubSetAchievement` to the enum
- Add `QueueSubSetNotification(Achievement achievement)` method
- Update `ProcessQueue()` to:
  - Detect achievement set type
  - Use sub-set animation/position settings when appropriate
- Add visual indicator in XAML (conditional border color or badge)
- Add `ShowTestSubSetNotification()` method
- Consider adding optional "[BONUS]" or set name prefix to title

**Estimated Effort:** Medium

---

### Phase 4: Update MainViewModel Event Handling
**Files:** `Retro Achievement Tracker.WPF/ViewModels/MainViewModel.cs`

**Changes:**
- Add new event: `SubSetAchievementUnlocked`
- Add setting property: `EnableSeparateSubSetNotifications` (bool)
- Update `OnTrackingServiceAchievementsUnlocked()` to:
  - Check `achievement.SetType` or `achievement.IsSubSet` for each unlocked achievement
  - Fire `SubSetAchievementUnlocked` for non-core achievements (when enabled)
  - Fire `AchievementUnlocked` for core achievements
- Update stream label writing to include set name when applicable
- Add `TestSubSetAlertCommand` for testing

**Estimated Effort:** Medium

---

### Phase 5: Add AppSettings Persistence
**Files:** `Retro Achievement Tracker.WPF/Models/AppSettings.cs`

**Changes:**
- Add `EnableSubSetNotifications` (bool, default: false)
- Add sub-set animation settings:
  - `SubSetInDirection` (string, default: "Right")
  - `SubSetOutDirection` (string, default: "Left")
- Add sub-set position settings:
  - `SubSetNotificationLeft` (double)
  - `SubSetNotificationTop` (double)
- Add sub-set video settings:
  - `CustomSubSetVideoPath` (string)
  - `CustomSubSetVideoEnabled` (bool)

**Estimated Effort:** Small

---

### Phase 6: Wire Up MainWindow to AlertsOverlay
**Files:** `Retro Achievement Tracker.WPF/MainWindow.xaml.cs`

**Changes:**
- Subscribe to `SubSetAchievementUnlocked` event from MainViewModel
- Call `AlertsOverlay.QueueSubSetNotification()` when sub-set achievement unlocks
- Ensure proper event handler cleanup on window close

**Estimated Effort:** Small

---

### Phase 7: Add UI for Sub-Set Notification Settings
**Files:** `Retro Achievement Tracker.WPF/MainWindow.xaml`

**Changes:**
- Add new section in Settings/Alerts tab: "Sub-Set Notifications" or "Bonus Achievement Notifications"
- Add checkbox: "Enable separate notifications for bonus/specialty achievements"
- Add sub-set animation direction dropdowns (In/Out)
- Add sub-set position controls (X/Y)
- Add sub-set custom video file picker
- Add "Test Sub-Set Alert" button

**Estimated Effort:** Medium-Large

---

## Detailed File Changes

### Phase 1: `Achievement.cs`
```csharp
// Add these properties after existing properties:

/// <summary>
/// The type of achievement set this achievement belongs to.
/// Defaults to Core for backward compatibility.
/// </summary>
public AchievementSetType SetType { get; set; } = AchievementSetType.Core;

/// <summary>
/// The ID of the achievement set this achievement belongs to.
/// </summary>
public long? SetId { get; set; }

/// <summary>
/// The display name of the achievement set (e.g., "Bonus", "Specialty").
/// </summary>
public string? SetName { get; set; }

/// <summary>
/// Whether this achievement is from the core/base achievement set.
/// </summary>
public bool IsCore => SetType == AchievementSetType.Core;

/// <summary>
/// Whether this achievement is from a bonus achievement set.
/// </summary>
public bool IsBonus => SetType == AchievementSetType.Bonus;

/// <summary>
/// Whether this achievement is from any non-core set (bonus, specialty, exclusive).
/// </summary>
public bool IsSubSet => SetType != AchievementSetType.Core;
```

### Phase 2: `AlertsViewModel.cs` (New Properties)
```csharp
// Sub-set notification state
private bool _isSubSetNotification;
public bool IsSubSetNotification 
{ 
    get => _isSubSetNotification; 
    set => SetProperty(ref _isSubSetNotification, value); 
}

// Sub-set animation settings
private AnimationDirection _subSetInDirection = AnimationDirection.Right;
private AnimationDirection _subSetOutDirection = AnimationDirection.Left;

public AnimationDirection SubSetInDirection
{
    get => _subSetInDirection;
    set => SetProperty(ref _subSetInDirection, value);
}

public AnimationDirection SubSetOutDirection
{
    get => _subSetOutDirection;
    set => SetProperty(ref _subSetOutDirection, value);
}

// Sub-set position settings  
private double _subSetLeft = 150;
private double _subSetTop = 100;

public double SubSetLeft
{
    get => _subSetLeft;
    set => SetProperty(ref _subSetLeft, value);
}

public double SubSetTop
{
    get => _subSetTop;
    set => SetProperty(ref _subSetTop, value);
}

// Sub-set custom video
private string _customSubSetVideoPath = string.Empty;
private bool _customSubSetEnabled;

public string CustomSubSetVideoPath
{
    get => _customSubSetVideoPath;
    set => SetProperty(ref _customSubSetVideoPath, value);
}

public bool CustomSubSetEnabled
{
    get => _customSubSetEnabled;
    set => SetProperty(ref _customSubSetEnabled, value);
}

// Sub-set visual differentiation
private Brush _subSetBorderColor = Brushes.MediumPurple;

public Brush SubSetBorderColor
{
    get => _subSetBorderColor;
    set => SetProperty(ref _subSetBorderColor, value);
}

// Methods
public void SetSubSetAchievementNotification(Achievement achievement)
{
    IsSubSetNotification = true;
    IsMasteryNotification = false;
    Title = achievement.Title;
    Description = achievement.Description;
    Points = achievement.Points.ToString();
    BadgeUri = achievement.BadgeUri;
}

public void SetSampleSubSetNotification()
{
    IsSubSetNotification = true;
    IsMasteryNotification = false;
    Title = "[BONUS] Speed Run Master";
    Description = "Complete the game in under 30 minutes!";
    Points = "50";
    BadgeUri = "https://media.retroachievements.org/Badge/00001.png";
}
```

### Phase 3: `AlertsOverlay.xaml.cs` (New Enum Value & Method)
```csharp
private enum NotificationType
{
    Achievement,
    Mastery,
    SubSetAchievement  // NEW
}

public void QueueSubSetNotification(Achievement achievement)
{
    _notificationQueue.Enqueue(new NotificationItem
    {
        Type = NotificationType.SubSetAchievement,
        Achievement = achievement
    });
    ProcessQueue();
}

public async Task ShowTestSubSetNotification()
{
    ViewModel.SetSampleSubSetNotification();
    await PlayNotificationAnimation(false, true); // isMastery=false, isSubSet=true
}
```

---

## Testing Plan

| Test Case | Description |
|-----------|-------------|
| Core Achievement | Verify core achievements use standard notification settings |
| Bonus Achievement | Verify bonus achievements use sub-set notification settings (when enabled) |
| Feature Disabled | Verify all achievements use standard settings when sub-set notifications disabled |
| Animation Directions | Test all 5 animation directions work for sub-set notifications |
| Custom Video | Test custom video playback for sub-set notifications |
| Queue Processing | Verify mixed core/bonus achievements process in correct order |
| Settings Persistence | Verify sub-set settings save and load correctly |
| Backward Compatibility | Verify achievements without SetType default to Core behavior |

---

## Implementation Checklist

- [ ] **Phase 1** - Achievement.cs (foundation)
- [ ] **Phase 2** - AlertsViewModel.cs (settings/state)
- [ ] **Phase 3** - AlertsOverlay.xaml/.cs (notification handling)
- [ ] **Phase 4** - MainViewModel.cs (event routing)
- [ ] **Phase 5** - AppSettings.cs (persistence)
- [ ] **Phase 6** - MainWindow.xaml.cs (wiring)
- [ ] **Phase 7** - MainWindow.xaml (UI)

---

## Notes

- **Backward Compatibility**: Achievements without `SetType` default to `Core`
- **Opt-in Feature**: Sub-set notifications are disabled by default
- **Reuse Pattern**: We'll reuse the existing notification container with conditional styling rather than creating a completely separate container (reduces code duplication)
- **Visual Indicator**: Different border color is the primary visual differentiator; optional set name prefix is secondary
- **Requires AchievementSetType**: Must import/reference `AchievementSetType` enum from `AchievementSet.cs` in the Achievement model

---

## Prompts for Each Phase

### Phase 1 Prompt
```
Implement Phase 1 of the sub-set notifications plan (see docs/plans/subset-notifications-plan.md).

Update `Retro Achievement Tracker.WPF/Models/Achievement.cs` to add:
1. `AchievementSetType SetType` property (default: Core)
2. `long? SetId` property
3. `string? SetName` property
4. Computed properties: `IsCore`, `IsBonus`, `IsSubSet`

The `AchievementSetType` enum already exists in `AchievementSet.cs`. Make sure to use that existing enum.
```

### Phase 2 Prompt
```
Implement Phase 2 of the sub-set notifications plan (see docs/plans/subset-notifications-plan.md).

Update `Retro Achievement Tracker.WPF/ViewModels/AlertsViewModel.cs` to add sub-set notification support:
1. Add `IsSubSetNotification` property
2. Add animation direction properties: `SubSetInDirection`, `SubSetOutDirection`
3. Add position properties: `SubSetLeft`, `SubSetTop`
4. Add custom video properties: `CustomSubSetVideoPath`, `CustomSubSetEnabled`
5. Add `SubSetBorderColor` property (default: MediumPurple)
6. Add `SetSubSetAchievementNotification(Achievement)` method
7. Add `SetSampleSubSetNotification()` method for testing
```

### Phase 3 Prompt
```
Implement Phase 3 of the sub-set notifications plan (see docs/plans/subset-notifications-plan.md).

Update `Retro Achievement Tracker.WPF/Views/AlertsOverlay.xaml` and `AlertsOverlay.xaml.cs`:
1. Add `NotificationType.SubSetAchievement` to the enum
2. Add `QueueSubSetNotification(Achievement)` method
3. Add `ShowTestSubSetNotification()` method
4. Update `ProcessQueue()` to handle sub-set achievements with their own settings
5. Update `PlayNotificationAnimation()` to accept isSubSet parameter
6. Add XAML triggers/styles to show different border color for sub-set notifications
```

### Phase 4 Prompt
```
Implement Phase 4 of the sub-set notifications plan (see docs/plans/subset-notifications-plan.md).

Update `Retro Achievement Tracker.WPF/ViewModels/MainViewModel.cs`:
1. Add `SubSetAchievementUnlocked` event
2. Add `EnableSeparateSubSetNotifications` property (with settings persistence)
3. Update `OnTrackingServiceAchievementsUnlocked()` to check `achievement.IsSubSet` and fire appropriate event
4. Add `TestSubSetAlertCommand` command
5. Update stream label writing to include set name
```

### Phase 5 Prompt
```
Implement Phase 5 of the sub-set notifications plan (see docs/plans/subset-notifications-plan.md).

Update `Retro Achievement Tracker.WPF/Models/AppSettings.cs` to add:
1. `EnableSubSetNotifications` (bool, default: false)
2. `SubSetInDirection` (string, default: "Right")
3. `SubSetOutDirection` (string, default: "Left")
4. `SubSetNotificationLeft` (double)
5. `SubSetNotificationTop` (double)
6. `CustomSubSetVideoPath` (string)
7. `CustomSubSetVideoEnabled` (bool)
```

### Phase 6 Prompt
```
Implement Phase 6 of the sub-set notifications plan (see docs/plans/subset-notifications-plan.md).

Update `Retro Achievement Tracker.WPF/MainWindow.xaml.cs`:
1. Subscribe to `_mainViewModel.SubSetAchievementUnlocked` event
2. In the handler, call `_alertsOverlay?.QueueSubSetNotification(achievement)`
3. Ensure cleanup in window closing/cleanup methods
```

### Phase 7 Prompt
```
Implement Phase 7 of the sub-set notifications plan (see docs/plans/subset-notifications-plan.md).

Update `Retro Achievement Tracker.WPF/MainWindow.xaml` to add UI for sub-set notifications:
1. Add a new section/expander "Bonus/Sub-Set Notifications" in the Alerts settings area
2. Add checkbox for "Enable separate notifications for bonus achievements"
3. Add ComboBoxes for In/Out animation direction
4. Add position controls (X/Y numeric inputs)
5. Add custom video file picker with browse button
6. Add "Test Sub-Set Alert" button
```

