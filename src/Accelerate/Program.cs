using var host = new AccelerateHostBuilder(args).Build();
await host.StartAsync();
await host.StopAsync();

public class AccelerateHostBuilder : HostBuilder
{
    public AccelerateHostBuilder(params string[] args)
    {
        ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.SetBasePath(Path.GetDirectoryName(GetType().Assembly.Location));
        })
        .ConfigureServices((hostBuilder, serviceCollection) =>
        {
            var argCollection = new ArgCollection(args);
            serviceCollection.AddHostedService<Execute>();
            serviceCollection.AddSingleton(argCollection);
            serviceCollection.AddSingleton<Campaign>();
            serviceCollection.Configure<ShellSettings>(hostBuilder.Configuration.GetSection(nameof(ShellSettings)));
            serviceCollection.Configure<AzureDevOpsSettings>(hostBuilder.Configuration.GetSection(nameof(AzureDevOpsSettings)));

            // TODO: Change the implementation to work with the actual base class `Repository`.
            serviceCollection.AddSingleton<IGitCommand<AzureDevOps>, AzureDevOps.Service>();

            // TODO: Share the foreach implementation across repositories.
            serviceCollection.AddSingleton<IShellCommand<AzureDevOps>, AzureDevOps.Service>();
        })
        .ConfigureDefaults(args);
    }
}