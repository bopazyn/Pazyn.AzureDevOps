using System.Threading;

namespace Pazyn.AzureDevOps
{
    internal class AzureDevOpsScopeProvider : IAzureDevOpsScopeProvider
    {
        private static readonly AsyncLocal<AzureDevOpsScope> _scope = new AsyncLocal<AzureDevOpsScope>();

        public AzureDevOpsScope Scope
        {
            get => _scope.Value;
            set => _scope.Value = value;
        }
    }
}