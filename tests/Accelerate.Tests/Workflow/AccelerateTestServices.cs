namespace Accelerate.Tests.Workflow;

public static class AccelerateTestServices
{
    public static IHostBuilder ConfigureTestServices(this IHostBuilder builder, IGitCommand<AzureDevOps> azureDevOpsService)
    {
        return builder.ConfigureServices((_, serviceCollection) =>
        {
            serviceCollection.Remove(serviceCollection.Single(service => typeof(IGitCommand<AzureDevOps>) == service.ServiceType));
            serviceCollection.AddSingleton(azureDevOpsService);
        });
    }
}