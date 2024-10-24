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
        private Mock<ILogger<CalculationRunNameController>> _mockLogger;

        [TestInitialize]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<CalculationRunNameController>>();
            mockClientFactory = new Mock<IHttpClientFactory>();
            mockHttpSession = new MockHttpSession();
            _controller = new CalculationRunNameController(configuration, mockClientFactory.Object, _mockLogger.Object);
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
        public void RunCalculatorConfirmation_ReturnsViewResult_WithCorrectViewName()
        {
            var result = _controller.Confirmation() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunConfirmation, result.ViewName);
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