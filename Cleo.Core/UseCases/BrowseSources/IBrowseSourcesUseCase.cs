using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.UseCases.BrowseSources;

public record BrowseSourcesRequest();

public record BrowseSourcesResponse(IReadOnlyList<SessionSource> Sources);

public interface IBrowseSourcesUseCase : IUseCase<BrowseSourcesRequest, BrowseSourcesResponse> { }
