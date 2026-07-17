using System.Net;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Services
{
    public interface IFileDownloadService
    {
        Task<FileResult> DownloadResultFile(int runId);
        Task<FileResult> DownloadBillingFile(int runId, bool hasBeenSentToFss);
    }

    public class FileDownloadService(
        IEprCalculatorApiService eprCalculatorApiService,
        TelemetryClient telemetryClient)
        : IFileDownloadService
    {
        public Task<FileResult> DownloadResultFile(int runId)
        {
            var path = $"v1/DownloadResult/{runId}";
            return DownloadFile(path, runId);
        }

        public async Task<FileResult> DownloadBillingFile(int runId, bool hasBeenSentToFss)
        {
            var path = $"v1/DownloadBillingFile/{runId}";
            var result = await DownloadFile(path, runId);

            var filename = Path.GetFileNameWithoutExtension(result.FileDownloadName);

            result.FileDownloadName = hasBeenSentToFss
                ? $"{filename}_AUTHORISED.csv"
                : $"{filename}_DRAFT.csv";

            return result;
        }

        private async Task<FileResult> DownloadFile(string relativePath, int runId)
        {
            try
            {
                // Call the ApiService; it handles token acquisition for the current user
                var response = await eprCalculatorApiService.CallApi(
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
                    }

                    return new FileContentResult(fileBytes, contentType)
                    {
                        FileDownloadName = fileName
                    };
                }

                throw new HttpRequestException($"File download failed: StatusCode = {response.StatusCode}");
            }
            catch (Exception ex)
            {
                telemetryClient.TrackException(ex);
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
