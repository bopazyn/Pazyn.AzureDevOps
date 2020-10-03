using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Pazyn.AzureDevOps.AspNetCore
{
    public static class Extensions
    {
        public static IHttpClientBuilder AddAzureDevOpsBugDelegatingHandler(this IHttpClientBuilder httpClientBuilder) =>
            httpClientBuilder.AddHttpMessageHandler(sp => ActivatorUtilities.CreateInstance<AzureDevOpsBugDelegatingHandler>(sp));

        public static IApplicationBuilder UseAzureDevOpsBug(this IApplicationBuilder app, AzureDevOpsAspNetCoreScopeOptions scopeOptions) =>
            app.UseMiddleware<AzureDevOpsBugMiddleware>(scopeOptions);
    }
}