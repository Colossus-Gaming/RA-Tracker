using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace RATracker.WPF.Services;

/// <summary>
/// Protects secrets (API key, password) at rest.
///
/// <para>The Windows build uses DPAPI, which ties the ciphertext to the current user account. DPAPI
/// does not exist on macOS or Linux, so this sits behind an interface: the shared core can be
/// consumed by a non-Windows shell without the settings layer throwing, and callers can ask
/// <see cref="ICredentialProtector.IsEncrypted"/> whether the secret is genuinely protected.</para>
/// </summary>
public interface ICredentialProtector
{
    /// <summary>Whether this implementation actually encrypts, as opposed to merely encoding.</summary>
    bool IsEncrypted { get; }

    /// <summary>Short description of the mechanism, for diagnostics and UI warnings.</summary>
    string Description { get; }

    /// <summary>Protects a secret, returning a Base64 string safe to write to settings.</summary>
    string Protect(string plainText);

    /// <summary>Reverses <see cref="Protect"/>. Returns empty when the input cannot be read.</summary>
    string Unprotect(string protectedText);
}

/// <summary>
/// Windows implementation, backed by DPAPI scoped to the current user.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class DpapiCredentialProtector : ICredentialProtector
{
    // Unchanged from the original implementation. Altering this would make every already-saved
    // API key unreadable, so it is deliberately frozen.
    private static readonly byte[] EntropyBytes = "RATracker_v1"u8.ToArray();

    public bool IsEncrypted => true;

    public string Description => "Windows DPAPI (current user)";

    public string Protect(string plainText)
    {
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encrypted = ProtectedData.Protect(plainBytes, EntropyBytes, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encrypted);
    }

    public string Unprotect(string protectedText)
    {
        var encryptedBytes = Convert.FromBase64String(protectedText);
        var plainBytes = ProtectedData.Unprotect(encryptedBytes, EntropyBytes, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(plainBytes);
    }
}

/// <summary>
/// Fallback for platforms without DPAPI. This is <b>encoding, not encryption</b> — anyone who can
/// read settings.json can recover the secret.
///
/// <para>It exists so the shared core runs on macOS and Linux at all. Before shipping a non-Windows
/// build, replace it with a real platform keystore (macOS Keychain via Security.framework, Linux
/// libsecret / Secret Service). <see cref="IsEncrypted"/> returns false so callers can warn.</para>
/// </summary>
public sealed class PlaintextCredentialProtector : ICredentialProtector
{
    public bool IsEncrypted => false;

    public string Description => "Base64 encoding only (NOT encrypted) - no OS keystore wired up yet";

    public string Protect(string plainText) => Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));

    public string Unprotect(string protectedText) => Encoding.UTF8.GetString(Convert.FromBase64String(protectedText));
}

/// <summary>
/// Selects the protector appropriate to the running OS.
/// </summary>
public static class CredentialProtection
{
    private static ICredentialProtector? _current;

    /// <summary>The protector for this platform. DPAPI on Windows, encoding-only elsewhere.</summary>
    public static ICredentialProtector Current
    {
        get
        {
            _current ??= RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? new DpapiCredentialProtector()
                : new PlaintextCredentialProtector();

            return _current;
        }
        // Settable so a host can supply a real keystore implementation (or a test double).
        set => _current = value;
    }
}
