namespace Accelerate.Tests.Workflow;

public sealed class InitCmd : IAsyncLifetime, IDisposable
{
    private readonly IHost _host;

    public InitCmd(CampaignId campaignId, IGitCommand<AzureDevOps> azureDevOpsService)
    {
        _host = new AccelerateHostBuilder("init", "--name", campaignId.Id).ConfigureTestServices(azureDevOpsService).Build();
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