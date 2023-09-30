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

            using var outputStream = File.OpenWrite(Path.Combine(name, resourceName.Replace(embeddedResourcePrefix, string.Empty)));

            await inputStream!.CopyToAsync(outputStream, ct)
                .ConfigureAwait(false);
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
        await Task.WhenAll(_repositories.Select(repository => _gitCommand.CloneAsync(this, repository, ct)))
            .ConfigureAwait(false);

        await Task.WhenAll(_repositories.Select(repository => _gitCommand.CheckoutAsync(this, repository, ct)))
            .ConfigureAwait(false);
    }

    public Task CommitAsync(string message, CancellationToken ct = default)
    {
        return Task.WhenAll(_repositories.Select(repository => _gitCommand.CommitAsync(this, repository, message, ct)));
    }

    public Task PushAsync(CancellationToken ct = default)
    {
        return Task.WhenAll(_repositories.Select(repository => _gitCommand.PushAsync(this, repository, ct)));
    }

    public Task CreatePullRequestAsync(CancellationToken ct = default)
    {
        var readme = File.ReadAllText(ReadmeFileName);

        var markdown = Markdown.Parse(readme);

        var heading = markdown.OfType<HeadingBlock>().First();

        var title = readme.Substring(heading.Span.Start + heading.Level + 1, heading.Span.Length - heading.Level - 1);

        var description = readme.Substring(heading.Span.End + 1, markdown.Span.Length - heading.Span.Length);

        return Task.WhenAll(_repositories.Select(repository => _gitCommand.CreatePullRequestsAsync(this, repository, title.Trim(), description.Trim(), ct)));
    }

    public Task ForeachAsync(IEnumerable<string> command, CancellationToken ct = default)
    {
        return Task.WhenAll(_repositories.Select(repository => _shellCommand.ForeachAsync(this, repository, command, ct)));
    }
}