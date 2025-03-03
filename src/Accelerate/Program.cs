using var host = new AccelerateHostBuilder(args).Build();
await host.StartAsync();
await host.StopAsync();

public class AccelerateHostBuilder : HostBuilder
{
    public AccelerateHostBuilder(params string[] args)
    {
        ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.SetBasePath(AppContext.BaseDirectory);
        })
        .ConfigureServices((hostBuilder, serviceCollection) =>
        {
            var argCollection = new ArgCollection(args);
            serviceCollection.AddHostedService<Execute>();
            serviceCollection.AddSingleton(argCollection);
            serviceCollection.AddSingleton<Campaign>();
            serviceCollection.Configure<AzureDevOpsSettings>(hostBuilder.Configuration.GetSection(nameof(AzureDevOpsSettings)));
            serviceCollection.Configure<ShellSettings>(hostBuilder.Configuration.GetSection(nameof(ShellSettings)));
            serviceCollection.AddSingleton<IGitCommand<AzureDevOps>, AzureDevOps.Service>();
            serviceCollection.AddSingleton<IShellCommand<AzureDevOps>, AzureDevOps.Service>();
        })
        .ConfigureDefaults(args);
    }
}