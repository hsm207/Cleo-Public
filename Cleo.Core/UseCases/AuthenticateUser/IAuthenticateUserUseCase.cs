using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.UseCases.AuthenticateUser;

public record AuthenticateUserRequest(string ApiKey);

public record AuthenticateUserResponse(bool Success, string Message);

public interface IAuthenticateUserUseCase : IUseCase<AuthenticateUserRequest, AuthenticateUserResponse> { }
