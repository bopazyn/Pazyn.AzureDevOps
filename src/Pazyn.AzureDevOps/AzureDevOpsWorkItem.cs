using System;
using System.Collections.Generic;
using System.Linq;

namespace Pazyn.AzureDevOps
{
    public class AzureDevOpsWorkItem
    {
        private IDictionary<String, String> Values { get; } = new Dictionary<String, String>();

        public (String field, String value)[] GetFields() =>
            Values.
                Select(x => (field : x.Key, x.Value))
                .ToArray();

        public void SetProperty(String key, String value) =>
            Values[key] = value;

        public String Project { get; set; }
        public String Type { get; set; }
        public File[] Files { get; set; }
        public Uri Url { get; set; }
    }
}