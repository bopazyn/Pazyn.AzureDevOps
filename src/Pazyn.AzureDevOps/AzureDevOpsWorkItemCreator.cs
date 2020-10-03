using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace Pazyn.AzureDevOps
{
    internal class AzureDevOpsWorkItemCreator : IAzureDevOpsWorkItemCreator
    {
        private IServiceProvider ServiceProvider { get; }
        private IOptions<AzureDevOpsOptions> Options { get; }
        private WorkItemTrackingHttpClient WorkItemTrackingHttpClient { get; }

        public AzureDevOpsWorkItemCreator(IServiceProvider serviceProvider, IOptions<AzureDevOpsOptions> options, WorkItemTrackingHttpClient workItemTrackingHttpClient)
        {
            ServiceProvider = serviceProvider;
            Options = options;
            WorkItemTrackingHttpClient = workItemTrackingHttpClient;
        }

        public async Task Create(AzureDevOpsWorkItem azureDevOpsWorkItem, CancellationToken cancellationToken)
        {
            azureDevOpsWorkItem.Files ??= new File[0];

            var attachments = new List<AttachmentReference>(azureDevOpsWorkItem.Files.Length);
            foreach (var file in azureDevOpsWorkItem.Files)
            {
                if (file.Data.CanSeek)
                {
                    file.Data.Seek(0, SeekOrigin.Begin);
                }

                var requestAttachment = await WorkItemTrackingHttpClient.CreateAttachmentAsync(
                    file.Data,
                    fileName: file.Name,
                    project: azureDevOpsWorkItem.Project,
                    cancellationToken: cancellationToken);

                attachments.Add(requestAttachment);
                await file.Data.DisposeAsync();
            }

            var jsonPatchDocument = new JsonPatchDocument();
            jsonPatchDocument.AddRange(azureDevOpsWorkItem.GetFields().Select(x => new JsonPatchOperation
            {
                Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                Path = x.field,
                From = null,
                Value = x.value
            }));

            jsonPatchDocument.AddRange(attachments.Select(x => new JsonPatchOperation
            {
                Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                Path = "/relations/-",
                From = null,
                Value = new
                {
                    Rel = "AttachedFile",
                    x.Url
                }
            }));
            Options.Value.OnBeforeSend?.Invoke(ServiceProvider, azureDevOpsWorkItem);

            var workItem = await WorkItemTrackingHttpClient.CreateWorkItemAsync(jsonPatchDocument, azureDevOpsWorkItem.Project, azureDevOpsWorkItem.Type, cancellationToken: cancellationToken);
            azureDevOpsWorkItem.Url = new Uri(workItem.Url);

            Options.Value.OnAfterSend?.Invoke(ServiceProvider, azureDevOpsWorkItem);
        }
    }
}