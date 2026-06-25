using RATracker.WPF.Services;
using RATracker.WPF.ViewModels;

namespace RATracker.Tests.ViewModelTests;

/// <summary>
/// Verifies that credential environment variables flow into <see cref="MainViewModel"/> and take
/// precedence over stored settings. Mutates process environment variables, so it is non-parallelizable.
/// </summary>
[TestFixture]
[NonParallelizable]
public class MainViewModelEnvironmentTests
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
    }

    [TearDown]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable(EnvironmentCredentials.UsernameVariable, _origUser);
        Environment.SetEnvironmentVariable(EnvironmentCredentials.ApiKeyVariable, _origApiKey);
        Environment.SetEnvironmentVariable(EnvironmentCredentials.PasswordVariable, _origPassword);
    }

    [Test]
    public void Constructor_AppliesEnvironmentCredentials()
    {
        Environment.SetEnvironmentVariable(EnvironmentCredentials.UsernameVariable, "EnvUser");
        Environment.SetEnvironmentVariable(EnvironmentCredentials.ApiKeyVariable, "EnvApiKey123");
        Environment.SetEnvironmentVariable(EnvironmentCredentials.PasswordVariable, "EnvPass");

        var vm = new MainViewModel(loadSampleData: false);

        Assert.Multiple(() =>
        {
            Assert.That(vm.Username, Is.EqualTo("EnvUser"));
            Assert.That(vm.ApiKey, Is.EqualTo("EnvApiKey123"));
            Assert.That(vm.Password, Is.EqualTo("EnvPass"));
            // Credentials present -> Start is enabled.
            Assert.That(vm.CanStart, Is.True);
        });
    }

    [Test]
    public void Constructor_NoEnvVars_DoesNotThrow()
    {
        Environment.SetEnvironmentVariable(EnvironmentCredentials.UsernameVariable, null);
        Environment.SetEnvironmentVariable(EnvironmentCredentials.ApiKeyVariable, null);
        Environment.SetEnvironmentVariable(EnvironmentCredentials.PasswordVariable, null);

        Assert.DoesNotThrow(() => _ = new MainViewModel(loadSampleData: false));
    }
}
