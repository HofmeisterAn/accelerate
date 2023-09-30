namespace Accelerate.Tests.Workflow;

public class Cmd : AccelerateHostBuilder, IAsyncLifetime, IDisposable
{
    public const string PullRequestTitle = "Title";

    public const string PullRequestDescription = "Lorem ipsum dolor sit amet";

    public const string Organization = "organization";

    public const string Project = "project";

    public const string Name = "repository";

    public const string Repo = "https://dev.azure.com/" + Organization + "/" + Project + "/_git/" + Name;

    private const string ReadmeFileContent = "# " + PullRequestTitle + "\n" + PullRequestDescription;

    private const string ReposFileContent = "[{\"type\":\"Accelerate.Repositories.AzureDevOps\",\"repo\":{\"url\":\"" + Repo + "\"}}]";

    private readonly Lazy<IHost> _lazyHost;

    public Cmd(IGitCommand<AzureDevOps> gitCommand, IShellCommand<AzureDevOps> shellCommand, params string[] args) : base(args)
    {
        _lazyHost = new Lazy<IHost>(Build);
        Configure(gitCommand);
        Configure(shellCommand);
    }

    ~Cmd()
    {
        Dispose(false);
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
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _lazyHost.Value.Dispose();
        }
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
            File.WriteAllText(Campaign.ReadmeFileName, ReadmeFileContent);
            File.WriteAllText(Campaign.ReposFileName, ReposFileContent);
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

    public sealed class CreatePullRequests : Cmd
    {
        public CreatePullRequests(CampaignId campaignId, IGitCommand<AzureDevOps> gitCommand, IShellCommand<AzureDevOps> shellCommand)
            : base(gitCommand, shellCommand, "create-prs")
        {
        }
    }
}