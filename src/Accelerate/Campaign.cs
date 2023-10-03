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
        IEnumerable<bool> results;
        int successful;
        int failure;

        _logger.LogInformation("Cmd=\"{Cmd}\" Status=\"{Status}\"", "Clone", "Running");
        results = await Task.WhenAll(_repositories.Select(repository => _gitCommand.CloneAsync(this, repository, ct)));
        successful = results.Count(result => result);
        failure = results.Count(result => !result);
        _logger.LogInformation("Cmd=\"{Cmd}\" Status=\"{Status}\" Successful={Successful} Failure={Failure}", "Clone", "Completed", successful, failure);

        if (failure > 0)
        {
            throw new AccelerateException(AccelerateErrorCode.GitCommandExecutionFailed);
        }

        _logger.LogInformation("Cmd=\"{Cmd}\" Status=\"{Status}\"", "Checkout", "Running");
        results = await Task.WhenAll(_repositories.Select(repository => _gitCommand.CheckoutAsync(this, repository, ct)));
        successful = results.Count(result => result);
        failure = results.Count(result => !result);
        _logger.LogInformation("Cmd=\"{Cmd}\" Status=\"{Status}\" Successful={Successful} Failure={Failure}", "Checkout", "Completed", successful, failure);

        if (failure > 0)
        {
            throw new AccelerateException(AccelerateErrorCode.GitCommandExecutionFailed);
        }
    }

    public async Task CommitAsync(string message, CancellationToken ct = default)
    {
        _logger.LogInformation("Cmd=\"{Cmd}\" Status=\"{Status}\"", "Commit", "Running");
        var results = await Task.WhenAll(_repositories.Select(repository => _gitCommand.CommitAsync(this, repository, message, ct)));
        var successful = results.Count(result => result);
        var failure = results.Count(result => !result);
        _logger.LogInformation("Cmd=\"{Cmd}\" Status=\"{Status}\" Successful={Successful} Failure={Failure}", "Commit", "Completed", successful, failure);

        if (failure > 0)
        {
            throw new AccelerateException(AccelerateErrorCode.GitCommandExecutionFailed);
        }
    }

    public async Task PushAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Cmd=\"{Cmd}\" Status=\"{Status}\"", "Push", "Running");
        var results = await Task.WhenAll(_repositories.Select(repository => _gitCommand.PushAsync(this, repository, ct)));
        var successful = results.Count(result => result);
        var failure = results.Count(result => !result);
        _logger.LogInformation("Cmd=\"{Cmd}\" Status=\"{Status}\" Successful={Successful} Failure={Failure}", "Push", "Completed", successful, failure);

        if (failure > 0)
        {
            throw new AccelerateException(AccelerateErrorCode.GitCommandExecutionFailed);
        }
    }

    public async Task CreatePullRequestAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Cmd=\"{Cmd}\" Status=\"{Status}\"", "Create Pull Requests", "Running");

        if (!File.Exists(ReadmeFileName))
        {
            _logger.LogError("The '" + ReadmeFileName + "' file could not be found. Please make sure that the file exists in the campaign.");
            throw new AccelerateException(AccelerateErrorCode.Undefined);
        }

        MarkdownDocument markdown;

        HeadingBlock heading;

        string title;

        string description;

        var readme = await File.ReadAllTextAsync(ReadmeFileName, ct);

        try
        {
            markdown = Markdown.Parse(readme, true);
            heading = markdown.OfType<HeadingBlock>().First();
            _ = markdown.Remove(heading);
        }
        catch (InvalidOperationException)
        {
            throw new AccelerateException(AccelerateErrorCode.MarkdownDocumentHeadingMissing);
        }
        catch (Exception)
        {
            throw new AccelerateException(AccelerateErrorCode.ParsingMarkdownDocumentFailed);
        }

        using (var stringWriter = new StringWriter())
        {
            var renderer = new RoundtripRenderer(stringWriter);
            renderer.WriteLeafInline(heading);
            title = stringWriter.ToString();
        }

        using (var stringWriter = new StringWriter())
        {
            var renderer = new RoundtripRenderer(stringWriter);
            renderer.Write(markdown);
            description = stringWriter.ToString();
        }

        var results = await Task.WhenAll(_repositories.Select(repository => _gitCommand.CreatePullRequestsAsync(this, repository, title.Trim(), description.Trim(), ct)));
        var successful = results.Count(result => result);
        var failure = results.Count(result => !result);
        _logger.LogInformation("Cmd=\"{Cmd}\" Status=\"{Status}\" Successful={Successful} Failure={Failure}", "Create Pull Requests", "Completed", successful, failure);

        if (failure > 0)
        {
            throw new AccelerateException(AccelerateErrorCode.CreatingPullRequestFailed);
        }
    }

    public async Task ForeachAsync(IEnumerable<string> command, CancellationToken ct = default)
    {
        _logger.LogInformation("Cmd=\"{Cmd}\" Status=\"{Status}\"", "Foreach", "Running");
        var results = await Task.WhenAll(_repositories.Select(repository => _shellCommand.ForeachAsync(this, repository, command, ct)));
        var successful = results.Count(result => result);
        var failure = results.Count(result => !result);
        _logger.LogInformation("Cmd=\"{Cmd}\" Status=\"{Status}\" Successful={Successful} Failure={Failure}", "Foreach", "Completed", successful, failure);

        if (failure > 0)
        {
            throw new AccelerateException(AccelerateErrorCode.GitCommandExecutionFailed);
        }
    }
}