namespace Accelerate.Repositories;

public sealed class AzureDevOps : Repository
{
    public AzureDevOps(Uri url) : base(url)
    {
    }

    public string Organization => Url.Segments[1].Trim('/');

    public string Project => Url.Segments[2].Trim('/');

    public string Name => Url.Segments[4].Trim('/');

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

        public async Task<bool> CloneAsync(Campaign campaign, AzureDevOps repository, CancellationToken ct = default)
        {
            var workDir = Path.Combine(campaign.WorkingDirectoryPath, repository.WorkingDirectoryPath);
            var args = new[] { "clone", repository.Url.ToString(), "." };
            _ = Directory.CreateDirectory(workDir);
            var commandResult = await Cli.Wrap(GitCli).WithWorkingDirectory(workDir).WithArguments(args).WithValidation(CommandResultValidation.None).ExecuteBufferedAsync(ct);
            return IsSuccess(commandResult);
        }

        public async Task<bool> CheckoutAsync(Campaign campaign, AzureDevOps repository, CancellationToken ct = default)
        {
            var workDir = Path.Combine(campaign.WorkingDirectoryPath, repository.WorkingDirectoryPath);
            var args = new[] { "checkout", "-b", campaign.Name, "--track" };
            var commandResult = await Cli.Wrap(GitCli).WithWorkingDirectory(workDir).WithArguments(args).WithValidation(CommandResultValidation.None).ExecuteBufferedAsync(ct);
            return IsSuccess(commandResult);
        }

        public async Task<bool> CommitAsync(Campaign campaign, AzureDevOps repository, string message, CancellationToken ct = default)
        {
            var workDir = Path.Combine(campaign.WorkingDirectoryPath, repository.WorkingDirectoryPath);
            var args = new[] { "commit", "--all", "--message", message };
            var commandResult = await Cli.Wrap(GitCli).WithWorkingDirectory(workDir).WithArguments(args).WithValidation(CommandResultValidation.None).ExecuteBufferedAsync(ct);
            return IsSuccess(commandResult);
        }

        public async Task<bool> PushAsync(Campaign campaign, AzureDevOps repository, CancellationToken ct = default)
        {
            var workDir = Path.Combine(campaign.WorkingDirectoryPath, repository.WorkingDirectoryPath);
            var args = new[] { "push", "--set-upstream", "origin", campaign.Name };
            var commandResult = await Cli.Wrap(GitCli).WithWorkingDirectory(workDir).WithArguments(args).WithValidation(CommandResultValidation.None).ExecuteBufferedAsync(ct);
            return IsSuccess(commandResult);
        }

        public async Task<bool> CreatePullRequestsAsync(Campaign campaign, AzureDevOps repository, string title, string description, CancellationToken ct = default)
        {
            var workDir = Path.Combine(campaign.WorkingDirectoryPath, repository.WorkingDirectoryPath);
            var args = new[] { "symbolic-ref", "refs/remotes/origin/HEAD" };

            var commandResult = await Cli.Wrap(GitCli)
                .WithWorkingDirectory(workDir)
                .WithArguments(args)
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync(ct);

            if (commandResult.ExitCode != 0)
            {
                _logger.LogError(commandResult.StandardError);
                return false;
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
            return true;
        }

        public async Task<bool> ForeachAsync(Campaign campaign, AzureDevOps repository, IEnumerable<string> command, CancellationToken ct = default)
        {
            var workDir = Path.Combine(campaign.WorkingDirectoryPath, repository.WorkingDirectoryPath);
            var args = new[] { "-c", string.Join(' ', command) };
            var commandResult = await Cli.Wrap(_shellSettings.Value.Shell).WithWorkingDirectory(workDir).WithArguments(args).WithValidation(CommandResultValidation.None).ExecuteBufferedAsync(ct);
            return IsSuccess(commandResult);
        }

        private bool IsSuccess(BufferedCommandResult commandResult)
        {
            if (0.Equals(commandResult.ExitCode))
            {
                return true;
            }
            else
            {
                _logger.LogError(commandResult.StandardError);
                return false;
            }
        }
    }
}