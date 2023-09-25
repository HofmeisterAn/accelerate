namespace Accelerate;

public sealed class Execute : IHostedService
{
    private readonly IEnumerable<string> _args;

    private readonly Campaign _campaign;

    public Execute(ArgCollection args, Campaign campaign)
    {
        _args = args;
        _campaign = campaign;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var parsedArgs = Parser.Default.ParseArguments<InitVerb, CloneVerb, CommitVerb, PushVerb, ForeachVerb>(_args);
        var initTask = parsedArgs.WithParsedAsync<InitVerb>(verb => _campaign.InitAsync(verb.Name, cancellationToken));
        var cloneTask = parsedArgs.WithParsedAsync<CloneVerb>(_ => _campaign.CloneAsync(cancellationToken));
        var commitTask = parsedArgs.WithParsedAsync<CommitVerb>(verb => _campaign.CommitAsync(verb.Message, cancellationToken));
        var pushTask = parsedArgs.WithParsedAsync<PushVerb>(_ => _campaign.PushAsync(cancellationToken));
        var foreachTask = parsedArgs.WithParsedAsync<ForeachVerb>(verb => Task.CompletedTask);
        return Task.WhenAll(initTask, cloneTask, commitTask, pushTask, foreachTask);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}