var host = new AccelerateHostBuilder(args).Build();
await host.StartAsync();
await host.StopAsync();

public class AccelerateHostBuilder : HostBuilder
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

            // TODO: Change the implementation to work with the actual base class `Repository`.
            serviceCollection.AddSingleton<IGitCommand<AzureDevOps>, AzureDevOps.Service>();

            // TODO: Share the foreach implementation across repositories.
            serviceCollection.AddSingleton<IShellCommand<AzureDevOps>, AzureDevOps.Service>();
        })
        .ConfigureDefaults(args);
    }
}