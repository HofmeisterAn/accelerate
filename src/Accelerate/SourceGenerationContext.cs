namespace Accelerate;

[JsonSerializable(typeof(Campaign.VisualStudioCodeWorkspace))]
[JsonSerializable(typeof(Campaign.VisualStudioCodeFolder))]
[JsonSerializable(typeof(AzureDevOps))]
[JsonSerializable(typeof(GitHub))]
[JsonSerializable(typeof(Dictionary<string, string>))]
internal partial class SourceGenerationContext : JsonSerializerContext;