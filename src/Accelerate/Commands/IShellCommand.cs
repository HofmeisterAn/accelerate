namespace Accelerate.Commands;

public interface IShellCommand<in TRepository>
{
    Task ForeachAsync(Campaign campaign, TRepository repository, IEnumerable<string> command, CancellationToken ct = default);
}