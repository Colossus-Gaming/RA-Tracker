using System;
using System.Security.Cryptography;
using System.Text;

namespace Retro_Achievement_Tracker.Services
{
    /// <summary>
    /// Provides secure encryption/decryption of sensitive data using Windows DPAPI.
    /// Data encrypted with this class can only be decrypted by the same Windows user.
    /// </summary>
    public static class CredentialProtector
    {
        /// <summary>
        /// Encrypts a string using Windows DPAPI with user-scope protection.
        /// </summary>
        /// <param name="plainText">The plain text to encrypt.</param>
        /// <returns>Base64-encoded encrypted data, or empty string if input is null/empty.</returns>
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            try
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedBytes = ProtectedData.Protect(
                    plainBytes,
                    null,
                    DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encryptedBytes);
            }
            catch (CryptographicException)
            {
                // If encryption fails, return empty string
                // This can happen in rare cases with Windows configuration issues
                return string.Empty;
            }
        }

        /// <summary>
        /// Decrypts a Base64-encoded DPAPI-encrypted string.
        /// </summary>
        /// <param name="encryptedText">Base64-encoded encrypted data.</param>
        /// <returns>Decrypted plain text, or empty string if decryption fails.</returns>
        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;

            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                byte[] plainBytes = ProtectedData.Unprotect(
                    encryptedBytes,
                    null,
                    DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch (CryptographicException)
            {
                // Decryption failed - could be different user or corrupted data
                return string.Empty;
            }
            catch (FormatException)
            {
                // Invalid Base64 - data is corrupted
                return string.Empty;
            }
        }

        /// <summary>
        /// Checks if a string appears to be an encrypted API key (Base64 format).
        /// </summary>
        public static bool IsEncrypted(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            try
            {
                // Try to decode as Base64 - encrypted values are always Base64
                Convert.FromBase64String(value);
                // API keys are typically short (32 chars), encrypted versions are longer
                return value.Length > 50;
            }
            catch
            {
                return false;
            }
        }
    }
}
