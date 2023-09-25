var host = new AccelerateHostBuilder(args).Build();
await host.StartAsync();
await host.StopAsync();

public sealed class AccelerateHostBuilder : HostBuilder
{
    static AccelerateHostBuilder()
    {
        Console.OutputEncoding = Encoding.Unicode;
    }

    public AccelerateHostBuilder(params string[] args)
    {
        ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.SetBasePath(Path.GetDirectoryName(GetType().Assembly.Location));
        })
        .ConfigureServices((_, serviceCollection) =>
        {
            var argCollection = new ArgCollection(args);
            serviceCollection.AddHostedService<Execute>();
            serviceCollection.AddSingleton(argCollection);
            serviceCollection.AddSingleton<Campaign>();
            serviceCollection.AddSingleton<IGitCommand<AzureDevOps>, AzureDevOps.Service>();
            serviceCollection.AddSingleton<IGitCommand<GitHub>, GitHub.Service>();
        })
        .ConfigureDefaults(args);
    }
}