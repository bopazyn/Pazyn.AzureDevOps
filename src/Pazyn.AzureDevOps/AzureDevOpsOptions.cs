using System;
using System.Threading.Tasks;

namespace Pazyn.AzureDevOps
{
    public class AzureDevOpsOptions
    {
        public String Pat { get; set; }
        public String Organization { get; set; }
        public Boolean Enabled { get; set; }

        public Func<IServiceProvider, AzureDevOpsWorkItem, Task> OnBeforeSend { get; set; }
        public Func<IServiceProvider, AzureDevOpsWorkItem, Task> OnAfterSend { get; set; }
    }
}