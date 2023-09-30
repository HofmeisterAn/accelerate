namespace Accelerate.Commands;

public interface IGitCommand<in TRepository>
{
    Task<bool> CloneAsync(Campaign campaign, TRepository repository, CancellationToken ct = default);

    Task<bool> CheckoutAsync(Campaign campaign, TRepository repository, CancellationToken ct = default);

    Task<bool> CommitAsync(Campaign campaign, TRepository repository, string commit, CancellationToken ct = default);

    Task<bool> PushAsync(Campaign campaign, TRepository repository, CancellationToken ct = default);

    Task<bool> CreatePullRequestsAsync(Campaign campaign, TRepository repository, string title, string description, CancellationToken ct = default);
}