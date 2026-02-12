using System.Security.Cryptography;
using System.Text;

namespace Cleo.Infrastructure.Security;

/// <summary>
/// Cross-platform encryption strategy using AES-GCM with a machine-derived key.
/// </summary>
internal sealed class AesGcmEncryptionStrategy : IEncryptionStrategy
{
    private const int NonceSize = 12; // 96 bits
    private const int TagSize = 16;   // 128 bits

    public byte[] Encrypt(string plainText)
    {
        ArgumentNullException.ThrowIfNull(plainText);

        byte[] key = DeriveKey();
        byte[] nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);

        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] cipherText = new byte[plainBytes.Length];
        byte[] tag = new byte[TagSize];

        using (var aes = new AesGcm(key, TagSize))
        {
            aes.Encrypt(nonce, plainBytes, cipherText, tag);
        }

        // Output format: [Nonce][Tag][CipherText]
        var result = new byte[NonceSize + TagSize + cipherText.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, result, NonceSize, TagSize);
        Buffer.BlockCopy(cipherText, 0, result, NonceSize + TagSize, cipherText.Length);

        return result;
    }

    public string Decrypt(byte[] encryptedData)
    {
        ArgumentNullException.ThrowIfNull(encryptedData);

        try
        {
            byte[] key = DeriveKey();
        var nonce = encryptedData.AsSpan(0, NonceSize);
        var tag = encryptedData.AsSpan(NonceSize, TagSize);
        var cipherText = encryptedData.AsSpan(NonceSize + TagSize);

        var plainBytes = new byte[cipherText.Length];

            using (var aes = new AesGcm(key, TagSize))
            {
                aes.Decrypt(nonce, cipherText, tag, plainBytes);
            }

            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (Exception ex)
        {
            throw new VaultSecurityException("Decryption failed.", ex);
        }
    }

    private static byte[] DeriveKey()
    {
        // Simple machine-bound key derivation.
        // In a real high-security app, we might use a more complex KDF with salt.
        string seed = $"{Environment.MachineName}-{Environment.UserName}";
        return SHA256.HashData(Encoding.UTF8.GetBytes(seed));
    }
}
