namespace Accelerate.Commands;

public interface IGitCommand<in TRepository>
{
    Task CloneAsync(Campaign campaign, TRepository repository, CancellationToken ct = default);

    Task CheckoutAsync(Campaign campaign, TRepository repository, CancellationToken ct = default);

    Task CommitAsync(Campaign campaign, TRepository repository, string commit, CancellationToken ct = default);

    Task PushAsync(Campaign campaign, TRepository repository, CancellationToken ct = default);

    Task CreatePullRequestsAsync(Campaign campaign, TRepository repository, string title, string description, CancellationToken ct = default);
}