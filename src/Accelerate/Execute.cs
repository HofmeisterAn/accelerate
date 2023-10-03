namespace Accelerate;

public sealed class Execute : IHostedService
{
    private readonly IReadOnlyList<string> _args;

    private readonly Campaign _campaign;

    public Execute(ArgCollection args, Campaign campaign)
    {
        _args = args;
        _campaign = campaign;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // The CommandLineParser does not support an option that matches everything after the verb.
        var isForeachVerb = _args.Count > 0 && "foreach".Equals(_args[0], StringComparison.Ordinal);
        var parsedArgs = Parser.Default.ParseArguments<InitVerb, CloneVerb, CommitVerb, PushVerb, CreatePullRequests, ForeachVerb>(isForeachVerb ? _args.Take(1) : _args);
        var initTask = parsedArgs.WithParsedAsync<InitVerb>(verb => _campaign.InitAsync(verb.Name, cancellationToken));
        var cloneTask = parsedArgs.WithParsedAsync<CloneVerb>(_ => _campaign.CloneAsync(cancellationToken));
        var commitTask = parsedArgs.WithParsedAsync<CommitVerb>(verb => _campaign.CommitAsync(verb.Message, cancellationToken));
        var pushTask = parsedArgs.WithParsedAsync<PushVerb>(_ => _campaign.PushAsync(cancellationToken));
        var createPrTask = parsedArgs.WithParsedAsync<CreatePullRequests>(_ => _campaign.CreatePullRequestAsync(cancellationToken));
        var foreachTask = parsedArgs.WithParsedAsync<ForeachVerb>(_ => _campaign.ForeachAsync(_args.Skip(1), cancellationToken));

        try
        {
            await Task.WhenAll(initTask, cloneTask, commitTask, pushTask, createPrTask, foreachTask);
        }
        catch (AccelerateException e)
        {
            Environment.Exit((int)e.ErrorCode);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}