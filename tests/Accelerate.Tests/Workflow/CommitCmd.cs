namespace Accelerate.Tests.Workflow;

public sealed class CommitCmd : IAsyncLifetime, IDisposable
{
    private readonly IHost _host;

    public CommitCmd(CampaignId campaignId, IGitCommand<AzureDevOps> azureDevOpsService)
    {
        _host = new AccelerateHostBuilder("commit", "--message", "chore: Run tests").ConfigureTestServices(azureDevOpsService).Build();
    }

    public Task InitializeAsync()
    {
        return _host.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _host.StopAsync();
    }

    public void Dispose()
    {
        _host.Dispose();
    }
}