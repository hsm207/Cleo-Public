using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace Cleo.Infrastructure.Security;

/// <summary>
/// Windows-specific encryption strategy using Data Protection API (DPAPI).
/// </summary>
[SupportedOSPlatform("windows")]
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
internal sealed class DpapiEncryptionStrategy : IEncryptionStrategy
{
    public byte[] Encrypt(string plainText)
    {
        ArgumentNullException.ThrowIfNull(plainText);
        var data = Encoding.UTF8.GetBytes(plainText);
        return ProtectedData.Protect(data, optionalEntropy: null, scope: DataProtectionScope.CurrentUser);
    }

    public string Decrypt(byte[] cipherText)
    {
        ArgumentNullException.ThrowIfNull(cipherText);
        var data = ProtectedData.Unprotect(cipherText, optionalEntropy: null, scope: DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(data);
    }
}
