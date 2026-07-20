using System.Net;
using System.Net.Http.Headers;
using EPR.Calculator.Frontend.Services;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests.Services
{
    [TestClass]
    public class FileDownloadServiceTests
    {
        private TelemetryClient _telemetryClient;
        private FileDownloadService _fileDownloadService;
        private Mock<IEprCalculatorApiService> _eprCalculatorApiServiceMock;

        public FileDownloadServiceTests()
        {
            _eprCalculatorApiServiceMock = new Mock<IEprCalculatorApiService>();
            _telemetryClient = new TelemetryClient(new Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration());

            _fileDownloadService = new FileDownloadService(
                _eprCalculatorApiServiceMock.Object,
                _telemetryClient);
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
                .Setup(x => x.CallApi(
                    HttpMethod.Get,
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string?>>(),
                    It.IsAny<object>()))
                .ReturnsAsync(response);

            // Act
            var result = await _fileDownloadService.DownloadResultFile(runId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(FileContentResult));
            var fileResult = (FileContentResult)result;
            Assert.AreEqual("Result_100.csv", fileResult.FileDownloadName);
            CollectionAssert.AreEqual(content, fileResult.FileContents);
        }

        [TestMethod]
        public async Task DownloadFileAsync_Appends_DRAFT_IfDraftBilling()
        {
            int runId = 101;
            var content = "draft billing"u8.ToArray();

            var response = CreateCsvResponse(content, "\"BillingRun.csv\"");

            _eprCalculatorApiServiceMock
                .Setup(x => x.CallApi(
                    HttpMethod.Get,
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string?>>(),
                    It.IsAny<object>()))
                .ReturnsAsync(response);

            var result = await _fileDownloadService.DownloadBillingFile(runId, false);

            var fileResult = (FileContentResult)result;
            Assert.AreEqual("BillingRun_DRAFT.csv", fileResult.FileDownloadName);
        }

        [TestMethod]
        public async Task DownloadFileAsync_Appends_AUTHORISED_IfNotDraft()
        {
            int runId = 102;
            var content = "auth billing"u8.ToArray();

            var response = CreateCsvResponse(content, "\"BillingRun.csv\"");

            _eprCalculatorApiServiceMock
                .Setup(x => x.CallApi(
                    HttpMethod.Get,
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string?>>(),
                    It.IsAny<object>()))
                .ReturnsAsync(response);

            var result = await _fileDownloadService.DownloadBillingFile(runId, true);

            var fileResult = (FileContentResult)result;
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
                .Setup(x => x.CallApi(
                    HttpMethod.Get,
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string?>>(),
                    It.IsAny<object>()))
                .ReturnsAsync(response);

            await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
                _fileDownloadService.DownloadResultFile(runId));
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
                .Setup(x => x.CallApi(
                    HttpMethod.Get,
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string?>>(),
                    It.IsAny<object>()))
                .ReturnsAsync(response);

            var result = await _fileDownloadService
                .DownloadResultFile(runId);

            var fileResult = (FileContentResult)result;
            Assert.AreEqual("AlternativeFile.csv", fileResult.FileDownloadName);
        }

        [TestMethod]
        public async Task DownloadFileAsync_UsesDefaultFileName_WhenBothFileNameAndFileNameStarAreEmpty()
        {
            int runId = 105;
            var content = "test content"u8.ToArray();

            var response = CreateCsvResponse(content);

            _eprCalculatorApiServiceMock
                .Setup(x => x.CallApi(
                    HttpMethod.Get,
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string?>>(),
                    It.IsAny<object>()))
                .ReturnsAsync(response);

            var result = await _fileDownloadService.DownloadResultFile(runId);

            var fileResult = (FileContentResult)result;
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
                .Setup(x => x.CallApi(
                    HttpMethod.Get,
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string?>>(),
                    It.IsAny<object>()))
                .ReturnsAsync(response);

            var result = await _fileDownloadService.DownloadResultFile(runId);

            var fileResult = (FileContentResult)result;
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
                .Setup(x => x.CallApi(
                    HttpMethod.Get,
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string?>>(),
                    It.IsAny<object>()))
                .ReturnsAsync(response);

            var result = await _fileDownloadService.DownloadBillingFile(runId, false);

            var fileResult = (FileContentResult)result;
            Assert.AreEqual("BillingFromStar_DRAFT.csv", fileResult.FileDownloadName);
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
