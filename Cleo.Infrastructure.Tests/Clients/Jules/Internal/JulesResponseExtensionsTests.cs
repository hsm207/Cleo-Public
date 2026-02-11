using System.Net;
using Cleo.Core.Domain.Exceptions;
using Cleo.Infrastructure.Clients.Jules.Internal;
using FluentAssertions;
using Xunit;

namespace Cleo.Infrastructure.Tests.Clients.Jules.Internal;

public class JulesResponseExtensionsTests
{
    [Fact(DisplayName = "Given a successful response, EnsureSuccessWithDetailAsync should complete successfully.")]
    public async Task EnsureSuccess_WithSuccess_Completes()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        // Act & Assert
        await response.EnsureSuccessWithDetailAsync(CancellationToken.None);
    }

    [Fact(DisplayName = "Given a failure response with body, EnsureSuccessWithDetailAsync should throw RemoteCollaboratorUnavailableException containing details.")]
    public async Task EnsureSuccess_WithFailure_ThrowsWithDetails()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("{\"error\": \"Invalid input\"}")
        };

        // Act
        var act = async () => await response.EnsureSuccessWithDetailAsync(CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<RemoteCollaboratorUnavailableException>();
        ex.WithMessage("*400*");
        ex.WithMessage("*Invalid input*");
    }

    [Fact(DisplayName = "Given a failure response without body, EnsureSuccessWithDetailAsync should throw with status code.")]
    public async Task EnsureSuccess_WithFailureNoBody_ThrowsWithStatusCode()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        // No content

        // Act
        var act = async () => await response.EnsureSuccessWithDetailAsync(CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<RemoteCollaboratorUnavailableException>();
        ex.WithMessage("*500*");
    }
}
