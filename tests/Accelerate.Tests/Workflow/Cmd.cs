namespace Accelerate.Tests.Workflow;

public class Cmd : AccelerateHostBuilder, IAsyncLifetime, IDisposable
{
    private readonly Lazy<IHost> _lazyHost;

    public Cmd(IGitCommand<AzureDevOps> gitCommand, IShellCommand<AzureDevOps> shellCommand, params string[] args) : base(args)
    {
        _lazyHost = new Lazy<IHost>(Build);
        Configure(gitCommand);
        Configure(shellCommand);
    }

    public virtual Task InitializeAsync()
    {
        return _lazyHost.Value.StartAsync();
    }

    public virtual Task DisposeAsync()
    {
        return _lazyHost.Value.StopAsync();
    }

    public void Dispose()
    {
        _lazyHost.Value.Dispose();
    }

    private void Configure<TService>(TService implementationInstance) where TService : class
    {
        ConfigureServices((_, serviceCollection) => Configure(implementationInstance, serviceCollection));
    }

    private void Configure<TService>(TService implementationInstance, IServiceCollection serviceCollection) where TService : class
    {
        serviceCollection.Remove(serviceCollection.Single(service => typeof(TService) == service.ServiceType));
        serviceCollection.AddSingleton(implementationInstance);
    }

    public sealed class Init : Cmd
    {
        private readonly CampaignId _campaignId;

        public Init(CampaignId campaignId, IGitCommand<AzureDevOps> gitCommand, IShellCommand<AzureDevOps> shellCommand)
            : base(gitCommand, shellCommand, "init", "--name", campaignId.Id)
        {
            _campaignId = campaignId;
        }

        public override Task DisposeAsync()
        {
            Directory.SetCurrentDirectory(_campaignId.Id);
            return base.DisposeAsync();
        }
    }

    public sealed class Clone : Cmd
    {
        public Clone(CampaignId campaignId, IGitCommand<AzureDevOps> gitCommand, IShellCommand<AzureDevOps> shellCommand)
            : base(gitCommand, shellCommand, "clone")
        {
        }
    }

    public sealed class Foreach : Cmd
    {
        public Foreach(CampaignId campaignId, IGitCommand<AzureDevOps> gitCommand, IShellCommand<AzureDevOps> shellCommand)
            : base(gitCommand, shellCommand, "foreach", "New-Item", "-Path", ".", "-ItemType", "File", "-Name", "test.txt")
        {
        }
    }

    public sealed class Commit : Cmd
    {
        public Commit(CampaignId campaignId, IGitCommand<AzureDevOps> gitCommand, IShellCommand<AzureDevOps> shellCommand)
            : base(gitCommand, shellCommand, "commit", "--message", "chore: Run tests")
        {
        }
    }

    public sealed class Push : Cmd
    {
        public Push(CampaignId campaignId, IGitCommand<AzureDevOps> gitCommand, IShellCommand<AzureDevOps> shellCommand)
            : base(gitCommand, shellCommand, "push")
        {
        }
    }
}