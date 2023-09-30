namespace Accelerate;

public sealed class Campaign
{
    public const string ReadmeFileName = "README.md";

    public const string ReposFileName = "repos.json";

    private readonly IGitCommand<AzureDevOps> _gitCommand;

    private readonly IShellCommand<AzureDevOps> _shellCommand;

    private readonly IReadOnlyList<AzureDevOps> _repositories;

    private readonly ILogger<Campaign> _logger;

    public Campaign(IGitCommand<AzureDevOps> gitCommand, IShellCommand<AzureDevOps> shellCommand, ILogger<Campaign> logger)
    {
        _gitCommand = gitCommand;
        _shellCommand = shellCommand;
        _logger = logger;

        if (File.Exists(ReadmeFileName) && File.Exists(ReposFileName))
        {
            _repositories = new RepositoryCollection(File.ReadAllText(ReposFileName)).OfType<AzureDevOps>().ToImmutableList();
        }
        else
        {
            _repositories = Array.Empty<AzureDevOps>();
        }
    }

    public string WorkingDirectoryPath => "work";

    public string Name => Directory.GetCurrentDirectory().Split(Path.DirectorySeparatorChar).Last();

    public async Task InitAsync(string name, CancellationToken ct = default)
    {
        const string embeddedResourcePrefix = "Accelerate.Resources.";

        _ = Directory.CreateDirectory(name);

        var assembly = GetType().Assembly;

        foreach (var resourceName in assembly.GetManifestResourceNames())
        {
            using var inputStream = assembly.GetManifestResourceStream(resourceName);

            using var outputStream = File.Create(Path.Combine(name, resourceName.Replace(embeddedResourcePrefix, string.Empty)));

            await inputStream!.CopyToAsync(outputStream, ct);
        }

        var message = new StringBuilder();
        message.Append("Accelerate initialized a new campaign:");
        message.AppendLine();
        message.Append("1. Change the current directory to the campaign \u001b[1m\u001b[32mcd " + name + "\u001b[0m");
        message.AppendLine();
        message.Append("2. Update the " + ReposFileName + " configuration and add the repositories that require changes");
        message.AppendLine();
        message.Append("3. Run the \u001b[1m\u001b[32maccelerate clone\u001b[0m command to clone the repositories");
        _logger.LogInformation(message.ToString());
    }

    public async Task CloneAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Initiating clone process.");
        var results = await Task.WhenAll(_repositories.Select(repository => _gitCommand.CloneAsync(this, repository, ct)));
        var successful = results.Where(result => result);
        var failure = results.Where(result => !result);
        _logger.LogInformation("Clone process completed. Successfully cloned {Successful} repositories with {Failure} failures.", successful.Count(), failure.Count());

        _logger.LogInformation("Initiating checkout process.");
        results = await Task.WhenAll(_repositories.Select(repository => _gitCommand.CheckoutAsync(this, repository, ct)));
        successful = results.Where(result => result);
        failure = results.Where(result => !result);
        _logger.LogInformation("Checkout process completed. Successfully checked out {Successful} branches with {Failure} failures.", successful.Count(), failure.Count());
    }

    public async Task CommitAsync(string message, CancellationToken ct = default)
    {
        _logger.LogInformation("Initiating commit process.");
        var results = await Task.WhenAll(_repositories.Select(repository => _gitCommand.CommitAsync(this, repository, message, ct)));
        var successful = results.Where(result => result);
        var failure = results.Where(result => !result);
        _logger.LogInformation("Commit process completed. Successfully committed to {Successful} repositories with {Failure} failures.", successful.Count(), failure.Count());
    }

    public async Task PushAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Initiating push process.");
        var results = await Task.WhenAll(_repositories.Select(repository => _gitCommand.PushAsync(this, repository, ct)));
        var successful = results.Where(result => result);
        var failure = results.Where(result => !result);
        _logger.LogInformation("Push process completed. Successfully pushed to {Successful} repositories with {Failure} failures.", successful.Count(), failure.Count());
    }

    public async Task CreatePullRequestAsync(CancellationToken ct = default)
    {
        var readme = await File.ReadAllTextAsync(ReadmeFileName, ct);

        var markdown = Markdown.Parse(readme);

        var heading = markdown.OfType<HeadingBlock>().First();

        var title = readme.Substring(heading.Span.Start + heading.Level + 1, heading.Span.Length - heading.Level - 1);

        var description = readme.Substring(heading.Span.End + 1, markdown.Span.Length - heading.Span.Length);

        _logger.LogInformation("Initiating pull request creation.");
        var results = await Task.WhenAll(_repositories.Select(repository => _gitCommand.CreatePullRequestsAsync(this, repository, title.Trim(), description.Trim(), ct)));
        var successful = results.Where(result => result);
        var failure = results.Where(result => !result);
        _logger.LogInformation("Pull request creation completed. Successfully created {Successful} pull requests with {Failure} failures.", successful.Count(), failure.Count());
    }

    public async Task ForeachAsync(IEnumerable<string> command, CancellationToken ct = default)
    {
        _logger.LogInformation("Initiating command execution.");
        var results = await Task.WhenAll(_repositories.Select(repository => _shellCommand.ForeachAsync(this, repository, command, ct)));
        var successful = results.Where(result => result);
        var failure = results.Where(result => !result);
        _logger.LogInformation("Command execution completed. Successfully executed {Successful} commands with {Failure} failures.", successful.Count(), failure.Count());
    }
}