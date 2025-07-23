using System.Net;
using System.Net.Http.Headers;
using System.Text;
using EPR.Calculator.Frontend.Services;
using Microsoft.ApplicationInsights;
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
        private FileDownloadService _fileDownloadService;

        public FileDownloadServiceTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _configurationMock = new Mock<IConfiguration>();
            _telemetryClient = new TelemetryClient();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            var client = new HttpClient(_httpMessageHandlerMock.Object);
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);
            // _configurationMock.Setup(c => c.GetValue<int>("CalculationRunSettings:DownloadResultTimeoutInMilliSeconds"))
            //    .Returns(TimeoutMs);
            _configurationMock.Setup(c => c.GetSection("CalculationRunSettings:DownloadResultTimeoutInMilliSeconds").Value)
                .Returns(TimeoutMs.ToString());

            _fileDownloadService = new FileDownloadService(
                _httpClientFactoryMock.Object,
                _configurationMock.Object,
                _telemetryClient);
        }

        [TestMethod]
        public async Task DownloadFileAsync_ReturnsFileResult_WithCorrectFileName()
        {
            // Arrange
            var apiUrl = new Uri("https://api.example.com/files");
            int runId = 100;
            string token = "Bearer token";
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

            _httpMessageHandlerMock
                .SetupSendAsync(HttpMethod.Get, $"{apiUrl}/{runId}", response);

            // Act
            var result = await _fileDownloadService.DownloadFileAsync(apiUrl, runId, token);

            // Assert
            Assert.IsInstanceOfType(result, typeof(FileContentResult));
            var fileResult = result as FileContentResult;
            Assert.AreEqual("Result_100.csv", fileResult.FileDownloadName);
            CollectionAssert.AreEqual(content, fileResult.FileContents);
        }

        [TestMethod]
        public async Task DownloadFileAsync_Appends_DRAFT_IfDraftBilling()
        {
            var apiUrl = new Uri("https://api.example.com/files");
            int runId = 101;
            string token = "Bearer token";
            byte[] content = Encoding.UTF8.GetBytes("draft billing");

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(content)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "\"BillingRun.csv\""
            };

            _httpMessageHandlerMock
                .SetupSendAsync(HttpMethod.Get, $"{apiUrl}/{runId}", response);

            var result = await _fileDownloadService.DownloadFileAsync(apiUrl, runId, token, true, true);
            var fileResult = result as FileContentResult;

            Assert.AreEqual("BillingRun_DRAFT.csv", fileResult.FileDownloadName);
        }

        [TestMethod]
        public async Task DownloadFileAsync_Appends_AUTHORISED_IfNotDraft()
        {
            var apiUrl = new Uri("https://api.example.com/files");
            int runId = 102;
            string token = "Bearer token";
            byte[] content = Encoding.UTF8.GetBytes("auth billing");

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(content)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "\"BillingRun.csv\""
            };

            _httpMessageHandlerMock
                .SetupSendAsync(HttpMethod.Get, $"{apiUrl}/{runId}", response);

            var result = await _fileDownloadService.DownloadFileAsync(apiUrl, runId, token, true, false);
            var fileResult = result as FileContentResult;

            Assert.AreEqual("BillingRun_AUTHORISED.csv", fileResult.FileDownloadName);
        }

        [TestMethod]
        public async Task DownloadFileAsync_ThrowsArgumentException_WhenRunIdInvalid()
        {
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            {
                await _fileDownloadService.DownloadFileAsync(new Uri("https://api.test.com"), 0, "token");
            });
        }

        [TestMethod]
        public async Task DownloadFileAsync_ThrowsHttpRequestException_OnNonOkResponse()
        {
            var apiUrl = new Uri("https://api.example.com/files");
            int runId = 103;
            string token = "Bearer token";

            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            _httpMessageHandlerMock
                .SetupSendAsync(HttpMethod.Get, $"{apiUrl}/{runId}", response);

            await Assert.ThrowsExceptionAsync<HttpRequestException>(async () =>
            {
                await _fileDownloadService.DownloadFileAsync(apiUrl, runId, token);
            });
        }
    }
}
