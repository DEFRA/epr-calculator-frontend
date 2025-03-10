using System.Configuration;
using System.Net;
using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
using EPR.Calculator.Frontend.Validators;
using EPR.Calculator.Frontend.ViewModels;
using FluentValidation.TestHelper;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Moq;
using Moq.Protected;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class CalculationRunNameControllerTests
    {
        private readonly IConfiguration configuration = ConfigurationItems.GetConfigurationValues();

        private CalculationRunNameController _controller;
        private CalculatorRunNameValidator _validationRules;
        private Mock<IHttpClientFactory> mockClientFactory;
        private Mock<IConfiguration> mockConfiguration;
        private Mock<ILogger<CalculationRunNameController>> mockLogger;
        private Mock<ITempDataDictionary> _tempDataMock;
        private Mock<ITokenAcquisition> mockTokenAcquisition;

        private Fixture Fixture { get; } = new Fixture();

        [TestInitialize]
        public void Setup()
        {
            mockClientFactory = new Mock<IHttpClientFactory>();
            mockLogger = new Mock<ILogger<CalculationRunNameController>>();
            mockTokenAcquisition = new Mock<ITokenAcquisition>();
            mockTokenAcquisition
                .Setup(x => x.GetAccessTokenForUserAsync(It.IsAny<IEnumerable<string>>(), null, null, null, null))
                .ReturnsAsync("somevalue");
            _controller = new CalculationRunNameController(configuration, mockClientFactory.Object, mockLogger.Object,
                mockTokenAcquisition.Object, new TelemetryClient());
            _validationRules = new CalculatorRunNameValidator();
            _tempDataMock = new Mock<ITempDataDictionary>();

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            _controller.TempData = _tempDataMock.Object;
        }

        [TestMethod]
        public void RunCalculator_CalculationRunNameController_View_Test()
        {
            var result = _controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
        }

        [TestMethod]
        public async Task RunCalculator_ShouldReturnView_WhenCalculationNameIsInvalid()
        {
            _controller.ModelState.AddModelError("CalculationName", "Enter a name for this calculation");
            var calculatorRunModel = new InitiateCalculatorRunModel()
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = null,
            };
            var result = await _controller.RunCalculator(null);
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, viewResult.ViewName);
            var errorViewModel = _controller.ViewBag.Errors as ErrorViewModel;
            Assert.IsNotNull(errorViewModel);
            Assert.AreEqual(ViewControlNames.CalculationRunName, errorViewModel.DOMElementId);
            Assert.AreEqual(ErrorMessages.CalculationRunNameEmpty, errorViewModel.ErrorMessage);
        }

        [TestMethod]
        public async Task RunCalculator_ShouldRedirect_IsOnlyAlphabets_WhenCalculationNameIsValid()
        {
            var model = new InitiateCalculatorRunModel
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = "ValidCalculationName",
            };
            MockHttpClientWithResponse();
            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            mockHttpContext.Setup(s => s.Session).Returns(mockSession.Object);
            mockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            var result = await _controller.RunCalculator(model) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
            Assert.AreNotEqual(ActionNames.RunCalculatorConfirmation, result.ViewName);
        }

        [TestMethod]
        public async Task RunCalculator_ShouldRedirect_IsOnlyNumeric_WhenCalculationNameIsValid()
        {
            var calculatorRunModel = new InitiateCalculatorRunModel()
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = "1234",
            };
            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            mockHttpContext.Setup(s => s.Session).Returns(mockSession.Object);
            mockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };
            MockHttpClientWithResponse();
            var result = await _controller.RunCalculator(calculatorRunModel) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
            Assert.AreNotEqual(ActionNames.RunCalculatorConfirmation, result.ViewName);
        }

        [TestMethod]
        public async Task RunCalculator_ShouldRedirect_IsAplhaNumeric_WithNoSpace_WhenCalculationNameIsValid()
        {
            var calculatorRunModel = new InitiateCalculatorRunModel()
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = "ValidCalculationName1234",
            };
            MockHttpClientWithResponse();

            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            mockHttpContext.Setup(s => s.Session).Returns(mockSession.Object);
            mockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            var result = await _controller.RunCalculator(calculatorRunModel) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
            Assert.AreNotEqual(ActionNames.RunCalculatorConfirmation, result.ViewName);
        }

        [TestMethod]
        public async Task RunCalculator_WhenCalculationName__IsAplhaNumeric_WithSpace_IsValid()
        {
            var calculatorRunModel = new InitiateCalculatorRunModel()
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = "ValidCalculationName 123",
            };
            MockHttpClientWithResponse();

            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            mockHttpContext.Setup(s => s.Session).Returns(mockSession.Object);
            mockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            var result = await _controller.RunCalculator(calculatorRunModel) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
            Assert.AreNotEqual(ActionNames.RunCalculatorConfirmation, result.ViewName);
        }

        [TestMethod]
        public async Task RunCalculator_WhenCalculationNameIsEmpty_ShouldReturnViewWithError()
        {
            var calculatorRunModel = new InitiateCalculatorRunModel()
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = string.Empty,
            };
            _controller.ModelState.AddModelError("CalculationName", "Enter a name for this calculation");
            var result = await _controller.RunCalculator(calculatorRunModel) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
            Assert.IsTrue(_controller.ViewBag.Errors is ErrorViewModel);
            var errorViewModel = _controller.ViewBag.Errors as ErrorViewModel;
            Assert.AreEqual(ErrorMessages.CalculationRunNameEmpty, errorViewModel.ErrorMessage);
        }

        [TestMethod]
        public async Task RunCalculator_WhenCalculationNameIsTooLong_ShouldReturnViewWithError()
        {
            _controller.ModelState.AddModelError("CalculationName", "Calculation name must contain no more than 100 characters");
            var calculatorRunModel = new InitiateCalculatorRunModel()
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = new string('a', 101)
            };
            var result = await _controller.RunCalculator(calculatorRunModel) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
            Assert.IsTrue(_controller.ViewBag.Errors is ErrorViewModel);
            var errorViewModel = _controller.ViewBag.Errors as ErrorViewModel;
            Assert.AreEqual(ErrorMessages.CalculationRunNameMaxLengthExceeded, errorViewModel.ErrorMessage);
        }

        [TestMethod]
        public async Task RunCalculator_WhenCalculationName_IsNotAlphaNumeric_ShouldReturnViewWithError()
        {
            _controller.ModelState.AddModelError("CalculationName", "Calculation name must only contain numbers and letters");
            var calculatorRunModel = new InitiateCalculatorRunModel()
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = "%^&*$@"
            };
            var result = await _controller.RunCalculator(calculatorRunModel) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
            Assert.IsTrue(_controller.ViewBag.Errors is ErrorViewModel);
            var errorViewModel = _controller.ViewBag.Errors as ErrorViewModel;
            Assert.AreEqual(ErrorMessages.CalculationRunNameMustBeAlphaNumeric, errorViewModel.ErrorMessage);
        }

        [TestMethod]
        public void RunCalculator_Validator_Should_Have_Error_Is_Empty()
        {
            var model = new InitiateCalculatorRunModel
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = string.Empty,
            };
            var result = _validationRules.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CalculationName)
                .WithErrorMessage(ErrorMessages.CalculationRunNameEmpty);
        }

        [TestMethod]
        public void RunCalculator_Validator_Should_Have_Error_Exceeds_MaxLength()
        {
            var model = new InitiateCalculatorRunModel
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = new string('a', 101),
            };
            var result = _validationRules.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CalculationName)
                .WithErrorMessage(ErrorMessages.CalculationRunNameMaxLengthExceeded);
        }

        [TestMethod]
        public void RunCalculator_Validator_Should_Have_Error_When_Is_Not_AlphaNumeric()
        {
            var model = new InitiateCalculatorRunModel
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = "test_123",
            };
            var result = _validationRules.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CalculationName)
                .WithErrorMessage(ErrorMessages.CalculationRunNameMustBeAlphaNumeric);
        }

        [TestMethod]
        public void RunCalculator_Validator_Should_Not_Have_Error_Is_Valid()
        {
            var model = new InitiateCalculatorRunModel
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = "test123",
            };
            var result = _validationRules.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.CalculationName);
        }

        [TestMethod]
        public void RunCalculator_Validator_Should_Not_Have_Error_Has_Spaces()
        {
            var model = new InitiateCalculatorRunModel
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = "test 123",
            };
            var result = _validationRules.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.CalculationName);
        }

        [TestMethod]
        public async Task RunCalculator_ValidModel_CalculationNameExists_ShouldReturnToIndexWithError()
        {
            // Arrange
            var model = new InitiateCalculatorRunModel
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = "TestCalculation",
            };
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            mockHttpContext.Setup(s => s.Session).Returns(mockSession.Object);
            mockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);
            mockClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(mockHttpClient);
            var result = await _controller.RunCalculator(model) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
            Assert.IsNotNull(_controller.ViewBag.Errors);
            Assert.AreEqual(ErrorMessages.CalculationRunNameExists, ((ErrorViewModel)_controller.ViewBag.Errors).ErrorMessage);
        }

        [TestMethod]
        public async Task RunCalculator_ValidModel_CalculationNameDoesNotExist_ShouldRedirectToConfirmation()
        {
            var model = new InitiateCalculatorRunModel
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = "UniqueCalculation",
            };

            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            mockHttpContext.Setup(s => s.Session).Returns(mockSession.Object);
            mockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            // Mock the first API call to return NotFound
            var mockHttpMessageHandler1 = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler1
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            var mockHttpClient1 = new HttpClient(mockHttpMessageHandler1.Object);
            mockClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(mockHttpClient1);

            // Mock the second API call to return Accepted
            var mockHttpMessageHandler2 = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler2
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Accepted));

            var mockHttpClient2 = new HttpClient(mockHttpMessageHandler2.Object);
            mockClientFactory.SetupSequence(x => x.CreateClient(It.IsAny<string>()))
                             .Returns(mockHttpClient1)
                             .Returns(mockHttpClient2);

            var result = await _controller.RunCalculator(model) as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.RunCalculatorConfirmation, result.ActionName);
        }

        [TestMethod]
        public async Task CheckIfCalculationNameExistsAsync_ApiUrlIsNull_ShouldThrowArgumentNullException()
        {
            var mockApiSection = new Mock<IConfigurationSection>();
            mockApiSection.Setup(s => s.Value).Returns(string.Empty);

            var mockSettingsSection = new Mock<IConfigurationSection>();
            mockSettingsSection
                .Setup(s => s.GetSection(ConfigSection.CalculationRunNameApi))
                .Returns(mockApiSection.Object);

            mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration
                .Setup(c => c.GetSection(ConfigSection.CalculationRunSettings))
                .Returns(mockSettingsSection.Object);

            var model = new InitiateCalculatorRunModel
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = "TestCalculation",
            };
            _controller = new CalculationRunNameController(mockConfiguration.Object, mockClientFactory.Object,
                mockLogger.Object, mockTokenAcquisition.Object, new TelemetryClient());
            var redirectResult = await _controller.RunCalculator(model) as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(ActionNames.StandardErrorIndex, redirectResult.ActionName);
            Assert.AreEqual("StandardError", redirectResult.ControllerName);
        }

        [TestMethod]
        public async Task RunCalculatorConfirmation_ValidModel_RedirectsToConfirmation()
        {
            var model = new InitiateCalculatorRunModel
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = "TestRun",
            };
            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            mockHttpContext.Setup(s => s.Session).Returns(mockSession.Object);
            mockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            // Mock the first API call to return NotFound
            var mockHttpMessageHandler1 = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler1
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            var mockHttpClient1 = new HttpClient(mockHttpMessageHandler1.Object);
            mockClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(mockHttpClient1);

            // Mock the second API call to return Accepted
            var mockHttpMessageHandler2 = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler2
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Accepted));

            var mockHttpClient2 = new HttpClient(mockHttpMessageHandler2.Object);
            mockClientFactory.SetupSequence(x => x.CreateClient(It.IsAny<string>()))
                             .Returns(mockHttpClient1)
                             .Returns(mockHttpClient2);

            var result = await _controller.RunCalculator(model);
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(ActionNames.RunCalculatorConfirmation, redirectResult.ActionName);
        }

        [TestMethod]
        public async Task RunCalculator_HttpPostToCalculatorRunAPI_Failure_RedirectsToStandardError()
        {
            var model = new InitiateCalculatorRunModel
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = "TestName",
            };
            var mockHttpContext = new Mock<HttpContext>();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            // Mock the first API call to return NotFound
            var mockHttpMessageHandler1 = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler1
                .Protected()
                        .Setup<Task<HttpResponseMessage>>(
                            "SendAsync",
                            ItExpr.IsAny<HttpRequestMessage>(),
                            ItExpr.IsAny<CancellationToken>())
                        .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));
            var mockHttpClient1 = new HttpClient(mockHttpMessageHandler1.Object);
            mockClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(mockHttpClient1);

            // Mock the second API call to return Accepted
            var mockHttpMessageHandler2 = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler2
                .Protected()
                        .Setup<Task<HttpResponseMessage>>(
                            "SendAsync",
                            ItExpr.IsAny<HttpRequestMessage>(),
                            ItExpr.IsAny<CancellationToken>())
                        .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

            var mockHttpClient2 = new HttpClient(mockHttpMessageHandler2.Object);
            mockClientFactory.SetupSequence(x => x.CreateClient(It.IsAny<string>()))
                                     .Returns(mockHttpClient1)
                                     .Returns(mockHttpClient2);

            // Act
            var result = await _controller.RunCalculator(model) as RedirectToActionResult;

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = result as RedirectToActionResult;
            Assert.AreEqual(ActionNames.StandardErrorIndex, redirectResult.ActionName);
            Assert.AreEqual("StandardError", redirectResult.ControllerName);
        }

        [TestMethod]
        public async Task RunCalculator_ValidModel_ApiCallFails_RedirectsToStandardError()
        {
            var model = new InitiateCalculatorRunModel
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = "TestRun",
            };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

            var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);
            mockClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(mockHttpClient);

            var result = await _controller.RunCalculator(model) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
        }

        [TestMethod]
        public void Confirmation_ReturnsViewResult()
        {
            var result = _controller.Confirmation() as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunConfirmation, result.ViewName);
        }

        [TestMethod]
        public async Task RunCalculatorConfirmation_NullExceptionForAPIConfig_RedirectsToErrorPage()
        {
            var model = new InitiateCalculatorRunModel
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = "TestRun",
            };
            mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(config => config[$"{ConfigSection.CalculationRunSettings}:{ConfigSection.CalculationRunApi}"])
                             .Returns((string)null);

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

            var mockHttpContext = new Mock<HttpContext>();
            var mockTokenAcquisition = new Mock<ITokenAcquisition>();
            var controller = new CalculationRunNameController(mockConfiguration.Object, mockClientFactory.Object,
                mockLogger.Object, mockTokenAcquisition.Object, new TelemetryClient())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };

            var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);
            mockClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(mockHttpClient);
            var result = await controller.RunCalculator(model);

            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(ActionNames.StandardErrorIndex, redirectResult.ActionName);
            Assert.AreEqual("StandardError", redirectResult.ControllerName);
        }

        [TestMethod]
        public async Task RunCalculator_ShouldRedirectToError_WhenUnprocessableEntity()
        {
            var calculationRunModel = new InitiateCalculatorRunModel
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = "TestRun"
            };
            var errorMessage = "The calculator is currently running. You will be able to run another calculation once the current one has finished.";
            var httpResponse = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
            {
                Content = new StringContent($"{{ \"message\": \"{errorMessage}\" }}")
            };

            var mockHttpMessageNotFoundHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageNotFoundHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            var mockHttpClient1 = new HttpClient(mockHttpMessageNotFoundHandler.Object);
            mockClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(mockHttpClient1);

            var mockHttpMessageUnprocessableEntityHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageUnprocessableEntityHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            var mockHttpClient2 = new HttpClient(mockHttpMessageUnprocessableEntityHandler.Object);
            mockClientFactory.SetupSequence(x => x.CreateClient(It.IsAny<string>()))
                             .Returns(mockHttpClient1)
                             .Returns(mockHttpClient2);

            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            mockHttpContext.Setup(s => s.Session).Returns(mockSession.Object);
            mockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            var result = await _controller.RunCalculator(calculationRunModel);

            var redirectResult = result as ViewResult;
            Assert.AreEqual("~/Views/CalculationRunError/Index.cshtml", redirectResult.ViewName);
        }

        [TestMethod]
        public async Task RunCalculator_ShouldRedirectToError_WhenUnprocessableEntity_ParsingError()
        {
            var calculationRunModel = new InitiateCalculatorRunModel
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = "TestRun"
            };
            var errorMessage = "Unable to process the error response.";
            var httpResponse = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
            {
                Content = new StringContent($"parsing error")
            };

            var mockHttpMessageNotFoundHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageNotFoundHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            var mockHttpClient1 = new HttpClient(mockHttpMessageNotFoundHandler.Object);
            mockClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(mockHttpClient1);

            var mockHttpMessageUnprocessableEntityHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageUnprocessableEntityHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            var mockHttpClient2 = new HttpClient(mockHttpMessageUnprocessableEntityHandler.Object);
            mockClientFactory.SetupSequence(x => x.CreateClient(It.IsAny<string>()))
                             .Returns(mockHttpClient1)
                             .Returns(mockHttpClient2);

            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            mockHttpContext.Setup(s => s.Session).Returns(mockSession.Object);
            mockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            var result = await _controller.RunCalculator(calculationRunModel);

            var redirectResult = result as ViewResult;
            Assert.AreEqual("~/Views/CalculationRunError/Index.cshtml", redirectResult.ViewName);
        }

        [TestMethod]
        public void GetCalculatorRunParametersTest()
        {
            var blankConfig = ConfigurationItems.GetConfigurationValuesWithEmptyStrings();
            var controller = new CalculationRunNameController(blankConfig, mockClientFactory.Object, mockLogger.Object,
                mockTokenAcquisition.Object, new TelemetryClient());
            Assert.ThrowsException<ConfigurationErrorsException>(() => controller.GetCalculatorRunParameters());
        }

        private void MockHttpClientWithResponse()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Accepted));

            var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);
            mockClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(mockHttpClient);
        }
    }
}