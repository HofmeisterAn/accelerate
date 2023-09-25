namespace Accelerate.Tests.Workflow;

public sealed class WorkflowTest : IGitCommand<AzureDevOps>
{
    private readonly Type[] _cmdTypes = { typeof(InitCmd), typeof(CloneCmd), typeof(CommitCmd), typeof(PushCmd) };

    private readonly CampaignId _campaignId = new CampaignId();

    [Fact]
    public async Task Should_Execute_Workflow_Successfully()
    {
        // We use reflection to generate an instance of each command, thus replicating an
        // entire Accelerate workflow. This encompasses a broad set of features without becoming
        // entangled in meticulously testing every detail of the CLI commands. In the future, we
        // can utilize this approach to execute commands against an actual available
        // repository. However, for the time being, we are simulating the Git CLI command calls to
        // maintain simplicity.
        foreach (var cmdType in _cmdTypes)
        {
            if (Activator.CreateInstance(cmdType, _campaignId, this) is not IAsyncLifetime cmdInstance)
            {
                continue;
            }

            await cmdInstance.InitializeAsync()
                .ConfigureAwait(false);

            await cmdInstance.DisposeAsync()
                .ConfigureAwait(false);
        }

        Assert.True(File.Exists(Campaign.ReadmeFileName));
        Assert.True(File.Exists(Campaign.ReposFileName));
    }

    Task IGitCommand<AzureDevOps>.CloneAsync(Campaign campaign, AzureDevOps repository, CancellationToken ct)
    {
        Assert.Equal(_campaignId.Id, campaign.Name);
        return Task.CompletedTask;
    }

    Task IGitCommand<AzureDevOps>.CheckoutAsync(Campaign campaign, AzureDevOps repository, CancellationToken ct)
    {
        Assert.Equal(_campaignId.Id, campaign.Name);
        return Task.CompletedTask;
    }

    Task IGitCommand<AzureDevOps>.CommitAsync(Campaign campaign, AzureDevOps repository, string commit, CancellationToken ct)
    {
        Assert.Equal(_campaignId.Id, campaign.Name);
        return Task.CompletedTask;
    }

    Task IGitCommand<AzureDevOps>.PushAsync(Campaign campaign, AzureDevOps repository, CancellationToken ct)
    {
        Assert.Equal(_campaignId.Id, campaign.Name);
        return Task.CompletedTask;
    }
}