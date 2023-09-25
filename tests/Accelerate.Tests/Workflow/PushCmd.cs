namespace Accelerate.Tests.Workflow;

public sealed class PushCmd : IAsyncLifetime, IDisposable
{
    private readonly IHost _host;

    public PushCmd(CampaignId campaignId, IGitCommand<AzureDevOps> azureDevOpsService)
    {
        _host = new AccelerateHostBuilder("push").ConfigureTestServices(azureDevOpsService).Build();
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