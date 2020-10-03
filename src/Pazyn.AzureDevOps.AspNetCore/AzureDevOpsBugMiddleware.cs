using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Pazyn.AzureDevOps.AspNetCore
{
    internal class AzureDevOpsBugMiddleware
    {
        private IOptions<AzureDevOpsOptions> Options { get; }
        private AzureDevOpsAspNetCoreScopeOptions ScopeOptions { get; }
        private RequestDelegate Next { get; }

        public AzureDevOpsBugMiddleware(RequestDelegate next, IOptions<AzureDevOpsOptions> options, AzureDevOpsAspNetCoreScopeOptions scopeOptions)
        {
            Next = next;
            Options = options;
            ScopeOptions = scopeOptions;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (!Options.Value.Enabled)
            {
                await Next(httpContext);
                return;
            }

            var serviceProvider = httpContext.RequestServices;
            var azureDevOpsScopeProvider = serviceProvider.GetRequiredService<IAzureDevOpsScopeProvider>();
            azureDevOpsScopeProvider.Scope = new AzureDevOpsScope(ScopeOptions.Project, ScopeOptions.Title, ScopeOptions.Tags);
            var azureDevOpsWorkItemCreator = serviceProvider.GetRequiredService<IAzureDevOpsWorkItemCreator>();


            try
            {
                await Next(httpContext);
                if (ScopeOptions.IsBug != null && ScopeOptions.IsBug(httpContext))
                {
                    await CreateBug(azureDevOpsScopeProvider, azureDevOpsWorkItemCreator, httpContext, null);
                }
            }
            catch (Exception ex)
            {
                await CreateBug(azureDevOpsScopeProvider, azureDevOpsWorkItemCreator, httpContext, ex);
                throw;
            }
        }

        private static async Task CreateBug(IAzureDevOpsScopeProvider azureDevOpsScopeProvider, IAzureDevOpsWorkItemCreator azureDevOpsWorkItemCreator, HttpContext httpContext, Exception ex)
        {
            azureDevOpsScopeProvider.Scope.DescriptionBuilder.Append("<h3>Request</h3>");
            azureDevOpsScopeProvider.Scope.DescriptionBuilder.AppendLine($"<b>Method: </b> {httpContext.Request.Method}");
            azureDevOpsScopeProvider.Scope.DescriptionBuilder.AppendLine($"<b>Url: </b> {httpContext.Request.GetDisplayUrl()}");
            azureDevOpsScopeProvider.Scope.DescriptionBuilder.Append("<hr />");

            azureDevOpsScopeProvider.Scope.DescriptionBuilder.Append("<h3>Response</h3>");
            azureDevOpsScopeProvider.Scope.DescriptionBuilder.AppendLine($"<b>Status: </b> {httpContext.Response.StatusCode}");
            azureDevOpsScopeProvider.Scope.DescriptionBuilder.Append("<hr />");

            if (ex != null)
            {
                azureDevOpsScopeProvider.Scope.DescriptionBuilder.AppendException(ex);
                azureDevOpsScopeProvider.Scope.DescriptionBuilder.Append("<hr />");
            }

            var azureDevOpsWorkItem = azureDevOpsScopeProvider.Scope.MaterializeToBug();
            await azureDevOpsWorkItemCreator.Create(azureDevOpsWorkItem, httpContext.RequestAborted);
        }
    }
}