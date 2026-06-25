using RATracker.WPF.Services;

namespace RATracker.Tests.ServiceTests;

/// <summary>
/// Tests for <see cref="EnvironmentCredentials"/>. These mutate process environment variables,
/// so the fixture is non-parallelizable and restores the originals after each test.
/// </summary>
[TestFixture]
[NonParallelizable]
public class EnvironmentCredentialsTests
{
    private string? _origUser;
    private string? _origApiKey;
    private string? _origPassword;

    [SetUp]
    public void SetUp()
    {
        _origUser = Environment.GetEnvironmentVariable(EnvironmentCredentials.UsernameVariable);
        _origApiKey = Environment.GetEnvironmentVariable(EnvironmentCredentials.ApiKeyVariable);
        _origPassword = Environment.GetEnvironmentVariable(EnvironmentCredentials.PasswordVariable);

        Environment.SetEnvironmentVariable(EnvironmentCredentials.UsernameVariable, null);
        Environment.SetEnvironmentVariable(EnvironmentCredentials.ApiKeyVariable, null);
        Environment.SetEnvironmentVariable(EnvironmentCredentials.PasswordVariable, null);
    }

    [TearDown]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable(EnvironmentCredentials.UsernameVariable, _origUser);
        Environment.SetEnvironmentVariable(EnvironmentCredentials.ApiKeyVariable, _origApiKey);
        Environment.SetEnvironmentVariable(EnvironmentCredentials.PasswordVariable, _origPassword);
    }

    [Test]
    public void AllUnset_ReturnsNull_AndHasAnyIsFalse()
    {
        Assert.Multiple(() =>
        {
            Assert.That(EnvironmentCredentials.GetUsername(), Is.Null);
            Assert.That(EnvironmentCredentials.GetApiKey(), Is.Null);
            Assert.That(EnvironmentCredentials.GetPassword(), Is.Null);
            Assert.That(EnvironmentCredentials.HasAny, Is.False);
        });
    }

    [Test]
    public void SetValues_AreReturned_AndHasAnyIsTrue()
    {
        Environment.SetEnvironmentVariable(EnvironmentCredentials.UsernameVariable, "Scott");
        Environment.SetEnvironmentVariable(EnvironmentCredentials.ApiKeyVariable, "abc123");
        Environment.SetEnvironmentVariable(EnvironmentCredentials.PasswordVariable, "hunter2");

        Assert.Multiple(() =>
        {
            Assert.That(EnvironmentCredentials.GetUsername(), Is.EqualTo("Scott"));
            Assert.That(EnvironmentCredentials.GetApiKey(), Is.EqualTo("abc123"));
            Assert.That(EnvironmentCredentials.GetPassword(), Is.EqualTo("hunter2"));
            Assert.That(EnvironmentCredentials.HasAny, Is.True);
        });
    }

    [Test]
    public void WhitespaceOrEmpty_TreatedAsUnset()
    {
        Environment.SetEnvironmentVariable(EnvironmentCredentials.UsernameVariable, "   ");
        Environment.SetEnvironmentVariable(EnvironmentCredentials.ApiKeyVariable, "");

        Assert.Multiple(() =>
        {
            Assert.That(EnvironmentCredentials.GetUsername(), Is.Null);
            Assert.That(EnvironmentCredentials.GetApiKey(), Is.Null);
        });
    }

    [Test]
    public void Values_AreTrimmed()
    {
        Environment.SetEnvironmentVariable(EnvironmentCredentials.UsernameVariable, "  Scott  ");

        Assert.That(EnvironmentCredentials.GetUsername(), Is.EqualTo("Scott"));
    }

    [Test]
    public void HasAny_TrueWhenOnlyOneSet()
    {
        Environment.SetEnvironmentVariable(EnvironmentCredentials.ApiKeyVariable, "only-key");

        Assert.That(EnvironmentCredentials.HasAny, Is.True);
    }
}
