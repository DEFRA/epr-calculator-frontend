using System.Security.Claims;
using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using global::EPR.Calculator.Frontend.Common.Constants;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
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

        private Mock<IConfiguration> _mockConfiguration = null!;
        private Mock<ITokenAcquisition> _mockTokenAcquisition = null!;
        private Mock<IHttpClientFactory> _mockHttpClientFactory = null!;
        private Mock<IResultBillingFileService> _mockFileDownloadService = null!;
        private Mock<ICalculatorRunDetailsService> _mockRunDetailsService = null!;
        private TelemetryClient _telemetryClient = null!;

        private FileDownloadController _controller = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockTokenAcquisition = new Mock<ITokenAcquisition>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockFileDownloadService = new Mock<IResultBillingFileService>();
            _mockRunDetailsService = new Mock<ICalculatorRunDetailsService>();
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
                fileDownloadService: _mockFileDownloadService.Object,
                calculatorRunDetailsService: _mockRunDetailsService.Object);
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
               x.DownloadFileAsync(It.IsAny<Uri>(), It.IsAny<int>(), It.IsAny<HttpContext>(), It.IsAny<bool>(), It.IsAny<bool>()))
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

            _mockFileDownloadService
                .Setup(s => s.DownloadFileAsync(It.IsAny<Uri>(), runId, It.IsAny<HttpContext>(),
                                                It.IsAny<bool>(), It.IsAny<bool>()))
                .ThrowsAsync(new Exception("Download error"));

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

            _mockRunDetailsService.Setup(s => s.GetCalculatorRundetailsAsync(It.IsAny<HttpContext>(), It.IsAny<int>()))
                .ReturnsAsync(new CalculatorRunDetailsViewModel() { RunId = 1, RunClassificationId = RunClassification.INITIAL_RUN, RunName = "Test" });

            _mockTokenAcquisition
             .Setup(t => t.GetAccessTokenForUserAsync(
                 It.IsAny<IEnumerable<string>>(),
                 It.IsAny<string>(),
                 It.IsAny<string>(),
                 It.IsAny<ClaimsPrincipal>(),
                 It.IsAny<TokenAcquisitionOptions>()))
             .ReturnsAsync(fakeToken);

            _mockFileDownloadService.Setup(x =>
               x.DownloadFileAsync(It.IsAny<Uri>(), It.IsAny<int>(), It.IsAny<HttpContext>(), It.IsAny<bool>(), It.IsAny<bool>()))
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
            var runId = 789;

            _mockRunDetailsService
                .Setup(s => s.GetCalculatorRundetailsAsync(It.IsAny<HttpContext>(), runId))
                .ThrowsAsync(new Exception("Download billing error"));

            // Act
            var result = await _controller.DownloadBillingFile(runId, true, true);

            // Assert
            var redirect = result as RedirectToActionResult;
            Assert.IsNotNull(redirect);
            Assert.AreEqual(ControllerNames.DownloadFileErrorNewController, redirect.ControllerName);
            Assert.AreEqual(ActionNames.IndexNew, redirect.ActionName);
            Assert.AreEqual(runId, redirect.RouteValues["runId"]);
        }

        [TestMethod]
        public async Task DownloadBillingFile_WithNullRunDetails_RedirectsToStandardError()
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
               x.DownloadFileAsync(It.IsAny<Uri>(), It.IsAny<int>(), It.IsAny<HttpContext>(), It.IsAny<bool>(), It.IsAny<bool>()))
               .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.DownloadBillingFile(runId, isBillingFile, isDraft);

            // Assert
            var redirect = result as RedirectToActionResult;
            Assert.IsNotNull(redirect);
            Assert.AreEqual(ActionNames.StandardErrorIndex, redirect.ActionName);
            Assert.AreEqual(CommonUtil.GetControllerName(typeof(StandardErrorController)), redirect.ControllerName);
        }

        [TestMethod]
        public async Task DownloadBillingFile_WithBillingFileNotGeneratedLatest_RedirectsToIndex()
        {
            // Arrange
            var runId = 789;
            var fakeToken = "mock-token";
            var isBillingFile = true;
            var isDraft = false;
            var expectedResult = new FileContentResult(new byte[] { 9, 8, 7 }, "text/csv");

            _mockRunDetailsService.Setup(s => s.GetCalculatorRundetailsAsync(It.IsAny<HttpContext>(), It.IsAny<int>()))
                .ReturnsAsync(new CalculatorRunDetailsViewModel() { RunId = 1, RunClassificationId = RunClassification.INITIAL_RUN, RunName = "Test", IsBillingFileGeneratedLatest = false });

            _mockTokenAcquisition
             .Setup(t => t.GetAccessTokenForUserAsync(
                 It.IsAny<IEnumerable<string>>(),
                 It.IsAny<string>(),
                 It.IsAny<string>(),
                 It.IsAny<ClaimsPrincipal>(),
                 It.IsAny<TokenAcquisitionOptions>()))
             .ReturnsAsync(fakeToken);

            _mockFileDownloadService.Setup(x =>
               x.DownloadFileAsync(It.IsAny<Uri>(), It.IsAny<int>(), It.IsAny<HttpContext>(), It.IsAny<bool>(), It.IsAny<bool>()))
               .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.DownloadBillingFile(runId, isBillingFile, isDraft) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(ControllerNames.CalculationRunOverview, result.ControllerName);
        }
    }
}
