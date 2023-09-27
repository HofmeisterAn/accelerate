namespace Accelerate.Commands;

public interface IShellCommand<in TRepository>
{
    public Task ForeachAsync(Campaign campaign, AzureDevOps repository, IEnumerable<string> command, CancellationToken ct = default);
}