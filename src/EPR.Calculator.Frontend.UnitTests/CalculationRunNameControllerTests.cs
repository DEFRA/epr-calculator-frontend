using System.Net;
using System.Text;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using EPR.Calculator.Frontend.Validators;
using EPR.Calculator.Frontend.ViewModels;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private MockHttpSession mockHttpSession;
        private Mock<IConfiguration> mockConfiguration;
        private Mock<ILogger<CalculationRunNameController>> mockLogger;

        [TestInitialize]
        public void Setup()
        {
            mockClientFactory = new Mock<IHttpClientFactory>();
            mockHttpSession = new MockHttpSession();
            mockLogger = new Mock<ILogger<CalculationRunNameController>>();
            _controller = new CalculationRunNameController(configuration, mockClientFactory.Object, mockLogger.Object);
            _validationRules = new CalculatorRunNameValidator();

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            _controller.ControllerContext.HttpContext = new DefaultHttpContext();
            _controller.ControllerContext.HttpContext.Session = mockHttpSession;
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
            var calculatorRunModel = new InitiateCalculatorRunModel() { CalculationName = null };
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
                CalculationName = "ValidCalculationName"
            };
            MockHttpClientWithResponse();
            var result = await _controller.RunCalculator(model) as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.RunCalculatorConfirmation, result.ActionName);
            Assert.AreEqual("ValidCalculationName", mockHttpSession.GetString(SessionConstants.CalculationName));
        }

        [TestMethod]
        public async Task RunCalculator_ShouldRedirect_IsOnlyNumeric_WhenCalculationNameIsValid()
        {
            var calculatorRunModel = new InitiateCalculatorRunModel() { CalculationName = "1234" };
            MockHttpClientWithResponse();
            var result = await _controller.RunCalculator(calculatorRunModel) as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.RunCalculatorConfirmation, result.ActionName);
            Assert.AreEqual("1234", mockHttpSession.GetString(SessionConstants.CalculationName));
        }

        [TestMethod]
        public async Task RunCalculator_ShouldRedirect_IsAplhaNumeric_WithNoSpace_WhenCalculationNameIsValid()
        {
            var calculatorRunModel = new InitiateCalculatorRunModel() { CalculationName = "ValidCalculationName1234" };
            MockHttpClientWithResponse();
            var result = await _controller.RunCalculator(calculatorRunModel) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.RunCalculatorConfirmation, result.ActionName);
            Assert.AreEqual("ValidCalculationName1234", mockHttpSession.GetString(SessionConstants.CalculationName));
        }

        [TestMethod]
        public async Task RunCalculator_WhenCalculationName__IsAplhaNumeric_WithSpace_IsValid()
        {
            var calculatorRunModel = new InitiateCalculatorRunModel() { CalculationName = "ValidCalculationName 123" };
            MockHttpClientWithResponse();
            var result = await _controller.RunCalculator(calculatorRunModel) as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.RunCalculatorConfirmation, result.ActionName);
            Assert.AreEqual("ValidCalculationName 123", mockHttpSession.GetString(SessionConstants.CalculationName));
        }

        [TestMethod]
        public async Task RunCalculator_WhenCalculationNameIsProvided_ShouldSetSessionAndRedirect()
        {
            var calculatorRunModel = new InitiateCalculatorRunModel() { CalculationName = "TestCalculation" };
            byte[] calculationNameBytes = Encoding.UTF8.GetBytes(calculatorRunModel.CalculationName);
            MockHttpClientWithResponse();
            var result = await _controller.RunCalculator(calculatorRunModel) as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.RunCalculatorConfirmation, result.ActionName);
            Assert.AreEqual("TestCalculation", mockHttpSession.GetString(SessionConstants.CalculationName));
        }

        [TestMethod]
        public async Task RunCalculator_WhenCalculationNameIsEmpty_ShouldReturnViewWithError()
        {
            var calculatorRunModel = new InitiateCalculatorRunModel() { CalculationName = string.Empty };
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
            var calculatorRunModel = new InitiateCalculatorRunModel() { CalculationName = new string('a', 101) };
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
            var calculatorRunModel = new InitiateCalculatorRunModel() { CalculationName = "%^&*$@" };
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
            var model = new InitiateCalculatorRunModel { CalculationName = string.Empty };
            var result = _validationRules.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CalculationName)
                .WithErrorMessage(ErrorMessages.CalculationRunNameEmpty);
        }

        [TestMethod]
        public void RunCalculator_Validator_Should_Have_Error_Exceeds_MaxLength()
        {
            var model = new InitiateCalculatorRunModel { CalculationName = new string('a', 101) };
            var result = _validationRules.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CalculationName)
                .WithErrorMessage(ErrorMessages.CalculationRunNameMaxLengthExceeded);
        }

        [TestMethod]
        public void RunCalculator_Validator_Should_Have_Error_When_Is_Not_AlphaNumeric()
        {
            var model = new InitiateCalculatorRunModel { CalculationName = "test_123" };
            var result = _validationRules.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CalculationName)
                .WithErrorMessage(ErrorMessages.CalculationRunNameMustBeAlphaNumeric);
        }

        [TestMethod]
        public void RunCalculator_Validator_Should_Not_Have_Error_Is_Valid()
        {
            var model = new InitiateCalculatorRunModel { CalculationName = "test123" };
            var result = _validationRules.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.CalculationName);
        }

        [TestMethod]
        public void RunCalculator_Validator_Should_Not_Have_Error_Has_Spaces()
        {
            var model = new InitiateCalculatorRunModel { CalculationName = "test 123" };
            var result = _validationRules.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.CalculationName);
        }

        [TestMethod]
        public async Task RunCalculator_ValidModel_CalculationNameExists_ShouldReturnToIndexWithError()
        {
            // Arrange
            var model = new InitiateCalculatorRunModel { CalculationName = "TestCalculation" };
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);
            mockClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(mockHttpClient);
            var result = await _controller.RunCalculator(model);
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, viewResult.ViewName);
            Assert.IsNotNull(_controller.ViewBag.Errors);
            Assert.AreEqual(ErrorMessages.CalculationRunNameExists, ((ErrorViewModel)_controller.ViewBag.Errors).ErrorMessage);
        }

        [TestMethod]
        public async Task RunCalculator_ValidModel_CalculationNameDoesNotExist_ShouldRedirectToConfirmation()
        {
            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            mockHttpContext.Setup(s => s.Session).Returns(mockSession.Object);
            var model = new InitiateCalculatorRunModel { CalculationName = "UniqueCalculation" };
            byte[] calculationNameBytes = Encoding.UTF8.GetBytes(model.CalculationName);
            MockHttpClientWithResponse();
            var result = await _controller.RunCalculator(model) as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.RunCalculatorConfirmation, result.ActionName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
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

            var model = new InitiateCalculatorRunModel { CalculationName = "TestCalculation" };
            _controller = new CalculationRunNameController(mockConfiguration.Object, mockClientFactory.Object, mockLogger.Object);
            await _controller.RunCalculator(model);
        }

        [TestMethod]
        public async Task RunCalculatorConfirmation_ValidModel_RedirectsToConfirmation()
        {
            var model = new InitiateCalculatorRunModel { CalculationName = "TestRun" };
            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            mockHttpContext.Setup(s => s.Session).Returns(mockSession.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);
            mockClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(mockHttpClient);

            var result = await _controller.RunCalculator(model);

            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(ActionNames.RunCalculatorConfirmation, redirectResult.ActionName);
        }

        [TestMethod]
        public async Task RunCalculatorConfirmation_SuccessfulResponse_ReturnsConfirmationView()
        {
            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            byte[] value = System.Text.Encoding.UTF8.GetBytes("TestRun");
            mockSession.Setup(s => s.TryGetValue(SessionConstants.CalculationName, out value)).Returns(true);
            mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

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

            var result = await _controller.Confirmation();

            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(ViewNames.CalculationRunConfirmation, viewResult.ViewName);
        }

        [TestMethod]
        public async Task RunCalculatorConfirmation_ErrorResponse_RedirectsToErrorPage()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            mockClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);
            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            byte[] value = System.Text.Encoding.UTF8.GetBytes("TestRun");
            mockSession.Setup(s => s.TryGetValue(SessionConstants.CalculationName, out value)).Returns(true);
            mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);

            var result = await _controller.Confirmation();

            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(ActionNames.StandardErrorIndex, redirectResult.ActionName);
            Assert.AreEqual("StandardError", redirectResult.ControllerName);
        }

        [TestMethod]
        public async Task RunCalculatorConfirmation_NullExceptionForAPIConfig_RedirectsToErrorPage()
        {
            mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(config => config[$"{ConfigSection.CalculationRunSettings}:{ConfigSection.CalculationRunApi}"])
                             .Returns((string)null);

            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            byte[] value = System.Text.Encoding.UTF8.GetBytes("TestRun");
            mockSession.Setup(s => s.TryGetValue(SessionConstants.CalculationName, out value)).Returns(true);

            mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);

            var controller = new CalculationRunNameController(mockConfiguration.Object, mockClientFactory.Object, mockLogger.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };

            var result = await controller.Confirmation();

            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(ActionNames.StandardErrorIndex, redirectResult.ActionName);
            Assert.AreEqual("StandardError", redirectResult.ControllerName);
        }

        [TestMethod]
        public async Task RunCalculatorConfirmation_NullExceptionForYearConfig_RedirectsToErrorPage()
        {
            var mockApiSection = new Mock<IConfigurationSection>();

            var mockSettingsSection = new Mock<IConfigurationSection>();
            mockSettingsSection
                .Setup(s => s.GetSection(ConfigSection.CalculationRunApi))
                .Returns(mockApiSection.Object);

            mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration
                .Setup(c => c.GetSection(ConfigSection.CalculationRunSettings))
                .Returns(mockSettingsSection.Object);

            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            byte[] value = System.Text.Encoding.UTF8.GetBytes("TestRun");
            mockSession.Setup(s => s.TryGetValue(SessionConstants.CalculationName, out value)).Returns(true);

            mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);

            var controller = new CalculationRunNameController(mockConfiguration.Object, mockClientFactory.Object, mockLogger.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };

            var result = await controller.Confirmation();

            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(ActionNames.StandardErrorIndex, redirectResult.ActionName);
            Assert.AreEqual("StandardError", redirectResult.ControllerName);
        }

        [TestMethod]
        public async Task RunCalculatorConfirmation_SessionValueIsEmpty_RedirectsToErrorPage()
        {
            var mockSettingsSection = new Mock<IConfigurationSection>();

            var mockParameterYearSection = new Mock<IConfigurationSection>();
            mockParameterYearSection.Setup(s => s.Value).Returns(string.Empty);

            var mockParameterCalculationRunApiSection = new Mock<IConfigurationSection>();
            mockParameterCalculationRunApiSection.Setup(s => s.Value).Returns("http://localhost:5055/v1/calculatorRun");

            mockSettingsSection
                .Setup(s => s.GetSection(ConfigSection.CalculationRunApi))
                .Returns(mockParameterCalculationRunApiSection.Object);

            mockSettingsSection
                .Setup(s => s.GetSection(ConfigSection.RunParameterYear))
                .Returns(mockParameterYearSection.Object);

            mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration
                .Setup(c => c.GetSection(ConfigSection.CalculationRunSettings))
                .Returns(mockSettingsSection.Object);

            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            byte[] value = System.Text.Encoding.UTF8.GetBytes(string.Empty);
            mockSession.Setup(s => s.TryGetValue(SessionConstants.CalculationName, out value)).Returns(true);

            mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);

            var controller = new CalculationRunNameController(mockConfiguration.Object, mockClientFactory.Object, mockLogger.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            mockClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var result = await controller.Confirmation();

            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(ActionNames.StandardErrorIndex, redirectResult.ActionName);
            Assert.AreEqual("StandardError", redirectResult.ControllerName);
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
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);
            mockClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(mockHttpClient);
        }
    }
}