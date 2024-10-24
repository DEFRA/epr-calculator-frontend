using System.Net;
using System.Text;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Models;
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
        private Mock<IConfiguration> mockConfiguration;
        private Mock<IHttpClientFactory> mockClientFactory;
        private Mock<ILogger<CalculationRunNameController>> mockLogger;
        private CalculationRunNameController controller;
        private CalculatorRunNameValidator validationRules;

        [TestInitialize]
        public void Setup()
        {
            mockConfiguration = new Mock<IConfiguration>();
            mockClientFactory = new Mock<IHttpClientFactory>();
            mockLogger = new Mock<ILogger<CalculationRunNameController>>();
            controller = new CalculationRunNameController(mockConfiguration.Object, mockClientFactory.Object, mockLogger.Object);
            validationRules = new CalculatorRunNameValidator();

            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TestMethod]
        public void RunCalculator_CalculationRunNameController_View_Test()
        {
            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
        }

        [TestMethod]
        public void RunCalculator_ShouldReturnView_WhenCalculationNameIsInvalid()
        {
            controller.ModelState.AddModelError("CalculationName", "Enter a name for this calculation");
            var calculatorRunModel = new InitiateCalculatorRunModel() { CalculationName = null };
            var result = controller.RunCalculator(null) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
            var errorViewModel = controller.ViewBag.Errors as ErrorViewModel;
            Assert.IsNotNull(errorViewModel);
            Assert.AreEqual(ViewControlNames.CalculationRunName, errorViewModel.DOMElementId);
            Assert.AreEqual(ErrorMessages.CalculationRunNameEmpty, errorViewModel.ErrorMessage);
        }

        [TestMethod]
        public void RunCalculator_ShouldRedirect_IsOnlyAlphabets_WhenCalculationNameIsValid()
        {
            var mockHttpSession = new MockHttpSession();

            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var calculatorRunModel = new InitiateCalculatorRunModel() { CalculationName = "ValidCalculationName" };
            var result = controller.RunCalculator(calculatorRunModel) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.RunCalculatorConfirmation, result.ActionName);
            Assert.AreEqual("ValidCalculationName", mockHttpSession.GetString(SessionConstants.CalculationName));
        }

        [TestMethod]
        public void RunCalculator_ShouldRedirect_IsOnlyNumeric_WhenCalculationNameIsValid()
        {
            var mockHttpSession = new MockHttpSession();

            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var calculatorRunModel = new InitiateCalculatorRunModel() { CalculationName = "1234" };
            var result = controller.RunCalculator(calculatorRunModel) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.RunCalculatorConfirmation, result.ActionName);
            Assert.AreEqual("1234", mockHttpSession.GetString(SessionConstants.CalculationName));
        }

        [TestMethod]
        public void RunCalculator_ShouldRedirect_IsAplhaNumeric_WithNoSpace_WhenCalculationNameIsValid()
        {
            var mockHttpSession = new MockHttpSession();

            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var calculatorRunModel = new InitiateCalculatorRunModel() { CalculationName = "ValidCalculationName1234" };
            var result = controller.RunCalculator(calculatorRunModel) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.RunCalculatorConfirmation, result.ActionName);
            Assert.AreEqual("ValidCalculationName1234", mockHttpSession.GetString(SessionConstants.CalculationName));
        }

        [TestMethod]
        public void RunCalculator_WhenCalculationName__IsAplhaNumeric_WithSpace_IsValid()
        {
            var mockHttpSession = new MockHttpSession();

            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var calculatorRunModel = new InitiateCalculatorRunModel() { CalculationName = "ValidCalculationName 123" };
            var result = controller.RunCalculator(calculatorRunModel) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.RunCalculatorConfirmation, result.ActionName);
            Assert.AreEqual("ValidCalculationName 123", mockHttpSession.GetString(SessionConstants.CalculationName));
        }

        [TestMethod]
        public void RunCalculator_WhenCalculationNameIsProvided_ShouldSetSessionAndRedirect()
        {
            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            mockHttpContext.Setup(s => s.Session).Returns(mockSession.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            var calculatorRunModel = new InitiateCalculatorRunModel() { CalculationName = "TestCalculation" };
            byte[] calculationNameBytes = Encoding.UTF8.GetBytes(calculatorRunModel.CalculationName);

            var result = controller.RunCalculator(calculatorRunModel) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.RunCalculatorConfirmation, result.ActionName);
            mockSession.Verify(s => s.Set(SessionConstants.CalculationName, calculationNameBytes), Times.Once);
        }

        [TestMethod]
        public void RunCalculator_WhenCalculationNameIsEmpty_ShouldReturnViewWithError()
        {
            var calculatorRunModel = new InitiateCalculatorRunModel() { CalculationName = string.Empty };
            controller.ModelState.AddModelError("CalculationName", "Enter a name for this calculation");
            var result = controller.RunCalculator(calculatorRunModel) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
            Assert.IsTrue(controller.ViewBag.Errors is ErrorViewModel);
            var errorViewModel = controller.ViewBag.Errors as ErrorViewModel;
            Assert.AreEqual(ErrorMessages.CalculationRunNameEmpty, errorViewModel.ErrorMessage);
        }

        [TestMethod]
        public void RunCalculator_WhenCalculationNameIsTooLong_ShouldReturnViewWithError()
        {
            controller.ModelState.AddModelError("CalculationName", "Calculation name must contain no more than 100 characters");
            var calculatorRunModel = new InitiateCalculatorRunModel() { CalculationName = new string('a', 101) };
            var result = controller.RunCalculator(calculatorRunModel) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
            Assert.IsTrue(controller.ViewBag.Errors is ErrorViewModel);
            var errorViewModel = controller.ViewBag.Errors as ErrorViewModel;
            Assert.AreEqual(ErrorMessages.CalculationRunNameMaxLengthExceeded, errorViewModel.ErrorMessage);
        }

        [TestMethod]
        public void RunCalculator_WhenCalculationName_IsNotAlphaNumeric_ShouldReturnViewWithError()
        {
            controller.ModelState.AddModelError("CalculationName", "Calculation name must only contain numbers and letters");
            var calculatorRunModel = new InitiateCalculatorRunModel() { CalculationName = "%^&*$@" };
            var result = controller.RunCalculator(calculatorRunModel) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
            Assert.IsTrue(controller.ViewBag.Errors is ErrorViewModel);
            var errorViewModel = controller.ViewBag.Errors as ErrorViewModel;
            Assert.AreEqual(ErrorMessages.CalculationRunNameMustBeAlphaNumeric, errorViewModel.ErrorMessage);
        }

        [TestMethod]
        public void RunCalculator_Validator_Should_Have_Error_Is_Empty()
        {
            var model = new InitiateCalculatorRunModel { CalculationName = string.Empty };
            var result = validationRules.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CalculationName)
                .WithErrorMessage(ErrorMessages.CalculationRunNameEmpty);
        }

        [TestMethod]
        public void RunCalculator_Validator_Should_Have_Error_Exceeds_MaxLength()
        {
            var model = new InitiateCalculatorRunModel { CalculationName = new string('a', 101) };
            var result = validationRules.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CalculationName)
                .WithErrorMessage(ErrorMessages.CalculationRunNameMaxLengthExceeded);
        }

        [TestMethod]
        public void RunCalculator_Validator_Should_Have_Error_When_Is_Not_AlphaNumeric()
        {
            var model = new InitiateCalculatorRunModel { CalculationName = "test_123" };
            var result = validationRules.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CalculationName)
                .WithErrorMessage(ErrorMessages.CalculationRunNameMustBeAlphaNumeric);
        }

        [TestMethod]
        public void RunCalculator_Validator_Should_Not_Have_Error_Is_Valid()
        {
            var model = new InitiateCalculatorRunModel { CalculationName = "test123" };
            var result = validationRules.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.CalculationName);
        }

        [TestMethod]
        public void RunCalculator_Validator_Should_Not_Have_Error_Has_Spaces()
        {
            var model = new InitiateCalculatorRunModel { CalculationName = "test 123" };
            var result = validationRules.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.CalculationName);
        }

        [TestMethod]
        public void RunCalculatorConfirmation_ValidModel_RedirectsToConfirmation()
        {
            var model = new InitiateCalculatorRunModel { CalculationName = "TestRun" };
            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            mockHttpContext.Setup(s => s.Session).Returns(mockSession.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };
            var result = controller.RunCalculator(model);
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(ActionNames.RunCalculatorConfirmation, redirectResult.ActionName);
        }

        [TestMethod]
        public async Task RunCalculatorConfirmation_SuccessfulResponse_ReturnsConfirmationView()
        {
            SetupConfiguration();
            var mockHttpContext = SetupHttpContextWithSession("TestRun");
            controller = new CalculationRunNameController(mockConfiguration.Object, mockClientFactory.Object, mockLogger.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };
            var client = SetupHttpClient(HttpStatusCode.Accepted);
            mockClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);
            var result = await controller.Confirmation();
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(ViewNames.CalculationRunConfirmation, viewResult.ViewName);
        }

        [TestMethod]
        public async Task RunCalculatorConfirmation_NullExceptionForAPIConfig_RedirectsToErrorPage()
        {
            mockConfiguration.Setup(config => config[$"{ConfigSection.CalculatorRun}:{ConfigSection.CalculatorRunApi}"])
                .Returns((string)null);
            var mockHttpContext = SetupHttpContextWithSession("TestRun");
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
            SetupConfiguration(apiUrl: "http://localhost:5055/v1/calculatorRun", year: null);
            var mockHttpContext = SetupHttpContextWithSession("TestRun");
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
            SetupConfiguration();
            var mockHttpContext = SetupHttpContextWithSession(string.Empty);
            var controller = new CalculationRunNameController(mockConfiguration.Object, mockClientFactory.Object, mockLogger.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };
            var client = SetupHttpClient(HttpStatusCode.BadRequest);
            mockClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);
            var result = await controller.Confirmation();
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(ActionNames.StandardErrorIndex, redirectResult.ActionName);
            Assert.AreEqual("StandardError", redirectResult.ControllerName);
        }

        [TestMethod]
        public async Task RunCalculatorConfirmation_BadRequest_RedirectsToErrorPage()
        {
            SetupConfiguration();
            var mockHttpContext = SetupHttpContextWithSession("TestRun");
            var controller = new CalculationRunNameController(mockConfiguration.Object, mockClientFactory.Object, mockLogger.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };
            var client = SetupHttpClient(HttpStatusCode.BadRequest);
            mockClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);
            var result = await controller.Confirmation();
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(ActionNames.StandardErrorIndex, redirectResult.ActionName);
            Assert.AreEqual("StandardError", redirectResult.ControllerName);
        }

        [TestMethod]
        public async Task RunCalculatorConfirmation_ErrorRequest_RedirectsToErrorPage()
        {
            SetupConfiguration();
            var mockHttpContext = SetupHttpContextWithSession("TestRun");
            var controller = new CalculationRunNameController(mockConfiguration.Object, mockClientFactory.Object, mockLogger.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };
            var client = SetupHttpClient(HttpStatusCode.BadRequest);
            mockClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);
            var result = await controller.Confirmation();
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(ActionNames.StandardErrorIndex, redirectResult.ActionName);
            Assert.AreEqual("StandardError", redirectResult.ControllerName);
        }

        private void SetupConfiguration(string apiUrl = "http://localhost:5055/v1/calculatorRun", string year = "2024-25")
        {
            mockConfiguration.Setup(config => config[$"{ConfigSection.CalculatorRun}:{ConfigSection.CalculatorRunApi}"])
                .Returns(apiUrl);
            mockConfiguration.Setup(config => config[$"{ConfigSection.CalculatorRun}:{ConfigSection.RunParameterYear}"])
                .Returns(year);
        }

        private Mock<HttpContext> SetupHttpContextWithSession(string sessionValue)
        {
            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            byte[] value = Encoding.UTF8.GetBytes(sessionValue);
            mockSession.Setup(s => s.TryGetValue(SessionConstants.CalculationName, out value)).Returns(true);
            mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);
            return mockHttpContext;
        }

        private HttpClient SetupHttpClient(HttpStatusCode statusCode)
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode
                });
            return new HttpClient(mockHttpMessageHandler.Object);
        }
    }
}