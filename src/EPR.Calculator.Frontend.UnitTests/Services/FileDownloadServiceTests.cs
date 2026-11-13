using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using EPR.Calculator.Frontend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests.Services
{
    [TestClass]
    public class FileDownloadServiceTests
    {
        private FileDownloadService _fileDownloadService;
        private Mock<IEprCalculatorApiService> _eprCalculatorApiServiceMock;
        private DefaultHttpContext _browserRequest;

        public FileDownloadServiceTests()
        {
            _eprCalculatorApiServiceMock = new Mock<IEprCalculatorApiService>();
            _browserRequest = new DefaultHttpContext();
            _fileDownloadService = new FileDownloadService(
                _eprCalculatorApiServiceMock.Object,
                new HttpContextAccessor { HttpContext = _browserRequest },
                NullLogger<FileDownloadService>.Instance);
        }

        [TestMethod]
        public async Task DownloadFileAsync_ReturnsFileResult_WithCorrectFileName()
        {
            // Arrange
            int runId = 100;
            byte[] content = "test csv content"u8.ToArray();

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(content)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "\"Result_100.csv\""
            };

            _eprCalculatorApiServiceMock
                .Setup(x => x.CallApiForStream(
                    HttpMethod.Get,
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _fileDownloadService.DownloadResultFile(runId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(FileStreamResult));
            var fileResult = (FileStreamResult)result;
            Assert.AreEqual("Result_100.csv", fileResult.FileDownloadName);
            CollectionAssert.AreEqual(content, await ReadAllBytes(fileResult));
        }

        [TestMethod]
        public async Task DownloadFileAsync_Appends_DRAFT_IfDraftBilling()
        {
            int runId = 101;
            var content = "draft billing"u8.ToArray();

            var response = CreateCsvResponse(content, "\"BillingRun.csv\"");

            _eprCalculatorApiServiceMock
                .Setup(x => x.CallApiForStream(
                    HttpMethod.Get,
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var result = await _fileDownloadService.DownloadBillingFile(runId, false);

            var fileResult = (FileStreamResult)result;
            Assert.AreEqual("BillingRun_DRAFT.csv", fileResult.FileDownloadName);
        }

        [TestMethod]
        public async Task DownloadFileAsync_Appends_AUTHORISED_IfNotDraft()
        {
            int runId = 102;
            var content = "auth billing"u8.ToArray();

            var response = CreateCsvResponse(content, "\"BillingRun.csv\"");

            _eprCalculatorApiServiceMock
                .Setup(x => x.CallApiForStream(
                    HttpMethod.Get,
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var result = await _fileDownloadService.DownloadBillingFile(runId, true);

            var fileResult = (FileStreamResult)result;
            Assert.AreEqual("BillingRun_AUTHORISED.csv", fileResult.FileDownloadName);
        }

        [TestMethod]
        public async Task DownloadFileAsync_ThrowsHttpRequestException_OnNonOkResponse()
        {
            int runId = 103;

            var response = CreateCsvResponse(
                Array.Empty<byte>(),
                statusCode: HttpStatusCode.BadRequest);

            _eprCalculatorApiServiceMock
                .Setup(x => x.CallApiForStream(
                    HttpMethod.Get,
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
                _fileDownloadService.DownloadResultFile(runId));
        }

        [TestMethod]
        public async Task DownloadFileAsync_DisposesResponse_OnNonOkResponse()
        {
            var response = CreateCsvResponse(Array.Empty<byte>(), statusCode: HttpStatusCode.BadRequest);

            _eprCalculatorApiServiceMock
                .Setup(x => x.CallApiForStream(
                    HttpMethod.Get,
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
                _fileDownloadService.DownloadResultFile(108));

            await Assert.ThrowsExceptionAsync<ObjectDisposedException>(() => response.Content.ReadAsByteArrayAsync());
        }

        [TestMethod]
        public async Task DownloadFileAsync_DisposingTheFileStream_DisposesTheResponse()
        {
            var response = CreateCsvResponse("streamed content"u8.ToArray(), "\"Result_109.csv\"");

            _eprCalculatorApiServiceMock
                .Setup(x => x.CallApiForStream(
                    HttpMethod.Get,
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var fileResult = (FileStreamResult)await _fileDownloadService.DownloadResultFile(109);
            await fileResult.FileStream.DisposeAsync();

            await Assert.ThrowsExceptionAsync<ObjectDisposedException>(() => response.Content.ReadAsByteArrayAsync());
        }

        [TestMethod]
        public async Task DownloadFileAsync_UsesFileNameStar_WhenFileNameIsEmpty()
        {
            int runId = 104;
            var content = "test content"u8.ToArray();

            var response = CreateCsvResponse(
                content,
                fileName: null,
                fileNameStar: "\"AlternativeFile.csv\"");

            _eprCalculatorApiServiceMock
                .Setup(x => x.CallApiForStream(
                    HttpMethod.Get,
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var result = await _fileDownloadService
                .DownloadResultFile(runId);

            var fileResult = (FileStreamResult)result;
            Assert.AreEqual("AlternativeFile.csv", fileResult.FileDownloadName);
        }

        [TestMethod]
        public async Task DownloadFileAsync_UsesDefaultFileName_WhenBothFileNameAndFileNameStarAreEmpty()
        {
            int runId = 105;
            var content = "test content"u8.ToArray();

            var response = CreateCsvResponse(content);

            _eprCalculatorApiServiceMock
                .Setup(x => x.CallApiForStream(
                    HttpMethod.Get,
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var result = await _fileDownloadService.DownloadResultFile(runId);

            var fileResult = (FileStreamResult)result;
            Assert.AreEqual($"Result_{runId}.csv", fileResult.FileDownloadName);
        }

        [TestMethod]
        public async Task DownloadFileAsync_UsesDefaultFileName_WhenContentDispositionIsNull()
        {
            int runId = 106;
            var content = "test content"u8.ToArray();

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(content)
            };
            response.Content.Headers.ContentType =
                new MediaTypeHeaderValue("text/csv");

            _eprCalculatorApiServiceMock
                .Setup(x => x.CallApiForStream(
                    HttpMethod.Get,
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var result = await _fileDownloadService.DownloadResultFile(runId);

            var fileResult = (FileStreamResult)result;
            Assert.AreEqual($"Result_{runId}.csv", fileResult.FileDownloadName);
        }

        [TestMethod]
        public async Task DownloadFileAsync_AppliesBillingSuffix_WhenFileNameStarUsedAndIsBillingFile()
        {
            int runId = 107;
            var content = "billing content"u8.ToArray();

            var response = CreateCsvResponse(
                content,
                fileName: string.Empty,
                fileNameStar: "\"BillingFromStar.csv\"");

            _eprCalculatorApiServiceMock
                .Setup(x => x.CallApiForStream(
                    HttpMethod.Get,
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var result = await _fileDownloadService.DownloadBillingFile(runId, false);

            var fileResult = (FileStreamResult)result;
            Assert.AreEqual("BillingFromStar_DRAFT.csv", fileResult.FileDownloadName);
        }

        [TestMethod]
        [DataRow("br", "gzip, deflate, br, zstd")]
        public async Task DownloadFileAsync_PassesCompressedContentThrough_WhenTheBrowserAcceptsIt(string encoding, string browserAcceptEncoding)
        {
            var content = "producer,1.000,2.000\r\n"u8.ToArray();
            var compressed = Compress(content, encoding);
            SetupStreamResponse(CreateCompressedResponse(compressed, encoding));

            var (httpContext, body) = await Execute(await _fileDownloadService.DownloadResultFile(110), browserAcceptEncoding);

            Assert.AreEqual(encoding, httpContext.Response.Headers.ContentEncoding.ToString());
            Assert.AreEqual("Accept-Encoding", httpContext.Response.Headers.Vary.ToString());
            CollectionAssert.AreEqual(compressed, body);
        }

        [TestMethod]
        [DataRow("br", null)]
        [DataRow("gzip", "br;q=0, gzip;q=0")]
        public async Task DownloadFileAsync_DecompressesContent_WhenTheBrowserDoesNotAcceptTheEncoding(string encoding, string? browserAcceptEncoding)
        {
            var content = "producer,1.000,2.000\r\n"u8.ToArray();
            SetupStreamResponse(CreateCompressedResponse(Compress(content, encoding), encoding));

            var (httpContext, body) = await Execute(await _fileDownloadService.DownloadResultFile(111), browserAcceptEncoding);

            Assert.AreEqual(string.Empty, httpContext.Response.Headers.ContentEncoding.ToString());
            CollectionAssert.AreEqual(content, body);
        }

        [TestMethod]
        [DataRow("gzip, deflate, br, zstd", new[] { "br", "gzip" })]
        [DataRow("gzip, deflate", new[] { "gzip" })]
        [DataRow("br;q=0, gzip", new[] { "gzip" })]
        [DataRow(null, new string[0])]
        public async Task DownloadFileAsync_AsksTheApiOnlyForEncodingsTheBrowserAccepts(string? browserAcceptEncoding, string[] expectedEncodings)
        {
            if (browserAcceptEncoding is not null)
                _browserRequest.Request.Headers.AcceptEncoding = browserAcceptEncoding;
            IEnumerable<string>? requestedEncodings = null;
            _eprCalculatorApiServiceMock
                .Setup(x => x.CallApiForStream(HttpMethod.Get, It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .Callback<HttpMethod, string, IEnumerable<string>, CancellationToken>((_, _, encodings, _) => requestedEncodings = encodings)
                .ReturnsAsync(CreateCsvResponse("a,b"u8.ToArray(), "\"Result.csv\""));

            await _fileDownloadService.DownloadResultFile(112);

            CollectionAssert.AreEqual(expectedEncodings, requestedEncodings!.ToArray());
        }

        private void SetupStreamResponse(HttpResponseMessage response) =>
            _eprCalculatorApiServiceMock
                .Setup(x => x.CallApiForStream(
                    HttpMethod.Get,
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

        private static HttpResponseMessage CreateCompressedResponse(byte[] compressed, string encoding)
        {
            var response = CreateCsvResponse(compressed, "\"Result.csv\"");
            response.Content.Headers.ContentEncoding.Add(encoding);
            return response;
        }

        private static byte[] Compress(byte[] content, string encoding)
        {
            using var output = new MemoryStream();
            using (Stream compressor = encoding == "br"
                ? new BrotliStream(output, CompressionLevel.Optimal)
                : new GZipStream(output, CompressionLevel.Optimal))
            {
                compressor.Write(content);
            }

            return output.ToArray();
        }

        private static async Task<(HttpContext HttpContext, byte[] Body)> Execute(FileResult result, string? browserAcceptEncoding)
        {
            var httpContext = new DefaultHttpContext
            {
                RequestServices = new ServiceCollection()
                    .AddLogging()
                    .AddSingleton<IActionResultExecutor<FileStreamResult>, FileStreamResultExecutor>()
                    .BuildServiceProvider()
            };
            using var body = new MemoryStream();
            httpContext.Response.Body = body;

            if (browserAcceptEncoding is not null)
                httpContext.Request.Headers.AcceptEncoding = browserAcceptEncoding;

            await result.ExecuteResultAsync(new ActionContext(httpContext, new RouteData(), new ActionDescriptor()));
            return (httpContext, body.ToArray());
        }

        private static async Task<byte[]> ReadAllBytes(FileStreamResult fileResult)
        {
            using var buffer = new MemoryStream();
            await fileResult.FileStream.CopyToAsync(buffer);
            return buffer.ToArray();
        }

        private static HttpResponseMessage CreateCsvResponse(
            byte[] content,
            string? fileName = null,
            string? fileNameStar = null,
            HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new ByteArrayContent(content)
            };

            response.Content.Headers.ContentType =
                new MediaTypeHeaderValue("text/csv");

            if (fileName != null || fileNameStar != null)
            {
                response.Content.Headers.ContentDisposition =
                    new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = fileName,
                        FileNameStar = fileNameStar
                    };
            }

            return response;
        }
    }
}
