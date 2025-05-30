﻿using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class CalculationRunDetailsControllerTests
    {
        private readonly IConfiguration _configuration = ConfigurationItems.GetConfigurationValues();
        private Mock<IHttpClientFactory> _mockClientFactory;
        private Mock<ILogger<CalculationRunDetailsController>> _mockLogger;

        public CalculationRunDetailsControllerTests()
        {
            this.Fixture = new Fixture();
            this.MockHttpContext = new Mock<HttpContext>();
            this.MockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
        }

        private Fixture Fixture { get; }

        private Mock<HttpContext> MockHttpContext { get; }

        [TestInitialize]
        public void Setup()
        {
            _mockClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<CalculationRunDetailsController>>();
        }

        [TestMethod]
        public async Task IndexAsync_ReturnsView_WhenApiCallIsSuccessful()
        {
            // Arrange
            var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, MockData.GetCalculatorRun());
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var mockClient = new TelemetryClient();

            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);
            var mockHttpSession = new MockHttpSession();
            mockHttpSession.SetString("accessToken", "something");

            var context = new DefaultHttpContext()
            {
                User = principal,
                Session = mockHttpSession
            };

            var controller = new CalculationRunDetailsController(_configuration, _mockClientFactory.Object,
                _mockLogger.Object, new Mock<ITokenAcquisition>().Object, mockClient);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };
            controller.ControllerContext.HttpContext.Request.Scheme = "https";
            controller.ControllerContext.HttpContext.Request.Host = new HostString("localhost:7163");
            int runId = 1;
            string calcName = "Test Run";

            // Act
            var result = await controller.IndexAsync(runId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunDetailsIndex, result.ViewName);
            var model = result.Model as CalculatorRunStatusUpdateViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(runId, model.Data.RunId);
            Assert.AreEqual((int)RunClassification.UNCLASSIFIED, model.Data.ClassificationId);
            Assert.AreEqual(calcName, model.Data.CalcName);
            Assert.AreEqual(new Uri("http://localhost:5055/v1/DownloadResult/1"), model.DownloadResultURL);
            Assert.AreEqual("/DownloadFileError/1", model.DownloadErrorURL);
            Assert.AreEqual(30000, model.DownloadTimeout);
            Assert.AreEqual("12:09", model.Data.CreatedTime);
            Assert.AreEqual("21 Jun 2024", model.Data.CreatedDate);
        }

        [TestMethod]
        public async Task IndexAsync_ReturnsRunDetailsErrorPage_WhenRunIdStatusIsNotValid()
        {
            // Arrange
            var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, MockData.GetRunningCalculatorRun());
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var mockClient = new TelemetryClient();

            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);
            var mockHttpSession = new MockHttpSession();
            mockHttpSession.SetString("accessToken", "something");

            var context = new DefaultHttpContext()
            {
                User = principal,
                Session = mockHttpSession
            };

            var controller = new CalculationRunDetailsController(_configuration, _mockClientFactory.Object,
                _mockLogger.Object, new Mock<ITokenAcquisition>().Object, mockClient);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };
            controller.ControllerContext.HttpContext.Request.Scheme = "https";
            controller.ControllerContext.HttpContext.Request.Host = new HostString("localhost:7163");
            int runId = 1;
            string calcName = "Test Run";

            // Act
            var result = await controller.IndexAsync(runId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunDetailsErrorPage, result.ViewName);
        }

        [TestMethod]
        public async Task IndexAsync_ThrowError_WhenRunIdIsNotValid()
        {
            // Arrange
            var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, null);
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var mockClient = new TelemetryClient();

            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);
            var mockHttpSession = new MockHttpSession();
            mockHttpSession.SetString("accessToken", "something");

            var context = new DefaultHttpContext()
            {
                User = principal,
                Session = mockHttpSession
            };

            var controller = new CalculationRunDetailsController(_configuration, _mockClientFactory.Object,
                _mockLogger.Object, new Mock<ITokenAcquisition>().Object, mockClient);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };
            controller.ControllerContext.HttpContext.Request.Scheme = "https";
            controller.ControllerContext.HttpContext.Request.Host = new HostString("localhost:7163");
            int runId = 1;
            string calcName = "Test Run";

            // Act
            var result = await controller.IndexAsync(runId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual(CommonUtil.GetControllerName(typeof(StandardErrorController)), result.ControllerName);
        }

        [TestMethod]
        public async Task IndexAsync_RedirectsToError_WhenApiCallFails()
        {
            // Arrange
            var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.BadRequest, MockData.GetCalculationRuns());
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
            var mockTokenAcquisition = new Mock<ITokenAcquisition>();
            var controller = new CalculationRunDetailsController(_configuration, _mockClientFactory.Object,
                _mockLogger.Object, mockTokenAcquisition.Object, new TelemetryClient());
            int runId = 1;

            // Act
            var result = await controller.IndexAsync(runId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual(CommonUtil.GetControllerName(typeof(StandardErrorController)), result.ControllerName);
        }

        [TestMethod]
        public void DeleteCalcDetails_ReturnsView_WhenDeleteRadioIsNotChecked()
        {
            // Arrange
            var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, MockData.GetCalculationRuns());
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
            var mockTokenAcquisition = new Mock<ITokenAcquisition>();
            var controller = new CalculationRunDetailsController(_configuration, _mockClientFactory.Object,
                _mockLogger.Object, mockTokenAcquisition.Object, new TelemetryClient());

            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);
            var mockHttpSession = new MockHttpSession();
            mockHttpSession.SetString("accessToken", "something");

            var context = new DefaultHttpContext()
            {
                User = principal,
                Session = mockHttpSession
            };
            controller.ControllerContext.HttpContext = context;
            controller.ControllerContext.HttpContext.Request.Scheme = "https";
            controller.ControllerContext.HttpContext.Request.Host = new HostString("localhost:7163");

            int runId = 1;
            string calcName = "TestCalc";
            string calDate = "21 June 2024";
            string calTime = "12:09";

            // Act
            var task = controller.DeleteCalculation(runId, calcName, calDate, calTime, false);
            task.Wait();
            var result = task.Result as ViewResult;

            var errorViewModel = result.ViewData.Model as CalculatorRunStatusUpdateViewModel;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunDetailsIndex, result.ViewName);
            Assert.AreEqual(ViewControlNames.DeleteCalculationName, errorViewModel.Errors.DOMElementId);
            Assert.AreEqual(ErrorMessages.SelectDeleteCalculation, errorViewModel.Errors.ErrorMessage);
        }

        [TestMethod]
        public void DeleteCalculation_ReturnsView_WhenApiCallIsUnsuccessful()
        {
            // Arrange
            var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.RequestTimeout, MockData.GetCalculationRuns());
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
            var mockTokenAcquisition = new Mock<ITokenAcquisition>();

            var controller = new CalculationRunDetailsController(_configuration, _mockClientFactory.Object,
                _mockLogger.Object, mockTokenAcquisition.Object, new TelemetryClient());
            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);
            var mockHttpSession = new MockHttpSession();
            mockHttpSession.SetString("accessToken", "something");

            var context = new DefaultHttpContext()
            {
                User = principal,
                Session = mockHttpSession
            };
            controller.ControllerContext.HttpContext = context;
            controller.ControllerContext.HttpContext.Request.Scheme = "https";
            controller.ControllerContext.HttpContext.Request.Host = new HostString("localhost:7163");

            int runId = 1;
            string calcName = "TestCalc";
            string calDate = "21 June 2024";
            string calTime = "12:09";

            // Act
            var task = controller.DeleteCalculation(runId, calcName, calDate, calTime, true);
            task.Wait();
            var result = task.Result as ViewResult;

            var errorViewModel = result.ViewData.Model as CalculatorRunStatusUpdateViewModel;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunDetailsIndex, result.ViewName);
            Assert.AreEqual(ViewControlNames.DeleteCalculationName, errorViewModel.Errors.DOMElementId);
            Assert.AreEqual(ErrorMessages.DeleteCalculationError, errorViewModel.Errors.ErrorMessage);
        }

        [TestMethod]
        public void DeleteCalculation_ReturnsView_WhenApiCallIsSuccessful()
        {
            // Arrange
            var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.Created, MockData.GetCalculationRuns());
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
            var mockTokenAcquisition = new Mock<ITokenAcquisition>();

            var controller = new CalculationRunDetailsController(_configuration, _mockClientFactory.Object,
                _mockLogger.Object, mockTokenAcquisition.Object, new TelemetryClient());

            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);
            var mockHttpSession = new MockHttpSession();
            mockHttpSession.SetString("accessToken", "something");

            var context = new DefaultHttpContext()
            {
                User = principal,
                Session = mockHttpSession
            };

            controller.ControllerContext.HttpContext = context;
            controller.ControllerContext.HttpContext.Request.Scheme = "https";
            controller.ControllerContext.HttpContext.Request.Host = new HostString("localhost:7163");

            int runId = 1;
            string calcName = "TestCalc";
            string calDate = "21 June 2024";
            string calTime = "12:09";

            // Act
            var task = controller.DeleteCalculation(runId, calcName, calDate, calTime, true);
            task.Wait();

            var result = task.Result as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.DeleteConfirmation, result.ViewName);
        }

        [TestMethod]
        public async Task IndexAsync_GetCalculationDetailsResponseIsNull_ShouldLogErrorAndRedirect()
        {
            // Arrange
            var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.InternalServerError, MockData.GetCalculationRuns());
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var mockTokenAcquisition = new Mock<ITokenAcquisition>();
            var mockClient = new TelemetryClient();
            var controller = new CalculationRunDetailsController(_configuration, _mockClientFactory.Object,
                _mockLogger.Object, mockTokenAcquisition.Object, mockClient);
            int runId = 1;

            // Act
            var result = await controller.IndexAsync(runId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
            _mockLogger.Verify(
               logger => logger.Log(
                   LogLevel.Error,
                   It.IsAny<EventId>(),
                   It.Is<It.IsAnyType>((state, t) => state.ToString().Contains($"Request failed with status code {HttpStatusCode.InternalServerError}")),
                   null,
                   It.IsAny<Func<It.IsAnyType, Exception, string>>()),
               Times.Once);
        }

        [TestMethod]
        public async Task IndexAsync_GetCalculationDetails_Exception_ShouldLogErrorAndRedirect()
        {
            // Arrange
            var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.InternalServerError, MockData.GetCalculationRuns());
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var mockClient = new Mock<ITokenAcquisition>();
            var controller =
                new CalculationRunDetailsController(null, null, _mockLogger.Object, mockClient.Object,
                    new TelemetryClient());
            int runId = 1;

            // Act
            var result = await controller.IndexAsync(runId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        [TestMethod]
        public async Task DeleteAsync_GetCalculationDetails_Exception_ShouldLogErrorAndRedirect()
        {
            // Arrange
            var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.InternalServerError, MockData.GetCalculationRuns());
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var mockTokenAcquisition = new Mock<ITokenAcquisition>();

            var controller = new CalculationRunDetailsController(null, null, _mockLogger.Object,
                mockTokenAcquisition.Object, new TelemetryClient());
            int runId = 1;
            string calcName = "TestCalc";
            string calDate = "21 June 2024";
            string calTime = "12:09";

            // Act
            var task = controller.DeleteCalculation(runId, calcName, calDate, calTime, true);
            task.Wait();

            var result = task.Result as RedirectToActionResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        [TestMethod]
        public async Task IndexAsync_ReturnsView_WhenApiCallIsSuccessfulForInitialRun()
        {
            // Arrange
            var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, MockData.GetInitialRunCalculatorRun());
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var mockClient = new TelemetryClient();

            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);
            var mockHttpSession = new MockHttpSession();
            mockHttpSession.SetString("accessToken", "something");

            var context = new DefaultHttpContext()
            {
                User = principal,
                Session = mockHttpSession
            };

            var controller = new CalculationRunDetailsController(_configuration, _mockClientFactory.Object,
                _mockLogger.Object, new Mock<ITokenAcquisition>().Object, mockClient);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };
            controller.ControllerContext.HttpContext.Request.Scheme = "https";
            controller.ControllerContext.HttpContext.Request.Host = new HostString("localhost:7163");
            int runId = 1;
            string calcName = "Test Run";

            // Act
            var result = await controller.IndexAsync(runId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunDetailsIndex, result.ViewName);
            var model = result.Model as CalculatorRunStatusUpdateViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(runId, model.Data.RunId);
            Assert.AreEqual((int)RunClassification.INITIAL_RUN, model.Data.ClassificationId);
            Assert.AreEqual(calcName, model.Data.CalcName);
            Assert.AreEqual(new Uri("http://localhost:5055/v1/DownloadResult/1"), model.DownloadResultURL);
            Assert.AreEqual("/DownloadFileError/1", model.DownloadErrorURL);
            Assert.AreEqual(30000, model.DownloadTimeout);
            Assert.AreEqual("12:09", model.Data.CreatedTime);
            Assert.AreEqual("21 Jun 2024", model.Data.CreatedDate);
        }

        private static Mock<HttpMessageHandler> CreateMockHttpMessageHandler(HttpStatusCode statusCode, object content)
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
                    StatusCode = statusCode,
                    Content = new StringContent(JsonConvert.SerializeObject(content))
                });

            return mockHttpMessageHandler;
        }
    }
}