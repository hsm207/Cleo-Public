using System.Net;
using System.Net.Http.Json;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;
using Cleo.Infrastructure.Clients.Jules.Mapping;
using Moq;
using Moq.Protected;
using Xunit;

namespace Cleo.Infrastructure.Tests.Clients.Jules;

public class RestPulseMonitorTests
{
    private readonly Mock<HttpMessageHandler> _handlerMock = new();
    private readonly ISessionStatusMapper _statusMapper = new DefaultSessionStatusMapper();
    private readonly RestPulseMonitor _monitor;
    private readonly SessionId _id = new("session-123");

    public RestPulseMonitorTests()
    {
        var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri("https://jules.googleapis.com/") };
        _monitor = new RestPulseMonitor(httpClient, _statusMapper);
    }

    [Fact(DisplayName = "GetPulse: Returns status without coupling to Creation logic.")]
    public async Task GetPulse_IsIsolatedAndCorrect()
    {
        // Arrange
        var dto = new JulesSessionResponse("session-123", "id", "IN_PROGRESS", "prompt", new SourceContextDto("repo", null), null, true, "NONE", null, null);
        
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = JsonContent.Create(dto) });

        // Act
        var result = await _monitor.GetSessionPulseAsync(_id, CancellationToken.None);

        // Assert
        Assert.Equal(SessionStatus.InProgress, result.Status);
        
        // Verify we used GET on the correct resource URI
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => 
                req.Method == HttpMethod.Get && 
                req.RequestUri!.ToString().Contains("session-123")
            ),
            ItExpr.IsAny<CancellationToken>());
    }
}
