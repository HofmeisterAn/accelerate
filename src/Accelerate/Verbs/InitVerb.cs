namespace Accelerate.Verbs;

[Verb("init", HelpText = "Create a new Accelerate campaign.")]
public sealed record InitVerb([property: Option("name", Required = true, HelpText = "The campaign name.")] string Name)
{
    public InitVerb() : this(string.Empty)
    {
    }
}