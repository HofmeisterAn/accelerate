namespace Accelerate;

public sealed class Campaign
{
    public const string ReadmeFileName = "README.md";

    public const string ReposFileName = "repos.json";

    private readonly IGitCommand<AzureDevOps> _azureDevOpsService;

    private readonly IGitCommand<GitHub> _gitHubService;

    private readonly IReadOnlyList<AzureDevOps> _repositories;

    public Campaign(IGitCommand<AzureDevOps> azureDevOpsService, IGitCommand<GitHub> gitHubService)
    {
        RepositoryCollection repositoryCollection;

        _azureDevOpsService = azureDevOpsService;
        _gitHubService = gitHubService;

        if (File.Exists(ReadmeFileName) && File.Exists(ReposFileName))
        {
            repositoryCollection = new RepositoryCollection(File.ReadAllText(ReposFileName));
        }
        else
        {
            repositoryCollection = new RepositoryCollection("[]");
        }

        // TODO: Change the implementation to work with the actual base class, `Repository`.
        _repositories = repositoryCollection.OfType<AzureDevOps>().ToImmutableList();
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
        await Task.WhenAll(_repositories.Select(repository => _azureDevOpsService.CloneAsync(this, repository, ct)))
            .ConfigureAwait(false);

        await Task.WhenAll(_repositories.Select(repository => _azureDevOpsService.CheckoutAsync(this, repository, ct)))
            .ConfigureAwait(false);
    }

    public async Task CommitAsync(string message, CancellationToken ct = default)
    {
        await Task.WhenAll(_repositories.Select(repository => _azureDevOpsService.CommitAsync(this, repository, message, ct)))
            .ConfigureAwait(false);
    }

    public async Task PushAsync(CancellationToken ct = default)
    {
        await Task.WhenAll(_repositories.Select(repository => _azureDevOpsService.PushAsync(this, repository, ct)))
            .ConfigureAwait(false);
    }
}