using System.Net;
using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class LocalAuthorityUploadFileProcessingControllerTests
    {
        public LocalAuthorityUploadFileProcessingControllerTests()
        {
            this.Fixture = new Fixture();
            this.MockSesion = TestMockUtils.BuildMockSession(this.Fixture);
            this.MockHttpContext = new Mock<HttpContext>();
            this.MockHttpContext.Setup(c => c.User.Identity!.Name).Returns(Fixture.Create<string>);
            this.MockHttpContext.Setup(c => c.Session).Returns(this.MockSesion.Object);

            this.Configuration = TestMockUtils.BuildConfiguration();

            this.MockMessageHandler = TestMockUtils.BuildMockMessageHandler(HttpStatusCode.Accepted, new StringContent("response content"));
            Mock<IHttpClientFactory> mockHttpClientFactory = TestMockUtils.BuildMockHttpClientFactory(
                this.MockMessageHandler.Object);
            this.TestClass = new LocalAuthorityUploadFileProcessingController(
                this.Configuration,
                new Mock<IEprCalculatorApiService>().Object,
                new TelemetryClient(new Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration()),
                new Mock<ICalculatorRunDetailsService>().Object)
            {
                TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>()),
            };
            this.TestClass.ControllerContext = new ControllerContext { HttpContext = this.MockHttpContext.Object };
        }

        private Fixture Fixture { get; init; }

        private Mock<HttpContext> MockHttpContext { get; init; }

        private Mock<HttpMessageHandler> MockMessageHandler { get; init; }

        private Mock<ISession> MockSesion { get; init; }

        private IConfiguration Configuration { get; init; }

        private LocalAuthorityUploadFileProcessingController TestClass { get; init; }

        [TestMethod]
        public async Task LocalAuthorityUploadFileProcessingController_Success_Result_Test()
        {
            // Arrange
            var controller = BuildTestClass(
                Fixture,
                HttpStatusCode.Created,
                new StringContent("response content")).Controller;

            var viewModel = new LapcapRefreshViewModel()
            {
                LapcapTemplateValue = MockData.GetLocalAuthorityDisposalCostsToUpload().ToList(),
                FileName = "Test Name",
            };

            // Act
            var result = (OkObjectResult)(await controller.Index(viewModel));

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [TestMethod]
        public async Task LocalAuthorityUploadFileProcessingController_Failure_Result_Test()
        {
            // Arrange
            var controller = BuildTestClass(
                Fixture,
                HttpStatusCode.BadRequest,
                new StringContent("response content")).Controller;

            var viewModel = new LapcapRefreshViewModel()
            {
                LapcapTemplateValue = MockData.GetLocalAuthorityDisposalCostsToUpload().ToList(),
                FileName = "Test Name",
            };

            // Act
            var result = (BadRequestObjectResult)(await controller.Index(viewModel));

            // Assert
            Assert.IsNotNull(result);
            Assert.AreNotEqual(201, result.StatusCode);
        }

        [TestMethod]
        public async Task Index_SendDateFromSession()
        {
            // Arrange
            var currentYear = CommonUtil.GetDefaultRelativeYear(DateTime.UtcNow, 4);
            var viewModel = new LapcapRefreshViewModel();
            var (controller, mockApiService) = BuildTestClass(
                Fixture,
                HttpStatusCode.OK);

            // Act
            var result = await controller.Index(new LapcapRefreshViewModel());
            mockApiService.Verify(
                s => s.CallApi(
                    It.IsAny<HttpContext>(),
                    It.IsAny<HttpMethod>(),
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string?>>(),
                    new CreateLapcapDataDto(new LapcapRefreshViewModel(), currentYear)),
                Times.Once());
        }

        [TestMethod]
        public async Task Index_DefaultToCurrentYearWhenNoRelativeYearInSession()
        {
            var currentYear = CommonUtil.GetDefaultRelativeYear(DateTime.UtcNow, 4);
            var viewModel = new LapcapRefreshViewModel();

            var (controller, mockApiService) = BuildTestClass(
                Fixture,
                HttpStatusCode.OK);

            // Act
            var result = await controller.Index(viewModel);

            // Assert
            Assert.IsFalse(this.MockSesion.Object.Keys.Contains(SessionConstants.RelativeYear));
            mockApiService.Verify(
                s => s.CallApi(
                    It.IsAny<HttpContext>(),
                    It.IsAny<HttpMethod>(),
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string?>>(),
                    new CreateLapcapDataDto(viewModel, currentYear)),
                Times.Once());
        }

        private (
            LocalAuthorityUploadFileProcessingController Controller,
            Mock<IEprCalculatorApiService> MockApiService) BuildTestClass(
            Fixture fixture,
            HttpStatusCode httpStatusCode,
            object? data = null,
            CalculatorRunDetailsViewModel? details = null,
            IConfiguration? configurationItems = null)
        {
            data ??= MockData.GetCalculatorRun();
            configurationItems ??= ConfigurationItems.GetConfigurationValues();
            details ??= Fixture.Create<CalculatorRunDetailsViewModel>();
            var mockApiService = TestMockUtils.BuildMockApiService(
                httpStatusCode,
                JsonConvert.SerializeObject(data ?? MockData.GetCalculatorRun()));

            var testClass = new LocalAuthorityUploadFileProcessingController(
                configurationItems,
                mockApiService.Object,
                new TelemetryClient(new Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration()),
                TestMockUtils.BuildMockCalculatorRunDetailsService(details).Object);
            testClass.ControllerContext.HttpContext = new DefaultHttpContext()
            {
                Session = TestMockUtils.BuildMockSession(fixture).Object,
            };

            return (testClass, mockApiService);
        }
    }
}