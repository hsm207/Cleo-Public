using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Ports;

/// <summary>
/// The voice of the developer. Responsible for transmitting feedback and approvals to a live Session.
/// </summary>
public interface ISessionMessenger
{
    Task SendMessageAsync(SessionId id, string message, CancellationToken cancellationToken = default);
}
