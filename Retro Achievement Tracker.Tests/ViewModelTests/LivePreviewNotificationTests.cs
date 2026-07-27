using System.ComponentModel;
using RATracker.WPF.ViewModels;

namespace RATracker.Tests.ViewModelTests;

/// <summary>
/// Verifies the live-preview contract: when a scalar layout property changes (driven by a settings
/// slider), the overlay ViewModel raises PropertyChanged for the computed CornerRadius/Thickness
/// "wrapper" property that the overlay XAML actually binds to. If a wrapper is not notified, the
/// open overlay window will NOT update until it is reopened — i.e. the slider looks "not linked up".
///
/// (The slider -> ViewModel-property half lives in MainWindow code-behind ValueChanged handlers and
/// is audited separately; these tests lock down the ViewModel -> binding half for every overlay.)
/// </summary>
[TestFixture]
public class LivePreviewNotificationTests
{
    /// <summary>Mutates the VM and returns the property names that fired PropertyChanged.</summary>
    private static List<string> Fired(INotifyPropertyChanged vm, Action mutate)
    {
        var names = new List<string>();
        PropertyChangedEventHandler handler = (_, e) => names.Add(e.PropertyName!);
        vm.PropertyChanged += handler;
        try { mutate(); }
        finally { vm.PropertyChanged -= handler; }
        return names;
    }

    [Test]
    public void Focus_LayoutWrappers_NotifyOnScalarChange()
    {
        var vm = new FocusViewModel();
        Assert.Multiple(() =>
        {
            Assert.That(Fired(vm, () => vm.BadgeCornerRadius += 1), Does.Contain(nameof(vm.BadgeCornerRadiusValue)), "BadgeCornerRadius -> BadgeCornerRadiusValue");
            Assert.That(Fired(vm, () => vm.ContainerCornerRadius += 1), Does.Contain(nameof(vm.ContainerCornerRadiusValue)), "ContainerCornerRadius -> ContainerCornerRadiusValue");
            Assert.That(Fired(vm, () => vm.ContainerMargin += 1), Does.Contain(nameof(vm.ContainerMarginValue)), "ContainerMargin -> ContainerMarginValue");
            Assert.That(Fired(vm, () => vm.ContentSpacing += 1), Does.Contain(nameof(vm.ContentSpacingMargin)), "ContentSpacing -> ContentSpacingMargin");
            Assert.That(Fired(vm, () => vm.LineMargin += 1), Does.Contain(nameof(vm.LineMarginValue)), "LineMargin -> LineMarginValue");
        });
    }

    [Test]
    public void Focus_ContainerCornerRadius_WrapperReflectsNewValue()
    {
        var vm = new FocusViewModel { ContainerCornerRadius = 7 };
        vm.ContainerCornerRadius = 23;
        Assert.That(vm.ContainerCornerRadiusValue.TopLeft, Is.EqualTo(23));
    }

    [Test]
    public void Alerts_LayoutWrappers_NotifyOnScalarChange()
    {
        var vm = new AlertsViewModel();
        Assert.Multiple(() =>
        {
            Assert.That(Fired(vm, () => vm.ContainerCornerRadius += 1), Does.Contain(nameof(vm.ContainerCornerRadiusValue)));
            Assert.That(Fired(vm, () => vm.BadgeCornerRadius += 1), Does.Contain(nameof(vm.BadgeCornerRadiusValue)));
            Assert.That(Fired(vm, () => vm.ContentSpacing += 1), Does.Contain(nameof(vm.ContentSpacingMargin)));
        });
    }

    [Test]
    public void AchievementList_LayoutWrappers_NotifyOnScalarChange()
    {
        var vm = new AchievementListViewModel();
        Assert.Multiple(() =>
        {
            Assert.That(Fired(vm, () => vm.ContainerCornerRadius += 1), Does.Contain(nameof(vm.ContainerCornerRadiusValue)));
            Assert.That(Fired(vm, () => vm.ContainerMargin += 1), Does.Contain(nameof(vm.ContainerMarginValue)));
            Assert.That(Fired(vm, () => vm.BadgeSpacing += 1), Does.Contain(nameof(vm.BadgeSpacingValue)));
            Assert.That(Fired(vm, () => vm.BadgeCornerRadius += 1), Does.Contain(nameof(vm.BadgeCornerRadiusValue)));
        });
    }

