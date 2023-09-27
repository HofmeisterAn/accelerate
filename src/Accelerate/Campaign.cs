namespace Accelerate;

public sealed class Campaign
{
    public const string ReadmeFileName = "README.md";

    public const string ReposFileName = "repos.json";

    private readonly IGitCommand<AzureDevOps> _gitCommand;

    private readonly IShellCommand<AzureDevOps> _shellCommand;

    private readonly IReadOnlyList<AzureDevOps> _repositories;

    public Campaign(IGitCommand<AzureDevOps> gitCommand, IShellCommand<AzureDevOps> shellCommand)
    {
        // TODO: Support other Git hosting platforms too.
        _gitCommand = gitCommand;
        _shellCommand = shellCommand;

        if (File.Exists(ReadmeFileName) && File.Exists(ReposFileName))
        {
            _repositories = new RepositoryCollection(File.ReadAllText(ReposFileName)).OfType<AzureDevOps>().ToImmutableList();
        }
        else
        {
            _repositories = Array.Empty<AzureDevOps>();
        }
    }

    public string WorkingDirectoryName => "work";

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

    public Task ForeachAsync(IEnumerable<string> command, CancellationToken ct = default)
    {
        return Task.WhenAll(_repositories.Select(repository => _shellCommand.ForeachAsync(this, repository, command, ct)));
    }
}