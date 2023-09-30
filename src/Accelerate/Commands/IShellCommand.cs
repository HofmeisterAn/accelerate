namespace Accelerate.Commands;

public interface IShellCommand<in TRepository>
{
    Task<bool> ForeachAsync(Campaign campaign, TRepository repository, IEnumerable<string> command, CancellationToken ct = default);
}