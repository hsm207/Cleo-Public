namespace Cleo.Infrastructure.Security;

/// <summary>
/// Defines a strategy for encrypting and decrypting sensitive domain data.
/// </summary>
internal interface IEncryptionStrategy
{
    /// <summary>
    /// Encrypts the provided plain text.
    /// </summary>
    byte[] Encrypt(string plainText);

    /// <summary>
    /// Decrypts the provided cipher text.
    /// </summary>
    string Decrypt(byte[] cipherText);
}
