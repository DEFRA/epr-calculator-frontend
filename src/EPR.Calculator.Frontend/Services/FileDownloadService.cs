using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace EPR.Calculator.Frontend.Services
{
    public interface IFileDownloadService
    {
        Task<FileResult> DownloadResultFile(int runId);
        Task<FileResult> DownloadBillingFile(int runId, bool hasBeenSentToFss);
    }

    public class FileDownloadService(
        IEprCalculatorApiService eprCalculatorApiService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<FileDownloadService> logger)
        : IFileDownloadService
    {
        private static readonly string[] SupportedEncodings = ["br", "gzip"];

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
                // Ask only for encodings the browser accepts, so the compressed bytes can be passed straight through.
                var browserAcceptEncoding = httpContextAccessor.HttpContext?.Request.Headers.AcceptEncoding ?? StringValues.Empty;

                response = await eprCalculatorApiService.CallApiForStream(
                    httpMethod: HttpMethod.Get,
                    relativePath: relativePath,
                    acceptEncodings: SupportedEncodings.Where(e => Accepts(browserAcceptEncoding, e)).ToArray());

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";

                    string fileName = $"Result_{runId}.csv";

                    if (response.Content.Headers.ContentDisposition != null)
                    {
                        fileName = HandleContentDisposition(response, fileName);
                    }

                    var contentStream = await response.Content.ReadAsStreamAsync();

                    return new EncodedFileStreamResult(
                        new HttpResponseContentStream(response, contentStream),
                        contentType,
                        response.Content.Headers.ContentEncoding.SingleOrDefault())
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

        private static bool Accepts(StringValues acceptEncoding, string encoding) =>
            StringWithQualityHeaderValue.TryParseList(acceptEncoding, out var accepted) &&
            accepted.Any(a => a.Value.Equals(encoding, StringComparison.OrdinalIgnoreCase) && a.Quality is not 0);

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
        /// Relays a possibly compressed API response: passes the compressed bytes straight through when the
        /// browser accepts that encoding, and decompresses them on the fly when it doesn't.
        /// </summary>
        private sealed class EncodedFileStreamResult(Stream fileStream, string contentType, string? contentEncoding)
            : FileStreamResult(fileStream, contentType)
        {
            public override Task ExecuteResultAsync(ActionContext context)
            {
                if (contentEncoding is not null)
                {
                    var response = context.HttpContext.Response;
                    response.Headers.Vary = HeaderNames.AcceptEncoding;

                    if (Accepts(context.HttpContext.Request.Headers.AcceptEncoding, contentEncoding))
                        response.Headers.ContentEncoding = contentEncoding;
                    else
                        FileStream = Decompress(FileStream, contentEncoding);
                }

                return base.ExecuteResultAsync(context);
            }

            private static Stream Decompress(Stream stream, string encoding) => encoding.ToLowerInvariant() switch
            {
                "br" => new BrotliStream(stream, CompressionMode.Decompress),
                "gzip" => new GZipStream(stream, CompressionMode.Decompress),
                _ => throw new NotSupportedException($"Unsupported Content-Encoding '{encoding}'")
            };
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
