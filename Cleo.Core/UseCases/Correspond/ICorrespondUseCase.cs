using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.UseCases.Correspond;

public record CorrespondRequest(SessionId Id, string Message);

public record CorrespondResponse(SessionId Id, DateTimeOffset SentAt);

public interface ICorrespondUseCase : IUseCase<CorrespondRequest, CorrespondResponse> { }
