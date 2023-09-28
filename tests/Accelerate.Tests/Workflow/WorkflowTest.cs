namespace Accelerate.Tests.Workflow;

public sealed class WorkflowTest : IGitCommand<AzureDevOps>, IShellCommand<AzureDevOps>, IDisposable
{
    private readonly IList<string> _actualCmds = new List<string>();

    private readonly CampaignId _campaignId = new CampaignId();

    [Fact]
    public async Task Should_Execute_Workflow_Successfully()
    {
        // Given
        IList<Cmd> cmds = new List<Cmd>();
        cmds.Add(new Cmd.Init(_campaignId, this, this));
        cmds.Add(new Cmd.Clone(_campaignId, this, this));
        cmds.Add(new Cmd.Foreach(_campaignId, this, this));
        cmds.Add(new Cmd.Commit(_campaignId, this, this));
        cmds.Add(new Cmd.Push(_campaignId, this, this));
        cmds.Add(new Cmd.CreatePullRequests(_campaignId, this, this));

        // When
        for (var step = 0; step < cmds.Count; step++)
        {
            using var cmd = cmds[step];

            await cmd.InitializeAsync()
                .ConfigureAwait(false);

            await cmd.DisposeAsync()
                .ConfigureAwait(false);
        }

        // Then
        Assert.True(File.Exists(Campaign.ReadmeFileName));
        Assert.True(File.Exists(Campaign.ReposFileName));
        Assert.Equivalent(typeof(IGitCommand<Repository>).GetMethods().Select(methodInfo => methodInfo.Name), _actualCmds);
        Assert.Equivalent(typeof(IShellCommand<Repository>).GetMethods().Select(methodInfo => methodInfo.Name), _actualCmds);
    }

    Task IGitCommand<AzureDevOps>.CloneAsync(Campaign campaign, AzureDevOps repository, CancellationToken ct)
    {
        _actualCmds.Add(nameof(IGitCommand<Repository>.CloneAsync));
        Assert.Equal(_campaignId.Id, campaign.Name);
        return Task.CompletedTask;
    }

    Task IGitCommand<AzureDevOps>.CheckoutAsync(Campaign campaign, AzureDevOps repository, CancellationToken ct)
    {
        _actualCmds.Add(nameof(IGitCommand<Repository>.CheckoutAsync));
        Assert.Equal(_campaignId.Id, campaign.Name);
        return Task.CompletedTask;
    }

    Task IGitCommand<AzureDevOps>.CommitAsync(Campaign campaign, AzureDevOps repository, string commit, CancellationToken ct)
    {
        _actualCmds.Add(nameof(IGitCommand<Repository>.CommitAsync));
        Assert.Equal(_campaignId.Id, campaign.Name);
        return Task.CompletedTask;
    }

    Task IGitCommand<AzureDevOps>.PushAsync(Campaign campaign, AzureDevOps repository, CancellationToken ct)
    {
        _actualCmds.Add(nameof(IGitCommand<Repository>.PushAsync));
        Assert.Equal(_campaignId.Id, campaign.Name);
        return Task.CompletedTask;
    }

    Task IGitCommand<AzureDevOps>.CreatePullRequestsAsync(Campaign campaign, AzureDevOps repository, string title, string description, CancellationToken ct)
    {
        _actualCmds.Add(nameof(IGitCommand<Repository>.CreatePullRequestsAsync));
        Assert.Equal(_campaignId.Id, campaign.Name);
        return Task.CompletedTask;
    }

    Task IShellCommand<AzureDevOps>.ForeachAsync(Campaign campaign, AzureDevOps repository, IEnumerable<string> command, CancellationToken ct)
    {
        _actualCmds.Add(nameof(IShellCommand<Repository>.ForeachAsync));
        Assert.Equal(_campaignId.Id, campaign.Name);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (Directory.GetCurrentDirectory().EndsWith(_campaignId.Id))
        {
            Directory.SetCurrentDirectory("..");
            Directory.Delete(_campaignId.Id, true);
        }
    }
}