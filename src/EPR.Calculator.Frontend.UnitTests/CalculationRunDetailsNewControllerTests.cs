﻿using System.Net;
using System.Security.Claims;
using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using EPR.Calculator.Frontend.ViewModels;
using EPR.Calculator.Frontend.ViewModels.Enums;
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
    public class CalculationRunDetailsNewControllerTests
    {
        private static readonly string[] Separator = new string[] { @"bin\" };
        private readonly IConfiguration _configuration = ConfigurationItems.GetConfigurationValues();
        private Mock<IHttpClientFactory> _mockClientFactory;
        private Mock<ILogger<CalculationRunDetailsNewController>> _mockLogger;
        private Mock<ITokenAcquisition> _mockTokenAcquisition;
        private TelemetryClient _mockTelemetryClient;
        private CalculationRunDetailsNewController _controller;
        private Mock<HttpMessageHandler> _mockMessageHandler;
        private Mock<HttpContext> _mockHttpContext;

        private Fixture Fixture { get; } = new Fixture();

        [TestInitialize]
        public void Setup()
        {
            _mockHttpContext = new Mock<HttpContext>();
            _mockClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<CalculationRunDetailsNewController>>();
            _mockTokenAcquisition = new Mock<ITokenAcquisition>();
            _mockTelemetryClient = new TelemetryClient();
            _mockMessageHandler = new Mock<HttpMessageHandler>();

            var mockSession = new MockHttpSession();
            _mockHttpContext.Setup(s => s.Session).Returns(mockSession);
            _mockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
            _mockTokenAcquisition
                .Setup(x => x.GetAccessTokenForUserAsync(It.IsAny<IEnumerable<string>>(), null, null, null, null))
                .ReturnsAsync("somevalue");
            mockSession.SetString("accessToken", "something");

            _controller = new CalculationRunDetailsNewController(
                       _configuration,
                       _mockClientFactory.Object,
                       _mockTokenAcquisition.Object,
                       _mockTelemetryClient);

            _mockHttpContext.Setup(context => context.User)
               .Returns(new ClaimsPrincipal(new ClaimsIdentity(
           [
               new Claim(ClaimTypes.Name, "Test User")
           ])));

            // Setting the mocked HttpContext for the controller
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            };
        }

        [TestMethod]
        public async Task Index_ValidRunId_ReturnsViewResult()
        {
            // Setup
            _mockMessageHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                   {
                       StatusCode = HttpStatusCode.OK,
                       Content = new StringContent(JsonConvert.SerializeObject(MockData.GetCalculatorRun())),
                   });
            _mockClientFactory = TestMockUtils.BuildMockHttpClientFactory(_mockMessageHandler.Object);

            _controller = new CalculationRunDetailsNewController(
                _configuration,
                _mockClientFactory.Object,
                _mockTokenAcquisition.Object,
                _mockTelemetryClient);

            // Setting the mocked HttpContext for the controller
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            };

            // Act
            var result = await _controller.Index(10) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunDetailsNewIndex, result.ViewName);
        }

        [TestMethod]
        public async Task Index_InvalidModelState_ReturnsRedirectToAction()
        {
            // Setup
            _mockMessageHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                   {
                       StatusCode = HttpStatusCode.NotFound,
                       Content = new StringContent(string.Empty),
                   });
            _mockClientFactory = TestMockUtils.BuildMockHttpClientFactory(_mockMessageHandler.Object);

            _controller = new CalculationRunDetailsNewController(
                _configuration,
                _mockClientFactory.Object,
                _mockTokenAcquisition.Object,
                _mockTelemetryClient);

            // Setting the mocked HttpContext for the controller
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            };

            // Arrange
            var runId = 10;

            // Act
            var result = await _controller.Index(runId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual(CommonUtil.GetControllerName(typeof(StandardErrorController)), result.ControllerName);
        }

        [TestMethod]
        public async Task Submit_ValidModelState_RedirectsToCorrectAction()
        {
            // Arrange
            int runId = 240008;
            var selectedOption = CalculationRunOption.OutputClassify;

            var model = new CalculatorRunDetailsNewViewModel()
            {
                CalculatorRunDetails = new CalculatorRunDetailsViewModel()
                {
                    RunId = runId,
                    RunName = "Test Run"
                },
                SelectedCalcRunOption = selectedOption
            };

            // Act
            var result = await _controller.Submit(model) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(ControllerNames.ClassifyingCalculationRun, result.ControllerName);
        }

        [TestMethod]
        public async Task Submit_InValidModelState_OutputClassify_ReturnsRedirectToAction()
        {
            var calcRunDto = MockData.GetCalculatorRun();
            calcRunDto.RunClassificationId = 2;

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(JsonConvert.SerializeObject(calcRunDto)),
                    });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                    .Returns(httpClient);
            var config = GetConfigurationValues();
            config.GetSection("ParameterSettings").GetSection("DefaultParameterSettingsApi").Value = string.Empty;
            var mockTokenAcquisition = new Mock<ITokenAcquisition>();
            var controller = new CalculationRunDetailsNewController(config, mockHttpClientFactory.Object,
                mockTokenAcquisition.Object, new TelemetryClient());

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            };

            var viewModel = new ParameterRefreshViewModel()
            {
                ParameterTemplateValues = MockData.GetSchemeParameterTemplateValues().ToList(),
                FileName = "Test Name",
            };

            var task = controller.Index(10);
            task.Wait();
            var result = task.Result as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunDetailsNewIndex, result.ViewName);
        }

        [TestMethod]
        public async Task Submit_ErrorClassificationState_OutputClassify_ReturnsRedirectToAction()
        {
            var calcRunDto = MockData.GetCalculatorRun();
            calcRunDto.RunClassificationId = 5;

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(JsonConvert.SerializeObject(calcRunDto)),
                    });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                    .Returns(httpClient);
            var config = GetConfigurationValues();
            config.GetSection("ParameterSettings").GetSection("DefaultParameterSettingsApi").Value = string.Empty;
            var mockTokenAcquisition = new Mock<ITokenAcquisition>();
            var controller = new CalculationRunDetailsNewController(config, mockHttpClientFactory.Object,
                mockTokenAcquisition.Object, new TelemetryClient());

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            };

            var viewModel = new ParameterRefreshViewModel()
            {
                ParameterTemplateValues = MockData.GetSchemeParameterTemplateValues().ToList(),
                FileName = "Test Name",
            };

            var task = controller.Index(10);
            task.Wait();
            var result = task.Result as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunDetailsNewErrorPage, result.ViewName);
        }

        [TestMethod]
        public async Task Submit_InvalidModelState_ReturnsRedirectToAction()
        {
            // Arrange
            var model = new CalculatorRunDetailsNewViewModel()
            {
                CalculatorRunDetails = new CalculatorRunDetailsViewModel()
                {
                    RunId = 1,
                    RunName = "Test Run"
                },
                SelectedCalcRunOption = null
            };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(JsonConvert.SerializeObject(MockData.GetCalculatorRun())),
                    });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                    .Returns(httpClient);
            var config = GetConfigurationValues();
            var mockTokenAcquisition = new Mock<ITokenAcquisition>();
            var controller = new CalculationRunDetailsNewController(config, mockHttpClientFactory.Object,
                mockTokenAcquisition.Object, new TelemetryClient());

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = _mockHttpContext.Object
            };

            controller.ModelState.AddModelError("Error", "Model error");

            // Act
            var result = await controller.Submit(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunDetailsNewIndex, result.ViewName);
            var viewModel = result.Model as CalculatorRunDetailsNewViewModel;
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(model.CalculatorRunDetails.RunId, viewModel.CalculatorRunDetails.RunId);
        }

        [TestMethod]
        public async Task Submit_ValidModelState_OutputClassify_ReturnsRedirectToAction()
        {
            // Arrange
            var model = new CalculatorRunDetailsNewViewModel()
            {
                CalculatorRunDetails = new CalculatorRunDetailsViewModel()
                {
                    RunId = 1,
                    RunName = "Test Run"
                },
                SelectedCalcRunOption = CalculationRunOption.OutputClassify
            };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(JsonConvert.SerializeObject(MockData.GetCalculatorRun())),
                    });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                    .Returns(httpClient);
            var config = GetConfigurationValues();
            var mockTokenAcquisition = new Mock<ITokenAcquisition>();
            var controller = new CalculationRunDetailsNewController(config, mockHttpClientFactory.Object,
                mockTokenAcquisition.Object, new TelemetryClient());
            // Act
            var result = await _controller.Submit(model) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(ControllerNames.ClassifyingCalculationRun, result.ControllerName);
            Assert.AreEqual(1, result.RouteValues["runId"]);
        }

        [TestMethod]
        public async Task Submit_ValidModelState_OutputDelete_ReturnsRedirectToAction()
        {
            // Arrange
            var model = new CalculatorRunDetailsNewViewModel()
            {
                CalculatorRunDetails = new CalculatorRunDetailsViewModel()
                {
                    RunId = 1,
                    RunName = "Test Run"
                },
                SelectedCalcRunOption = CalculationRunOption.OutputDelete
            };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(JsonConvert.SerializeObject(MockData.GetCalculatorRun())),
                    });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                    .Returns(httpClient);
            var config = GetConfigurationValues();
            var mockTokenAcquisition = new Mock<ITokenAcquisition>();
            var controller = new CalculationRunDetailsNewController(config, mockHttpClientFactory.Object,
                mockTokenAcquisition.Object, new TelemetryClient());
            // Act
            var result = await _controller.Submit(model) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(ControllerNames.CalculationRunDelete, result.ControllerName);
            Assert.AreEqual(1, result.RouteValues["runId"]);
        }

        [TestMethod]
        public async Task Submit_ValidModelState_NoOutput_ReturnsRedirectToAction()
        {
            // Arrange
            var model = new CalculatorRunDetailsNewViewModel()
            {
                CalculatorRunDetails = new CalculatorRunDetailsViewModel()
                {
                    RunId = 1,
                    RunName = "Test Run"
                },
                SelectedCalcRunOption = null
            };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(JsonConvert.SerializeObject(MockData.GetCalculatorRun())),
                    });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                    .Returns(httpClient);
            var config = GetConfigurationValues();
            var mockTokenAcquisition = new Mock<ITokenAcquisition>();
            var controller = new CalculationRunDetailsNewController(config, mockHttpClientFactory.Object,
                mockTokenAcquisition.Object, new TelemetryClient());
            // Act
            var result = await _controller.Submit(model) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(1, result.RouteValues["runId"]);
        }

        private static IConfiguration GetConfigurationValues()
        {
            string projectPath = AppDomain.CurrentDomain.BaseDirectory.Split(Separator, StringSplitOptions.None)[0];
            IConfiguration config = new ConfigurationBuilder()
               .SetBasePath(projectPath)
               .AddJsonFile("appsettings.Test.json")
               .Build();

            return config;
        }
    }
}