    [Test]
    public void GameProgress_LayoutWrappers_NotifyOnScalarChange()
    {
        var vm = new GameProgressViewModel();
        Assert.Multiple(() =>
        {
            Assert.That(Fired(vm, () => vm.ContainerCornerRadius += 1), Does.Contain(nameof(vm.ContainerCornerRadiusValue)));
            Assert.That(Fired(vm, () => vm.ContainerMargin += 1), Does.Contain(nameof(vm.ContainerMarginValue)));
            Assert.That(Fired(vm, () => vm.RowSpacing += 1), Does.Contain(nameof(vm.RowSpacingValue)));
        });
    }

    [Test]
    public void GameInfo_LayoutWrappers_NotifyOnScalarChange()
    {
        var vm = new GameInfoViewModel();
        Assert.Multiple(() =>
        {
            Assert.That(Fired(vm, () => vm.ContainerCornerRadius += 1), Does.Contain(nameof(vm.ContainerCornerRadiusValue)));
            Assert.That(Fired(vm, () => vm.ContainerMargin += 1), Does.Contain(nameof(vm.ContainerMarginValue)));
            Assert.That(Fired(vm, () => vm.RowSpacing += 1), Does.Contain(nameof(vm.RowSpacingValue)));
            Assert.That(Fired(vm, () => vm.BadgeCornerRadius += 1), Does.Contain(nameof(vm.BadgeCornerRadiusValue)));
        });
    }

    [Test]
    public void RelatedMedia_LayoutWrappers_NotifyOnScalarChange()
    {
        var vm = new RelatedMediaViewModel();
        Assert.Multiple(() =>
        {
            Assert.That(Fired(vm, () => vm.ContainerCornerRadius += 1), Does.Contain(nameof(vm.ContainerCornerRadiusValue)));
            Assert.That(Fired(vm, () => vm.ContainerMargin += 1), Does.Contain(nameof(vm.ContainerMarginValue)));
            Assert.That(Fired(vm, () => vm.ImageCornerRadius += 1), Does.Contain(nameof(vm.ImageCornerRadiusValue)));
        });
    }

    [Test]
    public void RecentUnlocks_LayoutWrappers_NotifyOnScalarChange()
    {
        var vm = new RecentUnlocksViewModel();
        Assert.Multiple(() =>
        {
            Assert.That(Fired(vm, () => vm.ContainerCornerRadius += 1), Does.Contain(nameof(vm.ContainerCornerRadiusValue)));
            Assert.That(Fired(vm, () => vm.ContainerMargin += 1), Does.Contain(nameof(vm.ContainerMarginValue)));
            Assert.That(Fired(vm, () => vm.ItemSpacing += 1), Does.Contain(nameof(vm.ItemSpacingValue)));
            Assert.That(Fired(vm, () => vm.BadgeCornerRadius += 1), Does.Contain(nameof(vm.BadgeCornerRadiusValue)));
        });
    }

    [Test]
    public void UserInfo_LayoutWrappers_NotifyOnScalarChange()
    {
        var vm = new UserInfoViewModel();
        Assert.Multiple(() =>
        {
            Assert.That(Fired(vm, () => vm.ContainerCornerRadius += 1), Does.Contain(nameof(vm.ContainerCornerRadiusValue)));
            Assert.That(Fired(vm, () => vm.ContainerMargin += 1), Does.Contain(nameof(vm.ContainerMarginValue)));
            Assert.That(Fired(vm, () => vm.RowSpacing += 1), Does.Contain(nameof(vm.RowSpacingValue)));
        });
    }
}
