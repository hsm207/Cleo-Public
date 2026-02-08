using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.UseCases.AuthenticateUser;

public class AuthenticateUserUseCase : IAuthenticateUserUseCase
{
    private readonly ICredentialStore _credentialStore;

    public AuthenticateUserUseCase(ICredentialStore credentialStore)
    {
        _credentialStore = credentialStore;
    }

    public async Task<AuthenticateUserResponse> ExecuteAsync(AuthenticateUserRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.ApiKey))
        {
            return new AuthenticateUserResponse(false, "API Key cannot be empty, babe! 🥀");
        }

        var identity = new Identity(new ApiKey(request.ApiKey));
        await _credentialStore.SaveIdentityAsync(identity, cancellationToken).ConfigureAwait(false);

        return new AuthenticateUserResponse(true, "Identity persisted! Systems ready for launch! 🚀✨");
    }
}
