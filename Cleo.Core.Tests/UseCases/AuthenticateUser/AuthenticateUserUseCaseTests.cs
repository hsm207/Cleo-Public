using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.UseCases.AuthenticateUser;
using Xunit;

namespace Cleo.Core.Tests.UseCases.AuthenticateUser;

public sealed class AuthenticateUserUseCaseTests
{
    private readonly FakeVault _vault = new();
    private readonly AuthenticateUserUseCase _sut;

    public AuthenticateUserUseCaseTests()
    {
        _sut = new AuthenticateUserUseCase(_vault);
    }

    [Fact(DisplayName = "Given a valid API Key, when logging in, then the Identity should be persisted in the Vault.")]
    public async Task ShouldPersistIdentity()
    {
        // Arrange
        var apiKey = "sk-12345";
        var request = new AuthenticateUserRequest(apiKey);

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(_vault.Identity);
        Assert.Equal(apiKey, _vault.Identity.ApiKey.Value);
    }

    [Fact(DisplayName = "Given an empty API Key, when logging in, then it should refuse the Identity.")]
    public async Task ShouldRefuseEmptyKey()
    {
        // Arrange
        var request = new AuthenticateUserRequest("");

        // Act
        var result = await _sut.ExecuteAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.Contains("cannot be empty", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(_vault.Identity);
    }

    private sealed class FakeVault : IVault
    {
        public Identity? Identity { get; private set; }

        public Task StoreAsync(Identity identity, CancellationToken cancellationToken = default)
        {
            Identity = identity;
            return Task.CompletedTask;
        }

        public Task<Identity?> RetrieveAsync(CancellationToken cancellationToken = default)
        {
             return Task.FromResult(Identity);
        }

        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            Identity = null;
            return Task.CompletedTask;
        }
    }
}
