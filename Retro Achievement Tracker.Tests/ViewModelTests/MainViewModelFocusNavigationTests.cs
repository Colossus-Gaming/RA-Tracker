using RATracker.Models;
using RATracker.WPF.ViewModels;

namespace RATracker.Tests.ViewModelTests;

/// <summary>
/// Tests for Focus navigation: Prev/Next must NOT wrap around (they stop at the first/last
/// achievement), and they only BROWSE the dashboard preview — navigating must NOT fire FocusChanged
/// or touch the on-stream overlay. Only the separate "Set Focus" action commits the focus.
/// </summary>
[TestFixture]
public class MainViewModelFocusNavigationTests
{
    private MainViewModel _vm = null!;

    [SetUp]
    public void SetUp()
    {
        _vm = new MainViewModel(loadSampleData: false);
        _vm.CurrentGame = GameWithLockedAchievements(4);
    }

    private static GameInfo GameWithLockedAchievements(int count)
    {
        var list = new List<Achievement>();
        for (int i = 1; i <= count; i++)
        {
            // DateEarned == null => locked, so all land in LockedAchievements.
            list.Add(new Achievement { Id = i, Title = $"Ach {i}", DisplayOrder = i });
        }
        return new GameInfo { Id = 1, Title = "Test Game", ConsoleName = "Test", Achievements = list };
    }

    [Test]
    public void Setup_StartsAtFirstLockedAchievement()
    {
        Assert.Multiple(() =>
        {
            Assert.That(_vm.LockedAchievements, Has.Count.EqualTo(4));
            Assert.That(_vm.CurrentFocusIndex, Is.EqualTo(0));
        });
    }

    [Test]
    public void NextFocus_AdvancesByOne()
    {
        _vm.NextFocusCommand.Execute(null);
        Assert.Multiple(() =>
        {
            Assert.That(_vm.CurrentFocusIndex, Is.EqualTo(1));
            Assert.That(_vm.CurrentFocusAchievement, Is.SameAs(_vm.LockedAchievements[1]));
        });
    }

    [Test]
    public void PreviousFocus_DecrementsByOne()
    {
        _vm.CurrentFocusIndex = 2;
        _vm.PreviousFocusCommand.Execute(null);
        Assert.That(_vm.CurrentFocusIndex, Is.EqualTo(1));
    }

    [Test]
    public void NextFocus_AtLast_DoesNotWrap()
    {
        _vm.CurrentFocusIndex = 3; // last
        _vm.NextFocusCommand.Execute(null);
        Assert.That(_vm.CurrentFocusIndex, Is.EqualTo(3));
    }

    [Test]
    public void PreviousFocus_AtFirst_DoesNotWrap()
    {
        _vm.CurrentFocusIndex = 0; // first
        _vm.PreviousFocusCommand.Execute(null);
        Assert.That(_vm.CurrentFocusIndex, Is.EqualTo(0));
    }

    [Test]
    public void CanGoToPreviousFocus_FalseAtFirst_TrueOtherwise()
    {
        _vm.CurrentFocusIndex = 0;
        Assert.That(_vm.CanGoToPreviousFocus, Is.False);
        _vm.CurrentFocusIndex = 1;
        Assert.That(_vm.CanGoToPreviousFocus, Is.True);
    }

    [Test]
    public void CanGoToNextFocus_FalseAtLast_TrueOtherwise()
    {
        _vm.CurrentFocusIndex = 3;
        Assert.That(_vm.CanGoToNextFocus, Is.False);
        _vm.CurrentFocusIndex = 2;
        Assert.That(_vm.CanGoToNextFocus, Is.True);
    }

    [Test]
    public void Navigation_BrowsesOnly_DoesNotSetFocus()
    {
        var fired = false;
        _vm.FocusChanged += (_, _) => fired = true;

        _vm.NextFocusCommand.Execute(null);
        _vm.NextFocusCommand.Execute(null);
        _vm.PreviousFocusCommand.Execute(null);

        Assert.Multiple(() =>
        {
            Assert.That(fired, Is.False, "Prev/Next must only browse the preview, never set focus");
            Assert.That(_vm.CurrentFocusIndex, Is.EqualTo(1), "but the preview index still moves");
            Assert.That(_vm.CurrentFocusAchievement, Is.SameAs(_vm.LockedAchievements[1]));
        });
    }

    [Test]
    public void SetFocus_CommitsFocus_FiresFocusChanged()
    {
        _vm.CurrentFocusIndex = 2;
        Achievement? fired = null;
        _vm.FocusChanged += (_, a) => fired = a;

        _vm.SetFocusCommand.Execute(null);

        Assert.That(fired, Is.SameAs(_vm.LockedAchievements[2]), "Set Focus is the only action that sets the focus");
    }
}
