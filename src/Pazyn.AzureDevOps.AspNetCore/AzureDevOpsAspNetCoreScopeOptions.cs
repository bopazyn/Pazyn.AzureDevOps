using System;
using Microsoft.AspNetCore.Http;

namespace Pazyn.AzureDevOps.AspNetCore
{
    public class AzureDevOpsAspNetCoreScopeOptions : AzureDevOpsScopeOptions
    {
        public Func<HttpContext, Boolean> IsBug { get; set; }
    }
}