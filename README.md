# Pazyn.AzureDevOps

`Pazyn.AzureDevOps` is library that integrates your application with Azure DevOps. It creates bug when an exception is occurred.

## Minimal working example

```
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAzureDevOpsConnection(x =>
        {
            x.Enabled = true;
            x.Organization = ">>ORG<<";
            x.Pat = ">>PAT<<";
        });

        services.AddHttpClient("A")
            .AddAzureDevOpsBugDelegatingHandler();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseAzureDevOpsBug(new AzureDevOpsAspNetCoreScopeOptions
        {
            Project = ">>PROJECT<<",
            Title = "[Bot] Pipeline",
            IsBug = context => context.Response.StatusCode >= 500,
        });

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/ex", context => throw new NotImplementedException());

            endpoints.MapGet("/http-client/{code}", async context =>
            {
                var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient("A");
                var code = context.GetRouteValue("code");
                var httpResponseMessage = await httpClient.GetAsync($"https://mock.codes/{code}");
                context.Response.StatusCode = (Int32) httpResponseMessage.StatusCode;
            });

            endpoints.MapGet("/200", async context =>
            {
                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.WriteAsync("Hello World!");
            });

            endpoints.MapGet("/500", async context =>
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("Internal server error!");
            });
        });
    }
}
```