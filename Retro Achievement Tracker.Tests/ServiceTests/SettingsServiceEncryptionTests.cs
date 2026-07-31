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

    /// <summary>
    /// An undecryptable stored value must read back as empty so the app re-prompts.
    /// </summary>
    /// <remarks>
    /// A DPAPI blob is itself valid Base64, so the plain-text fallback path will happily
    /// decode one that failed to decrypt and hand back ~60 characters of mojibake. The
    /// caller cannot distinguish that from a real key, so the app looks signed in and
    /// fails every API call instead of asking for the key again. Happens for real when
    /// settings.json is copied to another PC, or a Windows password reset invalidates the
    /// DPAPI master key.
    /// </remarks>
    [Test]
    public void DecryptApiKey_UndecryptableBlob_ReturnsEmptyRatherThanGarbage()
    {
        // Random bytes: valid Base64, but not something DPAPI can unprotect.
        var notOurs = Convert.ToBase64String(Enumerable.Range(0, 64).Select(i => (byte)(i * 7 + 13)).ToArray());

        var result = SettingsService.DecryptApiKey(notOurs);

        Assert.That(result, Is.Empty,
            $"Returned {result.Length} characters of garbage, which the app would treat as a valid key.");
    }

    /// <summary>A key stored as plain Base64 by an older build must still be readable.</summary>
    [Test]
    public void DecryptApiKey_LegacyPlainTextValue_StillReadable()
    {
        const string original = "AbCdEf1234567890";
        var legacy = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(original));

        Assert.That(SettingsService.DecryptApiKey(legacy), Is.EqualTo(original));
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
