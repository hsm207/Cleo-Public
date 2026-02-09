using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.UseCases.AuthenticateUser;

public class AuthenticateUserUseCase : IAuthenticateUserUseCase
{
    private readonly ICredentialStore _vault;

    public AuthenticateUserUseCase(ICredentialStore vault)
    {
        _vault = vault ?? throw new ArgumentNullException(nameof(vault));
    }

    public async Task<AuthenticateUserResponse> ExecuteAsync(AuthenticateUserRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.ApiKey))
        {
            return new AuthenticateUserResponse(false, "API Key cannot be empty. 🥀");
        }

        var identity = new Identity(new ApiKey(request.ApiKey));
        await _vault.SaveIdentityAsync(identity, cancellationToken).ConfigureAwait(false);

        return new AuthenticateUserResponse(true, "Identity persisted! Systems ready for launch! 🚀✨");
    }
}
