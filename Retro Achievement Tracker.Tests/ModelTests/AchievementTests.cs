using RATracker.Models;

namespace RATracker.Tests.ModelTests;

/// <summary>
/// Tests for the real <see cref="Achievement"/> model (equality, ordering, cloning, set membership).
/// </summary>
[TestFixture]
public class AchievementTests
{
    [Test]
    public void Equals_SameId_ReturnsTrue()
    {
        var a = new Achievement { Id = 123, Title = "Test Achievement" };
        var b = new Achievement { Id = 123, Title = "Different Title" };

        Assert.That(a.Equals(b), Is.True);
    }

    [Test]
    public void Equals_DifferentId_ReturnsFalse()
    {
        var a = new Achievement { Id = 123 };
        var b = new Achievement { Id = 456 };

        Assert.That(a.Equals(b), Is.False);
    }

    [Test]
    public void Equals_NullOther_ReturnsFalse()
    {
        var a = new Achievement { Id = 123 };

        Assert.That(a.Equals(null), Is.False);
    }

    [Test]
    public void GetHashCode_MatchesId()
    {
        var a = new Achievement { Id = 42 };

        Assert.That(a.GetHashCode(), Is.EqualTo(42.GetHashCode()));
    }

    [Test]
    public void Contains_UsesIdEquality()
    {
        var list = new List<Achievement> { new() { Id = 123 } };

        Assert.That(list.Contains(new Achievement { Id = 123 }), Is.True);
    }

    [Test]
    public void CompareTo_BothUnlocked_EarlierDateComesFirst()
    {
        var earlier = new Achievement { Id = 1, DateEarned = new DateTime(2024, 1, 1) };
        var later = new Achievement { Id = 2, DateEarned = new DateTime(2024, 1, 2) };

        Assert.Multiple(() =>
        {
            Assert.That(earlier.CompareTo(later), Is.LessThan(0));
            Assert.That(later.CompareTo(earlier), Is.GreaterThan(0));
        });
    }

    [Test]
    public void CompareTo_UnlockedVsLocked_UnlockedComesAfter()
    {
        var unlocked = new Achievement { Id = 1, DateEarned = DateTime.Now };
        var locked = new Achievement { Id = 2, DateEarned = null };

        Assert.Multiple(() =>
        {
            Assert.That(unlocked.CompareTo(locked), Is.GreaterThan(0));
            Assert.That(locked.CompareTo(unlocked), Is.LessThan(0));
        });
    }

    [Test]
    public void CompareTo_NullOther_ReturnsPositive()
    {
        Assert.That(new Achievement { Id = 1 }.CompareTo(null), Is.GreaterThan(0));
    }

    [Test]
    public void Sort_OrdersLockedBeforeUnlocked()
    {
        var achievements = new List<Achievement>
        {
            new() { Id = 3, DateEarned = null, DisplayOrder = 1 },
            new() { Id = 1, DateEarned = new DateTime(2024, 1, 15), DisplayOrder = 2 },
            new() { Id = 2, DateEarned = new DateTime(2024, 1, 10), DisplayOrder = 3 },
            new() { Id = 4, DateEarned = null, DisplayOrder = 5 }
        };

        achievements.Sort();

        Assert.Multiple(() =>
        {
            Assert.That(achievements[0].Id, Is.EqualTo(4)); // Locked, DisplayOrder 5
            Assert.That(achievements[1].Id, Is.EqualTo(3)); // Locked, DisplayOrder 1
            Assert.That(achievements[2].Id, Is.EqualTo(2)); // Unlocked, Jan 10
            Assert.That(achievements[3].Id, Is.EqualTo(1)); // Unlocked, Jan 15
        });
    }

    [Test]
    public void Clone_CreatesIndependentCopy()
    {
        var original = new Achievement { Id = 123, Title = "Original", Points = 10, DateEarned = DateTime.Now };

        var clone = (Achievement)original.Clone();
        clone.Title = "Modified";

        Assert.Multiple(() =>
        {
            Assert.That(clone, Is.Not.SameAs(original));
            Assert.That(clone.Id, Is.EqualTo(123));
            Assert.That(original.Title, Is.EqualTo("Original"));
        });
    }

    [Test]
    public void DefaultValues_StringsAreEmptyAndSetTypeIsCore()
    {
        var a = new Achievement();

        Assert.Multiple(() =>
        {
            Assert.That(a.Id, Is.EqualTo(0));
            Assert.That(a.Title, Is.EqualTo(string.Empty));
            Assert.That(a.Description, Is.EqualTo(string.Empty));
            Assert.That(a.DateEarned, Is.Null);
            Assert.That(a.SetType, Is.EqualTo(AchievementSetType.Core));
            Assert.That(a.SetId, Is.Null);
            Assert.That(a.IsCore, Is.True);
            Assert.That(a.IsSubSet, Is.False);
        });
    }

    [TestCase(AchievementSetType.Core, true, false)]
    [TestCase(AchievementSetType.Bonus, false, true)]
    [TestCase(AchievementSetType.Specialty, false, true)]
    [TestCase(AchievementSetType.Exclusive, false, true)]
    public void IsCore_And_IsSubSet_ReflectSetType(AchievementSetType type, bool isCore, bool isSubSet)
    {
        var a = new Achievement { SetType = type };

        Assert.Multiple(() =>
        {
            Assert.That(a.IsCore, Is.EqualTo(isCore));
            Assert.That(a.IsSubSet, Is.EqualTo(isSubSet));
        });
    }
}
