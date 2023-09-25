namespace Accelerate.Tests.Workflow;

public sealed class CloneCmd : IAsyncLifetime, IDisposable
{
    private readonly IHost _host;

    public CloneCmd(CampaignId campaignId, IGitCommand<AzureDevOps> azureDevOpsService)
    {
        _host = new AccelerateHostBuilder("clone").ConfigureTestServices(azureDevOpsService).Build();
        Directory.SetCurrentDirectory(campaignId.Id);
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