namespace Accelerate.Repositories;

public sealed class GitHub : Repository
{
    public GitHub(Uri url) : base(url)
    {
    }

    public sealed class Service : IGitCommand<GitHub>
    {
        public Task CloneAsync(Campaign campaign, GitHub repository, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task CheckoutAsync(Campaign campaign, GitHub repository, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task CommitAsync(Campaign campaign, GitHub repository, string commit, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task PushAsync(Campaign campaign, GitHub repository, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}