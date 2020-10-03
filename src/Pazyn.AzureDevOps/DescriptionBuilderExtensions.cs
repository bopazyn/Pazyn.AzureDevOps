using System;
using System.Diagnostics;

namespace Pazyn.AzureDevOps
{
    public static class DescriptionBuilderExtensions
    {
        public static void AppendException(this DescriptionBuilder descriptionBuilder, Exception ex)
        {
            descriptionBuilder.Append("<div style=\"white-space: nowrap; overflow-x: auto; width: auto; padding: 10px; background-color: rgba(0, 0, 0, 0.1); \">");
            descriptionBuilder.Append($"<h3>Wyjątek {ex.GetType().Name}</h3>");
            descriptionBuilder.Append(ex.Message);
            if (ex.StackTrace != null)
            {
                descriptionBuilder.Append("<h3>StackTrace</h3>");
                descriptionBuilder.Append(ex.Demystify().StackTrace.Replace(" at ", "<br />"));
            }

            if (ex.InnerException != null)
            {
                descriptionBuilder.AppendException(ex.InnerException);
            }

            descriptionBuilder.Append("</div>");
        }
    }
}