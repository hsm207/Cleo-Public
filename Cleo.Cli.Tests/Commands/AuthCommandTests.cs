using Cleo.Cli.Commands;
using Cleo.Core.Domain.Ports;
using Cleo.Core.UseCases.AuthenticateUser;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

public class AuthCommandTests
{
    private readonly AuthCommand _command;

    public AuthCommandTests()
    {
        var useCaseMock = new Mock<IAuthenticateUserUseCase>();
        var credentialStoreMock = new Mock<ICredentialStore>();
        var loggerMock = new Mock<ILogger<AuthCommand>>();

        _command = new AuthCommand(useCaseMock.Object, credentialStoreMock.Object, loggerMock.Object);
    }

    [Fact(DisplayName = "Given the auth command, when viewing help, then it should use the term 'Identity' and 'Vault'.")]
    public void Build_ShouldUseIdentityAndVaultTerm()
    {
        // Act
        var command = _command.Build();

        // Assert
        command.Description.Should().Be("Manage your Identity in the Vault 🔐");
    }
}
