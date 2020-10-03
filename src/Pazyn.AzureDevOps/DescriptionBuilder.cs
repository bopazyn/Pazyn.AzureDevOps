using System;
using System.Text;

namespace Pazyn.AzureDevOps
{
    public class DescriptionBuilder
    {
        private StringBuilder Builder { get; } = new StringBuilder();
        public void Append(String text) => Builder.Append(text);
        public void AppendLine(String text = null)
        {
            if(text != null)
            {
                Builder.Append(text);
            }
            Builder.Append("<br />");
        }

        public override String ToString() => Builder.ToString();
    }
}