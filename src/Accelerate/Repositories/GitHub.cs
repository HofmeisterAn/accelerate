namespace Accelerate.Repositories;

public sealed class GitHub : Repository
{
    public GitHub(Uri url) : base(url)
    {
    }

    public sealed class Service : IGitCommand<GitHub>, IShellCommand<AzureDevOps>
    {
        public Task<bool> CloneAsync(Campaign campaign, GitHub repository, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CheckoutAsync(Campaign campaign, GitHub repository, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CommitAsync(Campaign campaign, GitHub repository, string commit, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PushAsync(Campaign campaign, GitHub repository, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CreatePullRequestsAsync(Campaign campaign, GitHub repository, string title, string description, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ForeachAsync(Campaign campaign, AzureDevOps repository, IEnumerable<string> command, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}