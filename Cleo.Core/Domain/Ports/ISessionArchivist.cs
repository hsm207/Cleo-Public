using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Ports;

/// <summary>
/// The guardian of the Session Log. Responsible for local storage and retrieval of session history.
/// </summary>
public interface ISessionArchivist : IHistoryReader, IHistoryWriter
{
}
