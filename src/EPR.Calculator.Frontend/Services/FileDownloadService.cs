using System.Diagnostics.CodeAnalysis;
using System.Net;
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
        ILogger<FileDownloadService> logger)
        : IFileDownloadService
    {
        public Task<FileResult> DownloadResultFile(int runId)
        {
            var path = $"v1/downloadResultCsv/{runId}";
            return DownloadFile(path, runId);
        }

        public async Task<FileResult> DownloadBillingFile(int runId, bool hasBeenSentToFss)
        {
            var path = $"v1/downloadBillingCsv/{runId}";
            var result = await DownloadFile(path, runId);

            var filename = Path.GetFileNameWithoutExtension(result.FileDownloadName);

            result.FileDownloadName = hasBeenSentToFss
                ? $"{filename}_AUTHORISED.csv"
                : $"{filename}_DRAFT.csv";

            return result;
        }

        private async Task<FileResult> DownloadFile(string relativePath, int runId)
        {
            HttpResponseMessage? response = null;

            try
            {
                // Call the ApiService; it handles token acquisition for the current user
                response = await eprCalculatorApiService.CallApiForStream(
                    httpMethod: HttpMethod.Get,
                    relativePath: relativePath);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";

                    string fileName = $"Result_{runId}.csv";

                    if (response.Content.Headers.ContentDisposition != null)
                    {
                        fileName = HandleContentDisposition(response, fileName);
                    }

                    var contentStream = await response.Content.ReadAsStreamAsync();

                    return new FileStreamResult(new HttpResponseContentStream(response, contentStream), contentType)
                    {
                        FileDownloadName = fileName
                    };
                }

                throw new HttpRequestException($"File download failed: StatusCode = {response.StatusCode}");
            }
            catch (Exception ex)
            {
                response?.Dispose();
                logger.LogError(ex, "Count not download file {RelativePath} for {RunId}", relativePath, runId);
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

        /// <summary>
        /// Wraps the response content stream so that disposing it also disposes the owning
        /// <see cref="HttpResponseMessage"/>, releasing the underlying connection once streaming completes.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private sealed class HttpResponseContentStream(HttpResponseMessage response, Stream innerStream) : Stream
        {
            public override bool CanRead => innerStream.CanRead;

            public override bool CanSeek => innerStream.CanSeek;

            public override bool CanWrite => innerStream.CanWrite;

            public override long Length => innerStream.Length;

            public override long Position
            {
                get => innerStream.Position;
                set => innerStream.Position = value;
            }

            public override void Flush() => innerStream.Flush();

            public override int Read(byte[] buffer, int offset, int count) => innerStream.Read(buffer, offset, count);

            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
                innerStream.ReadAsync(buffer, offset, count, cancellationToken);

            public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
                innerStream.ReadAsync(buffer, cancellationToken);

            public override long Seek(long offset, SeekOrigin origin) => innerStream.Seek(offset, origin);

            public override void SetLength(long value) => innerStream.SetLength(value);

            public override void Write(byte[] buffer, int offset, int count) => innerStream.Write(buffer, offset, count);

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    innerStream.Dispose();
                    response.Dispose();
                }

                base.Dispose(disposing);
            }
        }
    }
}
