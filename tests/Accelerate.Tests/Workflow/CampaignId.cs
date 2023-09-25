namespace Accelerate.Tests.Workflow;

public sealed class CampaignId
{
    public string Id { get; } = Guid.NewGuid().ToString("D");
}