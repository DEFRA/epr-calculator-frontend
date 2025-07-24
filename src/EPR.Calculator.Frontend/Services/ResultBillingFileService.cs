using System.Net;
using System.Text;
using System.Text.Json;
using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Services
{
    public class ResultBillingFileService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        TelemetryClient telemetryClient) : IResultBillingFileService
    {
        private readonly IHttpClientFactory httpClientFactory = httpClientFactory;
        private readonly IConfiguration configuration = configuration;
        private readonly TelemetryClient telemetryClient = telemetryClient;

        public async Task<FileResult> DownloadFileAsync(
            Uri apiUrl,
            int runId,
            string accessToken,
            bool isBillingFile = false,
            bool isDraftBillingFile = false)
        {
            if (runId <= 0)
            {
                throw new ArgumentException("Invalid runId", nameof(runId));
            }

            var timeout = this.configuration.GetValue<int>($"{ConfigSection.CalculationRunSettings}:{ConfigSection.DownloadResultTimeoutInMilliSeconds}");

            try
            {
                var response = await this.CallApi(HttpMethod.Get, apiUrl, runId.ToString(), accessToken, null, timeout);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var fileBytes = await response.Content.ReadAsByteArrayAsync();
                    var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";

                    string fileName = $"Result_{runId}.csv";

                    if (response.Content.Headers.ContentDisposition != null)
                    {
                        fileName = HandleContentDisposition(response, fileName);

                        if (isBillingFile)
                        {
                            var baseName = Path.GetFileNameWithoutExtension(fileName);
                            fileName = isDraftBillingFile
                                ? $"{baseName}_DRAFT.csv"
                                : $"{baseName}_AUTHORISED.csv";
                        }
                    }

                    return new FileContentResult(fileBytes, contentType)
                    {
                        FileDownloadName = fileName,
                    };
                }

                throw new HttpRequestException($"File download failed: StatusCode = {response.StatusCode}");
            }
            catch (Exception ex)
            {
                this.telemetryClient.TrackException(ex);
                throw;
            }
        }

        private static string HandleContentDisposition(HttpResponseMessage response, string fileName)
        {
            var contentDisposition = response.Content.Headers.ContentDisposition;

            if (!string.IsNullOrEmpty(contentDisposition!.FileName))
            {
                fileName = contentDisposition.FileName.Trim('"').Trim('\'');
            }
            else if (!string.IsNullOrEmpty(contentDisposition.FileNameStar))
            {
                fileName = contentDisposition.FileNameStar.Trim('"').Trim('\'');
            }

            return fileName;
        }

        private async Task<HttpResponseMessage> CallApi(
         HttpMethod httpMethod,
         Uri apiUrl,
         string argument,
         string accessToken,
         object? body,
         int? timeout = 100000)
        {
            var argsString = !string.IsNullOrEmpty(argument)
                ? $"/{argument}"
                : string.Empty;
            argsString = !argument.Contains('&') ? argsString : $"?{argument}";
            var contentString = JsonSerializer.Serialize(body);
            var request = new HttpRequestMessage(
                httpMethod,
                new Uri($"{apiUrl}{argsString}"));
            if (body is not null)
            {
                request.Content = new StringContent(
                    contentString,
                    Encoding.UTF8,
                    StaticHelpers.MediaType);
            }

            var client = await this.GetHttpClient(accessToken, timeout);
            return await client.SendAsync(request);
        }

        private async Task<HttpClient> GetHttpClient(string accessToken, int? timeout)
        {
            var client = this.httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMilliseconds(timeout ?? 100000); // fallback timeout

            if (client.DefaultRequestHeaders is not null && !client.DefaultRequestHeaders.Contains("Authorization"))
            {
                client.DefaultRequestHeaders.Add("Authorization", accessToken);
            }

            await Task.CompletedTask;

            return client;
        }
    }
}