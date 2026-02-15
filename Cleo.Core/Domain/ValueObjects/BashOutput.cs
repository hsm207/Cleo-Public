namespace Cleo.Core.Domain.ValueObjects;

/// <summary>
/// The result of a shell command execution.
/// </summary>
public record BashOutput : Artifact
{
    public string Command { get; init; }
    public string Output { get; init; }
    public int ExitCode { get; init; }

    public BashOutput(string command, string output, int exitCode)
    {
        ArgumentNullException.ThrowIfNull(command);
        
        if (string.IsNullOrWhiteSpace(command))
        {
            throw new ArgumentException("Command cannot be empty.", nameof(command));
        }

        Command = command;
        Output = output ?? string.Empty;
        ExitCode = exitCode;
    }

    public override string GetSummary() => $"BashOutput: Executed '{Command}' (Exit Code: {ExitCode})";
}
