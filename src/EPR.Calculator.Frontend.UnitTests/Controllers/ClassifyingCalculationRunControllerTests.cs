using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace EPR.Calculator.Frontend.UnitTests.Controllers
{
    [TestClass]
    public class ClassifyingCalculationRunControllerTests
    {
        private readonly IConfiguration _configuration = ConfigurationItems.GetConfigurationValues();
        private Mock<ILogger<SetRunClassificationController>> _mockLogger;
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private TelemetryClient _mockTelemetryClient;
        private SetRunClassificationController _controller;
        private Mock<HttpContext> _mockHttpContext;
        private Mock<ICalculatorRunDetailsService> _mockCalculatorRunDetailsService;

        public ClassifyingCalculationRunControllerTests()
        {
            this.Fixture = new Fixture();
            this.MockMessageHandler = TestMockUtils.BuildMockMessageHandler();
            SetMessageHandlerResponses(true, HttpStatusCode.OK);
            _mockHttpClientFactory = TestMockUtils.BuildMockHttpClientFactory(this.MockMessageHandler.Object);
            _mockLogger = new Mock<ILogger<SetRunClassificationController>>();
            _mockTelemetryClient = new TelemetryClient();
            _mockCalculatorRunDetailsService = new Mock<ICalculatorRunDetailsService>();

            _controller = new SetRunClassificationController(
                       _configuration,
                       new Mock<IApiService>().Object,
                       _mockLogger.Object,
                       _mockTelemetryClient,
                       _mockCalculatorRunDetailsService.Object);

            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(context => context.User)
               .Returns(new ClaimsPrincipal(new ClaimsIdentity(
           [
               new Claim(ClaimTypes.Name, "Test User")
           ])));

            var mockSession = new MockHttpSession();
            mockSession.SetString("accessToken", "something");
            mockSession.SetString(SessionConstants.FinancialYear, "2024-25");
            var context = new DefaultHttpContext()
            {
                Session = mockSession
            };

            // Setting the mocked HttpContext for the controller
            _controller.ControllerContext = new ControllerContext { HttpContext = context };

            var details = Fixture.Create<CalculatorRunDetailsViewModel>();
            details.RunId = 1;
            details.RunClassificationId = RunClassification.UNCLASSIFIED;

            (_, _, _controller) = BuildTestClass(
                this.Fixture,
                HttpStatusCode.Created,
                Fixture.Create<FinancialYearClassificationResponseDto>(),
                details,
                _configuration);
        }

        private Fixture Fixture { get; init; }

        private Mock<IApiService> MockApiService { get; init; }

        private Mock<HttpMessageHandler> MockMessageHandler { get; set; }

        private CalculatorRunDetailsViewModel CalculatorRunDetails { get; init; }

        [TestMethod]
        public async Task Index_ReturnsViewResult_WithValidViewModel()
        {
            // Arrange
            int runId = 1;

            // Act
            var result = await _controller.Index(runId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.SetRunClassificationIndex, result.ViewName);
            var viewModel = result.Model as SetRunClassificationViewModel;
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(runId, viewModel.CalculatorRunDetails.RunId);
        }

        [TestMethod]
        public async Task Index_ShouldReturnSetRunClassificationView_WhenAllDependenciesSucceed()
        {
            // Arrange
            var runId = 1;
            var details = Fixture.Create<CalculatorRunDetailsViewModel>();
            details.RunId = runId;

            var classificationResponse = Fixture.Create<FinancialYearClassificationResponseDto>();

            SetMessageHandlerResponses(true, HttpStatusCode.OK); // Simulate success
            (_, _, _controller) = BuildTestClass(
                Fixture,
                HttpStatusCode.OK,
                classificationResponse,
                details,
                _configuration);

            // Act
            var result = await _controller.Index(runId);

            // Assert
            Assert.IsNotNull(result);
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(ViewNames.SetRunClassificationIndex, viewResult.ViewName);
            Assert.IsInstanceOfType(viewResult.Model, typeof(SetRunClassificationViewModel));
        }

        [TestMethod]
        public async Task Index_ShouldRedirectToError_WhenCalculatorRunDetailsIsInvalid()
        {
            // Arrange
            var runId = 1;

            var details = new CalculatorRunDetailsViewModel
            {
                RunId = 0 // Invalid
            };

            SetMessageHandlerResponses(true, HttpStatusCode.OK);

            (_, _, _controller) = BuildTestClass(
                Fixture,
                HttpStatusCode.OK,
                Fixture.Create<FinancialYearClassificationResponseDto>(),
                details,
                _configuration);

            // Act
            var result = await _controller.Index(runId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirect = result as RedirectToActionResult;
            Assert.AreEqual("Index", redirect.ActionName);
            Assert.AreEqual("StandardError", redirect.ControllerName);
        }

        [TestMethod]
        public async Task Index_ShouldRedirectToError_WhenSetClassificationsReturnsFalse()
        {
            // Arrange
            var runId = 0;

            var details = new CalculatorRunDetailsViewModel
            {
                RunId = 0,
                RunClassificationStatus = "None"
            };

            SetMessageHandlerResponses(false, HttpStatusCode.OK);

            (_, _, _controller) = BuildTestClass(
                Fixture,
                HttpStatusCode.OK,
                Fixture.Create<FinancialYearClassificationResponseDto>(),
                details,
                _configuration);

            // Act
            var result = await _controller.Index(runId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirect = result as RedirectToActionResult;
            Assert.AreEqual("Index", redirect.ActionName);
            Assert.AreEqual("StandardError", redirect.ControllerName);
        }

        [TestMethod]
        public async Task Submit_RedirectsToIndex_WhenModelStateIsInvalid()
        {
            // Arrange
            int runId = 1;
            SetRunClassificationViewModel model = new SetRunClassificationViewModel
            {
                CalculatorRunDetails = new CalculatorRunDetailsViewModel
                {
                    RunId = runId,
                    RunName = "Test Run"
                },
                ClassifyRunType = (int)RunClassification.INITIAL_RUN
            };

            _controller.ModelState.AddModelError("TestError", "Test error message");

            // Act
            var result = await _controller.Submit(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.SetRunClassificationIndex, result.ViewName);
            var viewModel = result.Model as SetRunClassificationViewModel;
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(runId, viewModel.CalculatorRunDetails.RunId);
        }

        [TestMethod]
        public async Task Submit_RedirectsToClassifyRunConfirmation_WhenSubmitSuccessful()
        {
            // Arrange
            int runId = 1;
            SetRunClassificationViewModel model = new SetRunClassificationViewModel
            {
                CalculatorRunDetails = new CalculatorRunDetailsViewModel
                {
                    RunId = runId,
                    RunName = "Test Run"
                },
                ClassifyRunType = (int)RunClassification.INITIAL_RUN
            };

            (var mockApiService, _, var controller) = BuildTestClass(
                this.Fixture,
                HttpStatusCode.Created,
                Fixture.Create<FinancialYearClassificationResponseDto>(),
                null,
                _configuration);

            // Act
            var result = await controller.Submit(model) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(ControllerNames.ClassifyRunConfirmation, result.ControllerName);
            Assert.AreEqual(runId, result.RouteValues["runId"]);
            mockApiService.Verify(
                MockApiService => MockApiService.CallApi(
                    controller.HttpContext,
                    HttpMethod.Put,
                    It.IsAny<Uri>(),
                    It.IsAny<string>(),
                    It.Is<ClassificationDto>(dto => dto.RunId == runId)),
                Times.Once);
        }

        [TestMethod]
        public async Task Submit_InvalidModel_ReturnsViewResult_WithErrors()
        {
            // Arrange
            var model = new SetRunClassificationViewModel { CalculatorRunDetails = new CalculatorRunDetailsViewModel { RunId = 1, RunName = "Test Run" } };
            _controller.ModelState.AddModelError("ClassifyRunType", "Required");

            // Act
            var result = await _controller.Submit(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(SetRunClassificationViewModel));
            Assert.IsTrue(_controller.ModelState.ErrorCount > 0);
        }

        [TestMethod]
        public async Task Submit_ValidModel_RedirectsToConfirmation()
        {
            // Arrange
            SetMessageHandlerResponses(false, HttpStatusCode.Created);
            var model = new SetRunClassificationViewModel
            {
                CalculatorRunDetails = new CalculatorRunDetailsViewModel
                {
                    RunId = 1,
                    RunName = "Test Run"
                },
                ClassifyRunType = (int)RunClassification.INITIAL_RUN
            };

            // Act
            var result = await _controller.Submit(model) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(ControllerNames.ClassifyRunConfirmation, result.ControllerName);
        }

        [TestMethod]
        public async Task Index_ReturnsStandardError_WhenRunIdIsZero()
        {
            var details = Fixture.Create<CalculatorRunDetailsViewModel>();
            details.RunId = 0;

            (_, _, _controller) = BuildTestClass(
                this.Fixture,
                HttpStatusCode.Created,
                Fixture.Create<FinancialYearClassificationResponseDto>(),
                details,
                _configuration);
            // Arrange
            int runId = 1;

            // Act
            var result = await _controller.Index(runId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(CommonUtil.GetControllerName(typeof(StandardErrorController)), result.ControllerName);
        }

        [TestMethod]
        public async Task Index_ReturnsStandardError_WhenRunIdIsZero_AndResponseDtoIsNull()
        {
            // Arrange
            int runId = 1;

            // Simulate API returning JSON "null" (which deserializes to null)
            var incorrecteHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null", Encoding.UTF8, "application/json")
            };

            // Create a CalculatorRunDetailsViewModel with RunId = 0 to simulate the failure condition
            var runDetails = Fixture.Create<CalculatorRunDetailsViewModel>();
            runDetails.RunId = 0;

            // Setup the controller with the mocked response and run details
            (_, _, _controller) = BuildTestClass(
               this.Fixture,
               HttpStatusCode.OK,
               null,
               runDetails,
               _configuration);

            // Act
            var result = await _controller.Index(runId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual(CommonUtil.GetControllerName(typeof(StandardErrorController)), result.ControllerName);
        }

        [TestMethod]

        [DataRow(RunClassification.INITIAL_RUN, CommonConstants.InitialRunDescription)]
        [DataRow(RunClassification.TEST_RUN, CommonConstants.TestRunDescription)]
        [DataRow(RunClassification.INTERIM_RECALCULATION_RUN, CommonConstants.InterimRunDescription)]
        [DataRow(RunClassification.FINAL_RECALCULATION_RUN, CommonConstants.FinalRecalculationRunDescription)]
        [DataRow(RunClassification.FINAL_RUN, CommonConstants.FinalRecalculationRunDescription)]
        public async Task Index_CheckClassificationStatusDescription_IsValid(RunClassification runClassification, string description)
        {
            var responseContent = "{\r\n  \"runId\": 1,\r\n  \"runClassificationId\": 7,\r\n  \"runName\": \"Test Calculator1702\",\r\n  \"runClassificationStatus\": \"UNCLASSIFIED\",\r\n  \"financialYear\": \"2025-26\"\r\n}";
            var classificationResponseContent = "{\r\n\"financialYear\": \"2025-26\",\r\n  \"classifications\": [\r\n    {\r\n      \"id\":" + (int)runClassification + ",\r\n\"status\": \"INITIAL RUN\"\r\n    }\r\n  ]\r\n}";

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(responseContent)
                    });

            mockHttpMessageHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.Is<HttpRequestMessage>(k => k.RequestUri != null && k.RequestUri.ToString().Contains("Financial")),
                   ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                   {
                       StatusCode = HttpStatusCode.OK,
                       Content = new StringContent(classificationResponseContent),
                   });

            // Arrange
            int runId = 1;

            var details = Fixture.Create<CalculatorRunDetailsViewModel>();
            details.RunId = runId;

            var financialYearDto = Fixture.Create<FinancialYearClassificationResponseDto>();

            financialYearDto.Classifications = new List<CalculatorRunClassificationDto>
            {
                new CalculatorRunClassificationDto
                {
                    Id = (int)runClassification,
                    Description = description,
                    Status = runClassification.ToString()
                }
            };

            (_, _, _controller) = BuildTestClass(
                this.Fixture,
                HttpStatusCode.Created,
                financialYearDto,
                details,
                _configuration);

            // Act
            var result = await _controller.Index(runId) as ViewResult;
            var model = result.Model as SetRunClassificationViewModel;
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(SetRunClassificationViewModel));
            Assert.AreEqual(description, model.FinancialYearClassifications.Classifications.First().Description);
        }

        [TestMethod]
        [DataRow(RunClassification.INITIAL_RUN, CommonConstants.InitialRunDescription)]
        [DataRow(RunClassification.FINAL_RECALCULATION_RUN, CommonConstants.FinalRecalculationRunDescription)]
        [DataRow(RunClassification.FINAL_RUN, CommonConstants.FinalRecalculationRunDescription)]
        public async Task Index_ValidModel_ClassificationStatusInformation_IsValid(RunClassification runClassification, string description)
        {
            // Arrange
            int runId = 1;

            var details = Fixture.Create<CalculatorRunDetailsViewModel>();
            details.RunId = runId;

            var financialYearDto = Fixture.Create<FinancialYearClassificationResponseDto>();

            financialYearDto.Classifications = new List<CalculatorRunClassificationDto>
            {
                new CalculatorRunClassificationDto
                {
                    Id = (int)runClassification,
                    Description = description,
                    Status = runClassification.ToString()
                }
            };

            (_, _, _controller) = BuildTestClass(
                this.Fixture,
                HttpStatusCode.Created,
                financialYearDto,
                details,
                _configuration);

            // Act
            var result = await _controller.Index(runId) as ViewResult;
            var model = result.Model as SetRunClassificationViewModel;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(SetRunClassificationViewModel));

            if(runClassification == RunClassification.INITIAL_RUN)
            {
               Assert.IsFalse(model.ClassificationStatusInformation.ShowInitialRunDescription);
               Assert.IsTrue(model.ClassificationStatusInformation.ShowInterimRecalculationRunDescription);
               Assert.IsTrue(model.ClassificationStatusInformation.ShowFinalRecalculationRunDescription);
               Assert.IsTrue(model.ClassificationStatusInformation.ShowFinalRunDescription);
            }

            if (runClassification == RunClassification.FINAL_RECALCULATION_RUN)
            {
                Assert.IsTrue(model.ClassificationStatusInformation.ShowInitialRunDescription);
                Assert.IsFalse(model.ClassificationStatusInformation.ShowFinalRecalculationRunDescription);
            }

            if (runClassification == RunClassification.FINAL_RUN)
            {
                Assert.IsTrue(model.ClassificationStatusInformation.ShowInitialRunDescription);
                Assert.IsFalse(model.ClassificationStatusInformation.ShowFinalRunDescription);
            }
        }

        private void SetMessageHandlerResponses(bool isUnclassified, HttpStatusCode httpStatusCode)
        {
            var responseContent = isUnclassified ? "{\r\n  \"runId\": 1,\r\n  \"runClassificationId\": 3,\r\n  \"runClassificationStatus\": \"UNCLASSIFIED\",\r\n  \"financialYear\": \"2025-26\"\r\n}"
                : "{\r\n  \"runId\": 1,\r\n  \"runClassificationId\": 7,\r\n  \"runName\": \"Test Calculator1702\",\r\n  \"runClassificationStatus\": \"UNCLASSIFIED\",\r\n  \"financialYear\": \"2025-26\"\r\n}";
            this.MockMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = httpStatusCode,
                        Content = new StringContent(responseContent)
                    });

            this.MockMessageHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                   "SendAsync",
                   ItExpr.Is<HttpRequestMessage>(k => k.RequestUri != null && k.RequestUri.ToString().Contains("Financial")),
                   ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                   {
                       StatusCode = httpStatusCode,
                       Content = new StringContent("{\r\n\"financialYear\": \"2025-26\",\r\n  \"classifications\": [\r\n    {\r\n      \"id\": 4,\r\n      \"status\": \"TEST RUN\"\r\n    },\r\n    {\r\n      \"id\": 8,\r\n      \"status\": \"INITIAL RUN\"\r\n    }\r\n  ]\r\n}"),
                   });
        }

        private (
            Mock<IApiService> MockApiService,
            Mock<ILogger<SetRunClassificationController>> MockLogger,
            SetRunClassificationController TestClass) BuildTestClass(
                Fixture fixture,
                HttpStatusCode httpStatusCode,
                object callApiResponse,
                CalculatorRunDetailsViewModel details = null,
                IConfiguration configurationItems = null)
        {
            configurationItems = configurationItems ?? ConfigurationItems.GetConfigurationValues();
            details = details ?? Fixture.Create<CalculatorRunDetailsViewModel>();
            var mockApiService = TestMockUtils.BuildMockApiService(
                httpStatusCode,
                JsonSerializer.Serialize(callApiResponse));

            var mockLogger = new Mock<ILogger<SetRunClassificationController>>();

            var testClass = new SetRunClassificationController(
                configurationItems,
                mockApiService.Object,
                mockLogger.Object,
                new TelemetryClient(),
                TestMockUtils.BuildMockCalculatorRunDetailsService(details).Object);
            testClass.ControllerContext.HttpContext = new DefaultHttpContext()
            {
                Session = TestMockUtils.BuildMockSession(fixture).Object,
            };

            return (mockApiService, mockLogger, testClass);
        }
    }
}