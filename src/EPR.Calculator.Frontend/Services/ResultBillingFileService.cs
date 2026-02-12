using System.Net;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Services
{
    public interface IResultBillingFileService
    {
        Task<FileResult> DownloadFileAsync(string relativePath, int runId, HttpContext httpContext, bool isBillingFile = false, bool isDraftBillingFile = false);
    }

    public class ResultBillingFileService(
        IEprCalculatorApiService eprCalculatorApiService,
        TelemetryClient telemetryClient) : IResultBillingFileService
    {
        private readonly IEprCalculatorApiService eprCalculatorApiService = eprCalculatorApiService;
        private readonly TelemetryClient telemetryClient = telemetryClient;

        public async Task<FileResult> DownloadFileAsync(
            string relativePath,
            int runId,
            HttpContext httpContext,
            bool isBillingFile = false,
            bool isDraftBillingFile = false)
        {
            if (runId <= 0)
            {
                throw new ArgumentException("Invalid runId", nameof(runId));
            }

            try
            {
                // Call the ApiService; it handles token acquisition for the current user
                var response = await this.eprCalculatorApiService.CallApi(
                    httpContext: httpContext,
                    httpMethod: HttpMethod.Get,
                    relativePath: relativePath);

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
    }
}
