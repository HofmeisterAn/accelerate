namespace Accelerate.Repositories;

public sealed class AzureDevOps : Repository
{
    public AzureDevOps(Uri url) : base(url)
    {
    }

    public sealed class Service : IGitCommand<AzureDevOps>, IShellCommand<AzureDevOps>
    {
        private const string GitCli = "git";

        private readonly ILogger<Service> _logger;

        public Service(ILogger<Service> logger)
        {
            _logger = logger;
        }

        public Task CloneAsync(Campaign campaign, AzureDevOps repository, CancellationToken ct = default)
        {
            _logger.LogInformation("Clone {Repository}", repository.Url);
            var workDir = Path.Combine(campaign.WorkingDirectoryPath, repository.WorkingDirectoryPath);
            var args = new[] { "clone", repository.Url.ToString(), "." };
            _ = Directory.CreateDirectory(workDir);
            return Cli.Wrap(GitCli).WithWorkingDirectory(workDir).WithArguments(args).ExecuteAsync(ct);
        }

        public Task CheckoutAsync(Campaign campaign, AzureDevOps repository, CancellationToken ct = default)
        {
            _logger.LogInformation("Create campaign {Campaign}", campaign.Name);
            var workDir = Path.Combine(campaign.WorkingDirectoryPath, repository.WorkingDirectoryPath);
            var args = new[] { "checkout", "-b", campaign.Name, "--track" };
            return Cli.Wrap(GitCli).WithWorkingDirectory(workDir).WithArguments(args).ExecuteAsync(ct);
        }

        public Task CommitAsync(Campaign campaign, AzureDevOps repository, string message, CancellationToken ct = default)
        {
            _logger.LogInformation("Commit campaign {Campaign}", campaign.Name);
            var workDir = Path.Combine(campaign.WorkingDirectoryPath, repository.WorkingDirectoryPath);
            var args = new[] { "commit", "--all", "--message", message };
            return Cli.Wrap(GitCli).WithWorkingDirectory(workDir).WithArguments(args).ExecuteAsync(ct);
        }

        public Task PushAsync(Campaign campaign, AzureDevOps repository, CancellationToken ct = default)
        {
            _logger.LogInformation("Push campaign {Campaign}", campaign.Name);
            var workDir = Path.Combine(campaign.WorkingDirectoryPath, repository.WorkingDirectoryPath);
            var args = new[] { "push" };
            return Cli.Wrap(GitCli).WithWorkingDirectory(workDir).WithArguments(args).ExecuteAsync(ct);
        }

        public Task ForeachAsync(Campaign campaign, AzureDevOps repository, IEnumerable<string> command, CancellationToken ct = default)
        {
            var accelerateShell = Environment.GetEnvironmentVariable("ACCELERATE_SHELL");
            var shell = string.IsNullOrWhiteSpace(accelerateShell) ? RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "PowerShell" : "sh" : accelerateShell;
            var workDir = Path.Combine(campaign.WorkingDirectoryPath, repository.WorkingDirectoryPath);
            return Cli.Wrap(shell).WithWorkingDirectory(workDir).WithArguments(command).ExecuteAsync(ct);
        }
    }
}