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

    public JulesMockServer GivenActivitiesAreEmpty(string sessionId)
    {
        _server
            .Given(Request.Create()
                .WithPath($"/v1alpha/sessions/{sessionId}/activities")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{}"));

        return this;
    }

    public JulesMockServer GivenSessionPulseExists(string sessionId, string state)
    {
        var json = $$"""{ "state": "{{state}}", "name": "sessions/{{sessionId}}" }""";

        _server
            .Given(Request.Create()
                .WithPath($"/v1alpha/sessions/{sessionId}")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(json));

        return this;
    }

    public JulesMockServer GivenSessionPulseReturnsNull(string sessionId)
    {
        _server
            .Given(Request.Create()
                .WithPath($"/v1alpha/sessions/{sessionId}")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("null"));

        return this;
    }

    public JulesMockServer GivenMessageCanBeSent(string sessionId)
    {
        _server
            .Given(Request.Create()
                .WithPath($"/v1alpha/sessions/{sessionId}:sendMessage")
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("{}"));

        return this;
    }

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
