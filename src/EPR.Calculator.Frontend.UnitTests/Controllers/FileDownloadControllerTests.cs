using System.Security.Claims;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Services;
using global::EPR.Calculator.Frontend.Common.Constants;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests.Controllers
{
    [TestClass]
    public class FileDownloadControllerTests
    {
        private const string ResultFileUrl = "https://fake-api.com/result";
        private const string BillingFileUrl = "https://fake-api.com/billing";

        private Mock<IConfiguration> _mockConfiguration;
        private Mock<ITokenAcquisition> _mockTokenAcquisition;
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private Mock<IResultBillingFileService> _mockFileDownloadService;
        private TelemetryClient _telemetryClient;

        private FileDownloadController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockTokenAcquisition = new Mock<ITokenAcquisition>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockFileDownloadService = new Mock<IResultBillingFileService>();
            _telemetryClient = new TelemetryClient();

            // Setup for DownloadResultApi
            var mockResultApiSection = new Mock<IConfigurationSection>();
            mockResultApiSection.Setup(x => x.Value).Returns(ResultFileUrl);

            var mockBillingApiSection = new Mock<IConfigurationSection>();
            mockBillingApiSection.Setup(x => x.Value).Returns(BillingFileUrl);

            var mockCalcRunSettingsSection = new Mock<IConfigurationSection>();
            mockCalcRunSettingsSection.Setup(x => x.GetSection(ConfigSection.DownloadResultApi))
                .Returns(mockResultApiSection.Object);
            mockCalcRunSettingsSection.Setup(x => x.GetSection(ConfigSection.DownloadCsvBillingApi))
                .Returns(mockBillingApiSection.Object);

            _mockConfiguration.Setup(x => x.GetSection(ConfigSection.CalculationRunSettings))
                .Returns(mockCalcRunSettingsSection.Object);

            // Setup the nested section "DownstreamApi:Scopes"
            var mockDownstreamApiSection = new Mock<IConfigurationSection>();
            mockDownstreamApiSection.Setup(s => s.Value).Returns("scope1 scope2");

            _mockConfiguration
                .Setup(c => c.GetSection("DownstreamApi:Scopes"))
                .Returns(mockDownstreamApiSection.Object);

            _controller = new FileDownloadController(
                tokenAcquisition: _mockTokenAcquisition.Object,
                configuration: _mockConfiguration.Object,
                apiService: new Mock<IApiService>().Object,
                telemetryClient: _telemetryClient,
                fileDownloadService: _mockFileDownloadService.Object);
        }

        [TestMethod]
        public async Task DownloadResultFile_Returns_FileContentResult()
        {
            // Arrange
            var runId = 123;
            var fakeToken = "mock-token";
            var expectedResult = new FileContentResult(new byte[] { 1, 2, 3 }, "application/octet-stream");

            _mockTokenAcquisition
            .Setup(t => t.GetAccessTokenForUserAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<TokenAcquisitionOptions>()))
            .ReturnsAsync(fakeToken);

            _mockFileDownloadService.Setup(x =>
               x.DownloadFileAsync(It.IsAny<Uri>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
               .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.DownloadResultFile(runId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(FileContentResult));
        }

        [TestMethod]
        public async Task DownloadResultFile_WhenException_ReturnsRedirect()
        {
            // Arrange
            var runId = 456;

            _mockTokenAcquisition.Setup(x =>
                x.GetAccessTokenForUserAsync(
                    It.IsAny<IEnumerable<string>>(),
                    null, null, (ClaimsPrincipal)null, null))
                .ThrowsAsync(new Exception("Auth error"));

            // Act
            var result = await _controller.DownloadResultFile(runId);

            // Assert
            var redirect = result as RedirectToActionResult;
            Assert.IsNotNull(redirect);
            Assert.AreEqual(ControllerNames.DownloadFileErrorNewController, redirect.ControllerName);
            Assert.AreEqual(ActionNames.IndexNew, redirect.ActionName);
            Assert.AreEqual(runId, redirect.RouteValues["runId"]);
        }

        [TestMethod]
        public async Task DownloadBillingFile_Returns_FileContentResult()
        {
            // Arrange
            var runId = 789;
            var fakeToken = "mock-token";
            var isBillingFile = true;
            var isDraft = false;
            var expectedResult = new FileContentResult(new byte[] { 9, 8, 7 }, "text/csv");

            _mockTokenAcquisition
             .Setup(t => t.GetAccessTokenForUserAsync(
                 It.IsAny<IEnumerable<string>>(),
                 It.IsAny<string>(),
                 It.IsAny<string>(),
                 It.IsAny<ClaimsPrincipal>(),
                 It.IsAny<TokenAcquisitionOptions>()))
             .ReturnsAsync(fakeToken);

            _mockFileDownloadService.Setup(x =>
               x.DownloadFileAsync(It.IsAny<Uri>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
               .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.DownloadBillingFile(runId, isBillingFile, isDraft);

            // Assert
            Assert.IsInstanceOfType(result, typeof(FileContentResult));
        }

        [TestMethod]
        public async Task DownloadBillingFile_WhenException_ReturnsRedirect()
        {
            // Arrange
            var runId = 987;
            var isBillingFile = false;
            var isDraft = true;

            _mockTokenAcquisition.Setup(x =>
                x.GetAccessTokenForUserAsync(
                    It.IsAny<IEnumerable<string>>(),
                    null, null, (ClaimsPrincipal)null, null))
                .ThrowsAsync(new Exception("Billing failure"));

            // Act
            var result = await _controller.DownloadBillingFile(runId, isBillingFile, isDraft);

            // Assert
            var redirect = result as RedirectToActionResult;
            Assert.IsNotNull(redirect);
            Assert.AreEqual(ControllerNames.DownloadFileErrorNewController, redirect.ControllerName);
            Assert.AreEqual(ActionNames.IndexNew, redirect.ActionName);
            Assert.AreEqual(runId, redirect.RouteValues["runId"]);
        }
    }
}
