using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using EPR.Calculator.Frontend.Services;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests.Services
{
    [TestClass]
    public class FileDownloadServiceTests
    {
        private const int TimeoutMs = 5000;
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private Mock<IConfiguration> _configurationMock;
        private TelemetryClient _telemetryClient;
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private ResultBillingFileService _fileDownloadService;
        private Mock<IApiService> _apiServiceMock;
        private HttpContext _httpContext;

        public FileDownloadServiceTests()
        {
            _apiServiceMock = new Mock<IApiService>();
            _telemetryClient = new TelemetryClient();

            _fileDownloadService = new ResultBillingFileService(
                _apiServiceMock.Object,
                _telemetryClient);

            _httpContext = new DefaultHttpContext();
            _httpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.Name, "TestUser") },
                    "mock"));
        }

        [TestMethod]
        public async Task DownloadFileAsync_ReturnsFileResult_WithCorrectFileName()
        {
            // Arrange
            var apiUrl = new Uri("https://api.example.com/files");
            int runId = 100;
            byte[] content = Encoding.UTF8.GetBytes("test csv content");

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(content)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "\"Result_100.csv\""
            };

            _apiServiceMock
                .Setup(x => x.CallApi(
                    It.IsAny<HttpContext>(),
                    HttpMethod.Get,
                    It.IsAny<Uri>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .ReturnsAsync(response);

            // Act
            var result = await _fileDownloadService.DownloadFileAsync(
                apiUrl,
                runId,
                _httpContext);

            // Assert
            Assert.IsInstanceOfType(result, typeof(FileContentResult));
            var fileResult = (FileContentResult)result;
            Assert.AreEqual("Result_100.csv", fileResult.FileDownloadName);
            CollectionAssert.AreEqual(content, fileResult.FileContents);
        }

        [TestMethod]
        public async Task DownloadFileAsync_Appends_DRAFT_IfDraftBilling()
        {
            var apiUrl = new Uri("https://api.example.com/files");
            int runId = 101;
            var content = Encoding.UTF8.GetBytes("draft billing");

            var response = CreateCsvResponse(content, "\"BillingRun.csv\"");

            _apiServiceMock
                .Setup(x => x.CallApi(
                    It.IsAny<HttpContext>(),
                    HttpMethod.Get,
                    It.IsAny<Uri>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .ReturnsAsync(response);

            var result = await _fileDownloadService
                .DownloadFileAsync(apiUrl, runId, _httpContext, true, true);

            var fileResult = (FileContentResult)result;
            Assert.AreEqual("BillingRun_DRAFT.csv", fileResult.FileDownloadName);
        }

        [TestMethod]
        public async Task DownloadFileAsync_Appends_AUTHORISED_IfNotDraft()
        {
            var apiUrl = new Uri("https://api.example.com/files");
            int runId = 102;
            var content = Encoding.UTF8.GetBytes("auth billing");

            var response = CreateCsvResponse(content, "\"BillingRun.csv\"");

            _apiServiceMock
                .Setup(x => x.CallApi(
                    It.IsAny<HttpContext>(),
                    HttpMethod.Get,
                    It.IsAny<Uri>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .ReturnsAsync(response);

            var result = await _fileDownloadService
                .DownloadFileAsync(apiUrl, runId, _httpContext, true, false);

            var fileResult = (FileContentResult)result;
            Assert.AreEqual("BillingRun_AUTHORISED.csv", fileResult.FileDownloadName);
        }

        [TestMethod]
        public async Task DownloadFileAsync_ThrowsArgumentException_WhenRunIdInvalid()
        {
            await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
                _fileDownloadService.DownloadFileAsync(
                    new Uri("https://api.test.com"), 0, _httpContext));
        }

        [TestMethod]
        public async Task DownloadFileAsync_ThrowsHttpRequestException_OnNonOkResponse()
        {
            var apiUrl = new Uri("https://api.example.com/files");
            int runId = 103;

            var response = CreateCsvResponse(
                Array.Empty<byte>(),
                statusCode: HttpStatusCode.BadRequest);

            _apiServiceMock
                .Setup(x => x.CallApi(
                    It.IsAny<HttpContext>(),
                    HttpMethod.Get,
                    It.IsAny<Uri>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .ReturnsAsync(response);

            await Assert.ThrowsExceptionAsync<HttpRequestException>(() =>
                _fileDownloadService.DownloadFileAsync(apiUrl, runId, _httpContext));
        }

        [TestMethod]
        public async Task DownloadFileAsync_UsesFileNameStar_WhenFileNameIsEmpty()
        {
            var apiUrl = new Uri("https://api.example.com/files");
            int runId = 104;
            var content = Encoding.UTF8.GetBytes("test content");

            var response = CreateCsvResponse(
                content,
                fileName: null,
                fileNameStar: "\"AlternativeFile.csv\"");

            _apiServiceMock
                .Setup(x => x.CallApi(
                    It.IsAny<HttpContext>(),
                    HttpMethod.Get,
                    It.IsAny<Uri>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .ReturnsAsync(response);

            var result = await _fileDownloadService
                .DownloadFileAsync(apiUrl, runId, _httpContext);

            var fileResult = (FileContentResult)result;
            Assert.AreEqual("AlternativeFile.csv", fileResult.FileDownloadName);
        }

        [TestMethod]
        public async Task DownloadFileAsync_UsesDefaultFileName_WhenBothFileNameAndFileNameStarAreEmpty()
        {
            var apiUrl = new Uri("https://api.example.com/files");
            int runId = 105;
            var content = Encoding.UTF8.GetBytes("test content");

            var response = CreateCsvResponse(content);

            _apiServiceMock
                .Setup(x => x.CallApi(
                    It.IsAny<HttpContext>(),
                    HttpMethod.Get,
                    It.IsAny<Uri>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .ReturnsAsync(response);

            var result = await _fileDownloadService
                .DownloadFileAsync(apiUrl, runId, _httpContext);

            var fileResult = (FileContentResult)result;
            Assert.AreEqual($"Result_{runId}.csv", fileResult.FileDownloadName);
        }

        [TestMethod]
        public async Task DownloadFileAsync_UsesDefaultFileName_WhenContentDispositionIsNull()
        {
            var apiUrl = new Uri("https://api.example.com/files");
            int runId = 106;
            var content = Encoding.UTF8.GetBytes("test content");

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(content)
            };
            response.Content.Headers.ContentType =
                new MediaTypeHeaderValue("text/csv");

            _apiServiceMock
                .Setup(x => x.CallApi(
                    It.IsAny<HttpContext>(),
                    HttpMethod.Get,
                    It.IsAny<Uri>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .ReturnsAsync(response);

            var result = await _fileDownloadService
                .DownloadFileAsync(apiUrl, runId, _httpContext);

            var fileResult = (FileContentResult)result;
            Assert.AreEqual($"Result_{runId}.csv", fileResult.FileDownloadName);
        }

        [TestMethod]
        public async Task DownloadFileAsync_AppliesBillingSuffix_WhenFileNameStarUsedAndIsBillingFile()
        {
            var apiUrl = new Uri("https://api.example.com/files");
            int runId = 107;
            var content = Encoding.UTF8.GetBytes("billing content");

            var response = CreateCsvResponse(
                content,
                fileName: string.Empty,
                fileNameStar: "\"BillingFromStar.csv\"");

            _apiServiceMock
                .Setup(x => x.CallApi(
                    It.IsAny<HttpContext>(),
                    HttpMethod.Get,
                    It.IsAny<Uri>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .ReturnsAsync(response);

            var result = await _fileDownloadService
                .DownloadFileAsync(apiUrl, runId, _httpContext, true, true);

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
