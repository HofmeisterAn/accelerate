namespace Accelerate.Repositories;

public sealed class AzureDevOps : Repository
{
    public AzureDevOps(Uri url) : base(url)
    {
    }

    public string Organization => Url.Segments[1].TrimEnd('/');

    public string Project => Url.Segments[2].TrimEnd('/');

    public string Name => Url.Segments[4].TrimEnd('/');

    public sealed class Service : IGitCommand<AzureDevOps>, IShellCommand<AzureDevOps>
    {
        private const string GitCli = "git";

        private readonly IOptions<ShellSettings> _shellSettings;

        private readonly IOptions<AzureDevOpsSettings> _azureDevOpsSettings;

        private readonly ILogger<Service> _logger;

        public Service(IOptions<ShellSettings> shellSettings, IOptions<AzureDevOpsSettings> azureDevOpsSettings, ILogger<Service> logger)
        {
            _shellSettings = shellSettings;
            _azureDevOpsSettings = azureDevOpsSettings;
            _logger = logger;
        }

        public Task CloneAsync(Campaign campaign, AzureDevOps repository, CancellationToken ct = default)
        {
            _logger.LogInformation("Clone {Repository}", repository.Url);
            var workDir = Path.Combine(campaign.WorkingDirectoryPath, repository.WorkingDirectoryPath);
            var args = new[] { "clone", repository.Url.ToString(), "." };
            _ = Directory.CreateDirectory(workDir);
            return Cli.Wrap(GitCli).WithWorkingDirectory(workDir).WithArguments(args).WithValidation(CommandResultValidation.None).ExecuteAsync(ct);
        }

        public Task CheckoutAsync(Campaign campaign, AzureDevOps repository, CancellationToken ct = default)
        {
            _logger.LogInformation("Create branch {Campaign}", campaign.Name);
            var workDir = Path.Combine(campaign.WorkingDirectoryPath, repository.WorkingDirectoryPath);
            var args = new[] { "checkout", "-b", campaign.Name, "--track" };
            return Cli.Wrap(GitCli).WithWorkingDirectory(workDir).WithArguments(args).WithValidation(CommandResultValidation.None).ExecuteAsync(ct);
        }

        public Task CommitAsync(Campaign campaign, AzureDevOps repository, string message, CancellationToken ct = default)
        {
            _logger.LogInformation("Commit campaign {Campaign}", campaign.Name);
            var workDir = Path.Combine(campaign.WorkingDirectoryPath, repository.WorkingDirectoryPath);
            var args = new[] { "commit", "--all", "--message", message };
            return Cli.Wrap(GitCli).WithWorkingDirectory(workDir).WithArguments(args).WithValidation(CommandResultValidation.None).ExecuteAsync(ct);
        }

        public Task PushAsync(Campaign campaign, AzureDevOps repository, CancellationToken ct = default)
        {
            _logger.LogInformation("Push campaign {Campaign}", campaign.Name);
            var workDir = Path.Combine(campaign.WorkingDirectoryPath, repository.WorkingDirectoryPath);
            var args = new[] { "push", "--set-upstream", "origin", campaign.Name };
            return Cli.Wrap(GitCli).WithWorkingDirectory(workDir).WithArguments(args).WithValidation(CommandResultValidation.None).ExecuteAsync(ct);
        }

        public async Task CreatePullRequestsAsync(Campaign campaign, AzureDevOps repository, string title, string description, CancellationToken ct = default)
        {
            _logger.LogInformation("Create pull request");

            var workDir = Path.Combine(campaign.WorkingDirectoryPath, repository.WorkingDirectoryPath);
            var args = new[] { "symbolic-ref", "refs/remotes/origin/HEAD" };

            var commandResult = await Cli.Wrap(GitCli)
                .WithWorkingDirectory(workDir)
                .WithArguments(args)
                .ExecuteBufferedAsync(ct);

            if (commandResult.ExitCode != 0)
            {
                _logger.LogError(commandResult.StandardError);
                return;
            }

            const string api = "https://dev.azure.com/{0}/{1}/_apis/git/repositories/{2}/pullrequests?api-version=7.0";
            var requestUri = new Uri(string.Format(api, repository.Organization, repository.Project, repository.Name));

            const string refsHeads = "refs/heads/";
            var sourceRefName = campaign.Name;
            var targetRefName = commandResult.StandardOutput.Trim().Split('/').Last();

            var prRequestBody = new Dictionary<string, string>();
            prRequestBody.Add("sourceRefName", refsHeads + sourceRefName);
            prRequestBody.Add("targetRefName", refsHeads + targetRefName);
            prRequestBody.Add("title", title);
            prRequestBody.Add("description", description);

            var parameter = Convert.ToBase64String(Encoding.Default.GetBytes(":" + _azureDevOpsSettings.Value.AuthToken));

            var jsonString = JsonSerializer.Serialize(prRequestBody);

            using var httpClient = new HttpClient();

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", parameter);
            httpRequest.Content = new StringContent(jsonString, Encoding.Default, "application/json");

            using var httpResponse = await httpClient.SendAsync(httpRequest, ct);
        }

        public Task ForeachAsync(Campaign campaign, AzureDevOps repository, IEnumerable<string> command, CancellationToken ct = default)
        {
            _logger.LogInformation("Execute command in campaign {Campaign}", campaign.Name);
            var workDir = Path.Combine(campaign.WorkingDirectoryPath, repository.WorkingDirectoryPath);
            return Cli.Wrap(_shellSettings.Value.Shell).WithWorkingDirectory(workDir).WithArguments(new [] { "-c", string.Join(' ', command) }).ExecuteAsync(ct);
        }
    }
}