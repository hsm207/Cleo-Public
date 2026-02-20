namespace Cleo.Cli.Aesthetics;

/// <summary>
/// Defines the authoritative visual aesthetic for the Cleo CLI.
/// Decouples the logic from specific emoji characters and formatting strings.
/// </summary>
internal static class CliAesthetic
{
    public const string ThoughtBubble = "💭";
    public const string ArtifactBox = "📦";

    public const string SuccessEmoji = "✅";
    public const string ProgressEmoji = "⏳";
    public const string IteratingEmoji = "🔄";
    public const string StalledEmoji = "🛑";

    public const string Indent = "                  "; // Exactly 18 spaces for label alignment 📏✨
}
