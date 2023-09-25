namespace Accelerate.Verbs;

[Verb("commit", HelpText = "Commit changes to Git repositories associated with the campaign.")]
public sealed record CommitVerb([property: Option("message", Required = true, HelpText = "The Git commit message.")] string Message)
{
    public CommitVerb() : this(string.Empty)
    {
    }
}