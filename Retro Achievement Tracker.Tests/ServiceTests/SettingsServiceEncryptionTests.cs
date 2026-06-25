using RATracker.WPF.Services;

namespace RATracker.Tests.ServiceTests;

/// <summary>
/// Tests for the real credential encryption used by the app (<see cref="SettingsService"/> DPAPI helpers).
/// Replaces the former tests that exercised a duplicate <c>CredentialProtector</c> class.
/// </summary>
[TestFixture]
public class SettingsServiceEncryptionTests
{
    [Test]
    public void EncryptApiKey_EmptyOrNull_ReturnsEmpty()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SettingsService.EncryptApiKey(string.Empty), Is.EqualTo(string.Empty));
            Assert.That(SettingsService.EncryptApiKey(null!), Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public void DecryptApiKey_EmptyOrNull_ReturnsEmpty()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SettingsService.DecryptApiKey(string.Empty), Is.EqualTo(string.Empty));
            Assert.That(SettingsService.DecryptApiKey(null!), Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public void EncryptDecrypt_RoundTrip_ReturnsOriginal()
    {
        const string original = "MySecretApiKey12345";

        var roundTripped = SettingsService.DecryptApiKey(SettingsService.EncryptApiKey(original));

        Assert.That(roundTripped, Is.EqualTo(original));
    }

    [Test]
    public void EncryptDecrypt_RoundTrip_SpecialAndUnicodeCharacters()
    {
        const string original = "API_Key!@#$%^&*()_+-={}[]|\\:\";<>?,./~`  éüñ";

        var roundTripped = SettingsService.DecryptApiKey(SettingsService.EncryptApiKey(original));

        Assert.That(roundTripped, Is.EqualTo(original));
    }

    [Test]
    public void EncryptDecrypt_RoundTrip_LongString()
    {
        var original = new string('A', 10000);

        var roundTripped = SettingsService.DecryptApiKey(SettingsService.EncryptApiKey(original));

        Assert.That(roundTripped, Is.EqualTo(original));
    }

    [Test]
    public void EncryptApiKey_ProducesBase64()
    {
        var encrypted = SettingsService.EncryptApiKey("test");

        Assert.DoesNotThrow(() => Convert.FromBase64String(encrypted));
    }

    [Test]
    public void EncryptApiKey_DifferentInputs_ProduceDifferentOutput()
    {
        Assert.That(SettingsService.EncryptApiKey("Key1"), Is.Not.EqualTo(SettingsService.EncryptApiKey("Key2")));
    }
}
