namespace Cleo.Core.Domain.Exceptions;

/// <summary>
/// Thrown when the remote engineering collaborator (e.g., Jules) is unreachable.
/// </summary>
public class RemoteCollaboratorUnavailableException : Exception
{
    public RemoteCollaboratorUnavailableException() : base("Remote collaborator is currently unreachable.") { }

    public RemoteCollaboratorUnavailableException(string message) : base(message) { }

    public RemoteCollaboratorUnavailableException(string message, Exception innerException) : base(message, innerException) { }
}
