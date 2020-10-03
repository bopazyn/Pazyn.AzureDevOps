using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Pazyn.AzureDevOps
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureDevOpsConnection(this IServiceCollection services, Action<AzureDevOpsOptions> configure)
        {
            services.AddOptions<AzureDevOpsOptions>()
                .Configure(configure);

            services.TryAddScoped(sp =>
            {
                var options = sp.GetRequiredService<IOptions<AzureDevOpsOptions>>();
                var collectionUri = $"https://dev.azure.com/{options.Value.Organization}";
                return new VssConnection(new Uri(collectionUri), new VssBasicCredential(String.Empty, options.Value.Pat));
            });

            services.TryAddScoped(sp =>
            {
                var connection = sp.GetRequiredService<VssConnection>();
                return connection.GetClient<WorkItemTrackingHttpClient>();
            });

            services.TryAddScoped<IAzureDevOpsWorkItemCreator, AzureDevOpsWorkItemCreator>();
            // Registered as singleton exactly the same as IHttpContextAccessor.
            services.TryAddSingleton<IAzureDevOpsScopeProvider, AzureDevOpsScopeProvider>();
            return services;
        }
    }
}