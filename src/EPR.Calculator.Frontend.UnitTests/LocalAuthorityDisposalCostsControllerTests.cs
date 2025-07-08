using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class LocalAuthorityDisposalCostsControllerTests
    {
        public LocalAuthorityDisposalCostsControllerTests()
        {
            this.Fixture = new Fixture();
            this.MockHttpContext = new Mock<HttpContext>();
            this.MockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
        }

        private Fixture Fixture { get; init; }

        private Mock<HttpContext> MockHttpContext { get; init; }

        [TestMethod]
        public async Task LocalAuthorityDisposalCostsController_Success_View_Test()
        {
            // Arrange
            var controller = BuildTestClass(
                Fixture,
                HttpStatusCode.OK,
                MockData.GetLocalAuthorityDisposalCosts(),
                null,
                ConfigurationItems.GetConfigurationValues());

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task LocalAuthorityDisposalCostsController_Success_No_Data_View_Test()
        {
            // Arrange
            var content = "No data available for the specified year.Please check the year and try again.";
            var controller = BuildTestClass(
                Fixture,
                HttpStatusCode.NotFound,
                new StringContent(content),
                null,
                ConfigurationItems.GetConfigurationValues());

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task LocalAuthorityDisposalCostsController_Failure_View_Test()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                   .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Test content")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            // Mock IHttpClientFactory
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var mockTokenAcquisition = new Mock<ITokenAcquisition>();
            var controller = new LocalAuthorityDisposalCostsController(
                ConfigurationItems.GetConfigurationValues(),
                new Mock<IApiService>().Object,
                mockTokenAcquisition.Object,
                new TelemetryClient(),
                new Mock<ICalculatorRunDetailsService>().Object);

            var result = controller.Index() as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        [TestMethod]
        public void Index_WhenExceptionThrown_RedirectsToErrorPage()
        {
            var mockTokenAcquisition = new Mock<ITokenAcquisition>();
            // Arrange
            var controller =
                new LocalAuthorityDisposalCostsController(
                    null,
                    null,
                    mockTokenAcquisition.Object,
                    new TelemetryClient(),
                    new Mock<ICalculatorRunDetailsService>().Object);

            // Act
            var result = controller.Index() as RedirectToActionResult;

            // Assert
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        [TestMethod]
        public async Task Index_SuccessfulResponse_ReturnsViewWithGroupedData()
        {
            // Arrange
            var controller = BuildTestClass(
                Fixture,
                HttpStatusCode.OK,
                MockData.GetLocalAuthorityDisposalCosts(),
                null,
                ConfigurationItems.GetConfigurationValues());

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.LocalAuthorityDisposalCostsIndex, result.ViewName);
            Assert.IsInstanceOfType(result.Model, typeof(LocalAuthorityViewModel));
        }

        [TestMethod]
        public async Task LocalAuthorityDisposalCostsController_No_Config_Failure_View_Test()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                   .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Test content")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            // Mock IHttpClientFactory
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var mockTokenAcquisition = new Mock<ITokenAcquisition>();
            var controller = new LocalAuthorityDisposalCostsController(
                ConfigurationItems.GetConfigurationValuesWithEmptyStrings(),
                new Mock<IApiService>().Object,
                mockTokenAcquisition.Object,
                new TelemetryClient(),
                new Mock<ICalculatorRunDetailsService>().Object);

            var result = controller.Index() as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        private LocalAuthorityDisposalCostsController BuildTestClass(
            Fixture fixture,
            HttpStatusCode httpStatusCode,
            object data = null,
            CalculatorRunDetailsViewModel details = null,
            IConfiguration configurationItems = null)
        {
            data = data ?? MockData.GetCalculatorRun();
            configurationItems = configurationItems ?? ConfigurationItems.GetConfigurationValues();
            details = details ?? Fixture.Create<CalculatorRunDetailsViewModel>();
            var mockApiService = TestMockUtils.BuildMockApiService(
                httpStatusCode,
                System.Text.Json.JsonSerializer.Serialize(data ?? MockData.GetCalculatorRun())).Object;

            var testClass = new LocalAuthorityDisposalCostsController(
                configurationItems,
                mockApiService,
                new Mock<ITokenAcquisition>().Object,
                new TelemetryClient(),
                TestMockUtils.BuildMockCalculatorRunDetailsService(details).Object);
            testClass.ControllerContext.HttpContext = new DefaultHttpContext()
            {
                Session = TestMockUtils.BuildMockSession(fixture).Object,
            };

            return testClass;
        }
    }
}