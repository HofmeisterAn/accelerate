namespace Accelerate.Verbs;

[Verb("foreach", HelpText = "Execute a shell command for each Git repositories associated with the campaign.")]
public sealed record ForeachVerb([property: Value(0)] IEnumerable<string> Command)
{
    public ForeachVerb() : this(Array.Empty<string>())
    {
    }
}