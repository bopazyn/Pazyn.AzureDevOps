using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;

namespace Pazyn.AzureDevOps.AspNetCore
{
    internal class AzureDevOpsBugDelegatingHandler : DelegatingHandler
    {
        private IAzureDevOpsScopeProvider AzureDevOpsScopeProvider { get; }

        public AzureDevOpsBugDelegatingHandler(IAzureDevOpsScopeProvider azureDevOpsScopeProvider)
        {
            AzureDevOpsScopeProvider = azureDevOpsScopeProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (AzureDevOpsScopeProvider.Scope == null)
            {
                return await base.SendAsync(request, cancellationToken);
            }

            try
            {
                AzureDevOpsScopeProvider.Scope.DescriptionBuilder.AppendLine();

                var requestFile = await CreateRequestFile(request);
                AzureDevOpsScopeProvider.Scope.AddFile(requestFile);
                AzureDevOpsScopeProvider.Scope.DescriptionBuilder.Append(requestFile.Name);
                AzureDevOpsScopeProvider.Scope.DescriptionBuilder.AppendLine();

                var response = await base.SendAsync(request, cancellationToken);

                var responseFile = await CreateResponseFile(response);
                AzureDevOpsScopeProvider.Scope.AddFile(responseFile);
                AzureDevOpsScopeProvider.Scope.DescriptionBuilder.Append(responseFile.Name);
                AzureDevOpsScopeProvider.Scope.DescriptionBuilder.AppendLine();

                return response;
            }
            catch (Exception ex)
            {
                AzureDevOpsScopeProvider.Scope.DescriptionBuilder.AppendException(ex);
                throw;
            }
            finally
            {
                AzureDevOpsScopeProvider.Scope.DescriptionBuilder.Append("<hr />");
            }
        }


        private static async Task WriteHttpHeadersAsync(HttpHeaders httpHeaders, TextWriter streamWriter)
        {
            foreach (var (key, value) in httpHeaders)
            {
                await streamWriter.WriteLineAsync($"{key}: {String.Join(" ", value)}");
            }
        }

        private static async Task WriteHttpContentAsync(HttpContent httpContent, TextWriter streamWriter)
        {
            if (httpContent != null)
            {
                await WriteHttpHeadersAsync(httpContent.Headers, streamWriter);
                await streamWriter.WriteLineAsync();
                var content = await httpContent.ReadAsStringAsync();
                await streamWriter.WriteLineAsync(content);
            }
            else
            {
                await streamWriter.WriteLineAsync();
            }
        }

        private static async Task<File> CreateFile(HttpHeaders httpHeaders, HttpContent httpContent, StreamWriter streamWriter, String fileName)
        {
            await WriteHttpHeadersAsync(httpHeaders, streamWriter);
            await WriteHttpContentAsync(httpContent, streamWriter);
            await streamWriter.FlushAsync();
            streamWriter.BaseStream.Seek(0, SeekOrigin.Begin);
            return new File(fileName, streamWriter.BaseStream);
        }

        private static async Task<File> CreateRequestFile(HttpRequestMessage request)
        {
            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);
            await streamWriter.WriteLineAsync($"{request.Method} {request.RequestUri.PathAndQuery} HTTP/{request.Version}");
            await streamWriter.WriteLineAsync($"Host: {request.RequestUri.Host}");
            return await CreateFile(request.Headers, request.Content, streamWriter, $"{request.RequestUri}-request.txt");
        }

        private static async Task<File> CreateResponseFile(HttpResponseMessage response)
        {
            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);
            await streamWriter.WriteLineAsync($"HTTP/{response.Version} {(Int32) response.StatusCode} {ReasonPhrases.GetReasonPhrase((Int32) response.StatusCode)}");
            return await CreateFile(response.Headers, response.Content, streamWriter, $"{response.RequestMessage.RequestUri}-response.txt");
        }
    }
}