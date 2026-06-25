using RATracker.Models;
using RATracker.WPF.ViewModels;

namespace RATracker.Tests.ViewModelTests;

/// <summary>
/// Tests for AlertsViewModel per-set-type visual differentiation (STORY-001 phase 5).
/// Covers subset classification, the corner badge, and that mastery clears any stale subset accent.
/// </summary>
[TestFixture]
public class AlertsViewModelSubsetTests
{
    private AlertsViewModel _viewModel = null!;

    [SetUp]
    public void SetUp()
    {
        _viewModel = new AlertsViewModel();
    }

    private static Achievement MakeAchievement(AchievementSetType setType, string? setName = null) => new()
    {
        Id = 1,
        Title = "Test Achievement",
        Description = "Test Description",
        Points = 25,
        BadgeUri = "https://example.com/badge.png",
        SetType = setType,
        SetName = setName
    };

    [Test]
    public void SetAchievementNotification_Core_IsNotSubsetAndHidesBadge()
    {
        _viewModel.SetAchievementNotification(MakeAchievement(AchievementSetType.Core, "Core"));

        Assert.Multiple(() =>
        {
            Assert.That(_viewModel.SetType, Is.EqualTo(AchievementSetType.Core));
            Assert.That(_viewModel.IsSubSetNotification, Is.False);
            Assert.That(_viewModel.SetBadgeVisible, Is.False);
            Assert.That(_viewModel.SetBadgeText, Is.Empty);
            // Core keeps the user's configured border color (same brush instance).
            Assert.That(_viewModel.EffectiveBorderColor, Is.SameAs(_viewModel.BorderColor));
        });
    }

    [Test]
    public void SetAchievementNotification_Bonus_IsSubsetAndShowsBadge()
    {
        _viewModel.SetAchievementNotification(MakeAchievement(AchievementSetType.Bonus, "Bonus"));

        Assert.Multiple(() =>
        {
            Assert.That(_viewModel.IsSubSetNotification, Is.True);
            Assert.That(_viewModel.SetBadgeVisible, Is.True);
            Assert.That(_viewModel.SetBadgeText, Is.EqualTo("BONUS"));
            // Subsets render an accent border distinct from the default Core border.
            Assert.That(_viewModel.EffectiveBorderColor, Is.Not.SameAs(_viewModel.BorderColor));
            Assert.That(_viewModel.EffectiveBorderColor, Is.SameAs(_viewModel.SetAccentColor));
        });
    }

    [TestCase(AchievementSetType.Bonus, "BONUS")]
    [TestCase(AchievementSetType.Specialty, "SPECIALTY")]
    [TestCase(AchievementSetType.Exclusive, "EXCLUSIVE")]
    [TestCase(AchievementSetType.Challenge, "CHALLENGE")]
    public void SetAchievementNotification_NonCore_BadgeTextMatchesType(AchievementSetType type, string expected)
    {
        _viewModel.SetAchievementNotification(MakeAchievement(type));

        Assert.Multiple(() =>
        {
            Assert.That(_viewModel.SetBadgeVisible, Is.True);
            Assert.That(_viewModel.SetBadgeText, Is.EqualTo(expected));
        });
    }

    [Test]
    public void SetAchievementNotification_Unknown_TreatedAsCore()
    {
        _viewModel.SetAchievementNotification(MakeAchievement(AchievementSetType.Unknown));

        Assert.Multiple(() =>
        {
            Assert.That(_viewModel.IsSubSetNotification, Is.False);
            Assert.That(_viewModel.SetBadgeVisible, Is.False);
        });
    }

    [Test]
    public void SetAchievementNotification_EmptySetName_FallsBackToTypeName()
    {
        _viewModel.SetAchievementNotification(MakeAchievement(AchievementSetType.Bonus, setName: null));

        Assert.That(_viewModel.SetName, Is.EqualTo("Bonus"));
    }

    [Test]
    public void SetAchievementNotification_ProvidedSetName_IsPreserved()
    {
        _viewModel.SetAchievementNotification(MakeAchievement(AchievementSetType.Challenge, "Speedrun Showcase"));

        Assert.Multiple(() =>
        {
            Assert.That(_viewModel.SetName, Is.EqualTo("Speedrun Showcase"));
            // The short categorical label still drives the badge/accent.
            Assert.That(_viewModel.SetBadgeText, Is.EqualTo("CHALLENGE"));
        });
    }

    [Test]
    public void SetMasteryNotification_AfterSubset_ClearsAccentAndBadge()
    {
        // Arrange: a subset alert leaves the VM in a non-core state.
        _viewModel.SetAchievementNotification(MakeAchievement(AchievementSetType.Bonus, "Bonus"));
        Assume.That(_viewModel.IsSubSetNotification, Is.True);

        // Act: a mastery notification follows.
        _viewModel.SetMasteryNotification(new GameInfo { Title = "Test Game" });

        // Assert: mastery uses the standard treatment, no stale subset accent/badge.
        Assert.Multiple(() =>
        {
            Assert.That(_viewModel.SetType, Is.EqualTo(AchievementSetType.Core));
            Assert.That(_viewModel.IsSubSetNotification, Is.False);
            Assert.That(_viewModel.SetBadgeVisible, Is.False);
            Assert.That(_viewModel.EffectiveBorderColor, Is.SameAs(_viewModel.BorderColor));
        });
    }

    [Test]
    public void SetType_WhenChanged_RaisesPropertyChangedForVisualProperties()
    {
        var changed = new List<string>();
        _viewModel.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        _viewModel.SetType = AchievementSetType.Exclusive;

        Assert.Multiple(() =>
        {
            Assert.That(changed, Does.Contain(nameof(AlertsViewModel.IsSubSetNotification)));
            Assert.That(changed, Does.Contain(nameof(AlertsViewModel.EffectiveBorderColor)));
            Assert.That(changed, Does.Contain(nameof(AlertsViewModel.SetBadgeVisible)));
            Assert.That(changed, Does.Contain(nameof(AlertsViewModel.SetBadgeText)));
        });
    }
}
