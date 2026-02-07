namespace Cleo.Infrastructure.Security;

/// <summary>
/// Thrown when a cryptographic or security-related operation fails within the vault.
/// </summary>
public sealed class VaultSecurityException : Exception
{
    public VaultSecurityException() { }
    public VaultSecurityException(string message) : base(message) { }
    public VaultSecurityException(string message, Exception innerException) : base(message, innerException) { }
}
