using System;
using System.Collections.Generic;

namespace Pazyn.AzureDevOps
{
    public class AzureDevOpsScope
    {
        public String Project { get; }
        public String Title { get; }
        public String[] Tags { get; }
        public DescriptionBuilder DescriptionBuilder { get; } = new DescriptionBuilder();
        public List<File> Files { get; } = new List<File>();

        public AzureDevOpsScope(String project, String title, String[] tags)
        {
            Project = project;
            Title = title;
            Tags = tags;
        }

        public void AddFile(File file)
        {
            Files.Add(file);
        }

        public AzureDevOpsWorkItem MaterializeToBug()
        {
            var azureDevOpsWorkItem = new AzureDevOpsWorkItem
            {
                Project = Project,
                Type = "Bug",
                Files = Files.ToArray()
            };

            azureDevOpsWorkItem.SetProperty("/fields/System.Title", Title);
            azureDevOpsWorkItem.SetProperty("/fields/Microsoft.VSTS.TCM.ReproSteps", DescriptionBuilder.ToString());
            if (Tags != null)
            {
                azureDevOpsWorkItem.SetProperty("/fields/System.Tags", String.Join(",", Tags));
            }

            return azureDevOpsWorkItem;
        }
    }
}