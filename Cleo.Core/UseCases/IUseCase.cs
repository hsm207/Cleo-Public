namespace Cleo.Core.UseCases;

/// <summary>
/// Represents a pure business operation (Interactor).
/// </summary>
/// <typeparam name="TRequest">The input data structures (Request Model).</typeparam>
/// <typeparam name="TResponse">The output data structures (Response Model).</typeparam>
public interface IUseCase<in TRequest, TResponse>
{
    Task<TResponse> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
}
