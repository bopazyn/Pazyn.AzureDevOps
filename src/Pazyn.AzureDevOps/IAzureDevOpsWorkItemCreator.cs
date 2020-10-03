using System.Threading;
using System.Threading.Tasks;

namespace Pazyn.AzureDevOps
{
    public interface IAzureDevOpsWorkItemCreator
    {
        Task Create(AzureDevOpsWorkItem azureDevOpsWorkItem, CancellationToken cancellationToken);
    }
}