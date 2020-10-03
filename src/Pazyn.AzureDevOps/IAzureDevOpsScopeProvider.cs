namespace Pazyn.AzureDevOps
{
    public interface IAzureDevOpsScopeProvider
    {
        AzureDevOpsScope Scope { get; set; }
    }
}