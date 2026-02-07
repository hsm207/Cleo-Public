using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Cleo.Infrastructure.Tests.Jules;

/// <summary>
/// A high-fidelity virtual representation of the Jules API for automated integration testing.
/// </summary>
public sealed class JulesMockServer : IDisposable
{
    private readonly WireMockServer _server;

    public string Url => _server.Url!;

    public JulesMockServer()
    {
        _server = WireMockServer.Start();
    }

    /// <summary>
    /// Configures the mock to return a successful session creation response.
    /// </summary>
    public JulesMockServer GivenSessionIsCreated()
    {
        var json = File.ReadAllText(Path.Combine("[internal metadata: TestData scrubbed]", "Jules", "session_created.json"));

        _server
            .Given(Request.Create()
                .WithPath("/v1alpha/sessions")
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(json));

        return this;
    }

    /// <summary>
    /// Configures the mock to return the rich activity list for a specific session.
    /// </summary>
    public JulesMockServer GivenActivitiesExist(string sessionId)
    {
        var json = File.ReadAllText(Path.Combine("[internal metadata: TestData scrubbed]", "Jules", "activities_list.json"));

        _server
            .Given(Request.Create()
                .WithPath($"/v1alpha/sessions/{sessionId}/activities")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(json));

        return this;
    }

    /// <summary>
    /// Configures the mock to return a 401 Unauthorized error (Simulation of bad key).
    /// </summary>
    public JulesMockServer GivenUnauthenticated()
    {
        _server
            .Given(Request.Create())
            .RespondWith(Response.Create()
                .WithStatusCode(401)
                .WithBody("{\"error\": \"Invalid API Key\"}"));

        return this;
    }

    public void Dispose()
    {
        _server.Stop();
        _server.Dispose();
    }
}
