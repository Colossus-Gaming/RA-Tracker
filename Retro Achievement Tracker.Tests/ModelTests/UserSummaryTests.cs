using RATracker.Models;

namespace RATracker.Tests.ModelTests;

/// <summary>
/// Tests for the real <see cref="UserSummary"/> model.
/// </summary>
[TestFixture]
public class UserSummaryTests
{
    [Test]
    public void RetroRatio_CalculatesToTwoDecimals()
    {
        Assert.That(new UserSummary { TotalPoints = 1000, TotalTruePoints = 1500 }.RetroRatio, Is.EqualTo("1.50"));
    }

    [Test]
    public void RetroRatio_ZeroPoints_ReturnsZeroRatio_NoDivideByZero()
    {
        Assert.That(new UserSummary { TotalPoints = 0, TotalTruePoints = 1500 }.RetroRatio, Is.EqualTo("0.00"));
    }

    [Test]
    public void Equals_ComparesProgressFields_IgnoresNameAndMotto()
    {
        var a = new UserSummary { UserName = "Player1", Motto = "Hi", LastGameID = 123, TotalPoints = 1000, TotalTruePoints = 1500, Rank = 500 };
        var b = new UserSummary { UserName = "Other", Motto = "Bye", LastGameID = 123, TotalPoints = 1000, TotalTruePoints = 1500, Rank = 500 };

        Assert.That(a.Equals(b), Is.True);
    }

    [TestCase(124, 1000, 1500, 500)]
    [TestCase(123, 2000, 1500, 500)]
    [TestCase(123, 1000, 2000, 500)]
    [TestCase(123, 1000, 1500, 600)]
    public void Equals_DifferentProgressField_ReturnsFalse(int lastGame, int points, int truePoints, int rank)
    {
        var baseline = new UserSummary { LastGameID = 123, TotalPoints = 1000, TotalTruePoints = 1500, Rank = 500 };
        var other = new UserSummary { LastGameID = lastGame, TotalPoints = points, TotalTruePoints = truePoints, Rank = rank };

        Assert.That(baseline.Equals(other), Is.False);
    }

    [Test]
    public void Equals_NullOther_ReturnsFalse()
    {
        Assert.That(new UserSummary { LastGameID = 123 }.Equals(null), Is.False);
    }

    [Test]
    public void Clone_CreatesIndependentCopy()
    {
        var original = new UserSummary { UserName = "TestUser", TotalPoints = 1000, Rank = 500 };

        var clone = (UserSummary)original.Clone();
        clone.UserName = "Modified";
        clone.TotalPoints = 2000;

        Assert.Multiple(() =>
        {
            Assert.That(clone, Is.Not.SameAs(original));
            Assert.That(original.UserName, Is.EqualTo("TestUser"));
            Assert.That(original.TotalPoints, Is.EqualTo(1000));
        });
    }

    [Test]
    public void DefaultValues_AreEmpty()
    {
        var user = new UserSummary();

        Assert.Multiple(() =>
        {
            Assert.That(user.UserName, Is.EqualTo(string.Empty));
            Assert.That(user.Motto, Is.EqualTo(string.Empty));
            Assert.That(user.LastGameID, Is.EqualTo(0));
            Assert.That(user.TotalPoints, Is.EqualTo(0));
        });
    }
}
