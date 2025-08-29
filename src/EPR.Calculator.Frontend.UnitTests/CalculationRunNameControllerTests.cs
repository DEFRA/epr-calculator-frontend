using System.Configuration;
using System.Net;
using AutoFixture;
using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using EPR.Calculator.Frontend.Validators;
using EPR.Calculator.Frontend.ViewModels;
using FluentValidation.TestHelper;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Build.Framework;
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

        private CalculationRunNameController _controller = null!;
        private CalculatorRunNameValidator _validationRules = null!;
        private Mock<IHttpClientFactory> mockClientFactory = null!;
        private Mock<IConfiguration> mockConfiguration = null!;
        private Mock<ILogger<CalculationRunNameController>> mockLogger = null!;
        private Mock<ITempDataDictionary> _tempDataMock = null!;
        private Mock<ITokenAcquisition> mockTokenAcquisition = null!;

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
            _controller = new CalculationRunNameController(
                configuration,
                new Mock<IApiService>().Object,
                mockLogger.Object,
                mockTokenAcquisition.Object,
                new TelemetryClient(),
                new Mock<ICalculatorRunDetailsService>().Object);
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
            var result = await _controller.RunCalculator(calculatorRunModel);
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, viewResult.ViewName);
            var initiateCalculatorRunModel = viewResult.Model as InitiateCalculatorRunModel;
            Assert.IsNotNull(initiateCalculatorRunModel.Errors);
            Assert.AreEqual(ViewControlNames.CalculationRunName, initiateCalculatorRunModel.Errors.DOMElementId);
            Assert.AreEqual(ErrorMessages.CalculationRunNameEmpty, initiateCalculatorRunModel.Errors.ErrorMessage);
        }

        [TestMethod]
        public async Task RunCalculator_ShouldRedirect_IsOnlyAlphabets_WhenCalculationNameIsValid()
        {
            // Arrange
            var model = new InitiateCalculatorRunModel
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = "ValidCalculationName",
            };
            var controller = BuildTestClass(this.Fixture, HttpStatusCode.OK);

            // Act
            var result = await controller.RunCalculator(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
            Assert.AreNotEqual(ActionNames.RunCalculatorConfirmation, result.ViewName);
        }

        [TestMethod]
        public async Task RunCalculator_ShouldRedirect_IsOnlyNumeric_WhenCalculationNameIsValid()
        {
            // Arrange
            var calculatorRunModel = new InitiateCalculatorRunModel()
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = "1234",
            };

            var controller = BuildTestClass(this.Fixture, HttpStatusCode.OK);

            // Act
            var result = await controller.RunCalculator(calculatorRunModel) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
            Assert.AreNotEqual(ActionNames.RunCalculatorConfirmation, result.ViewName);
        }

        [TestMethod]
        public async Task RunCalculator_ShouldRedirect_IsAplhaNumeric_WithNoSpace_WhenCalculationNameIsValid()
        {
            // Arrange
            var calculatorRunModel = new InitiateCalculatorRunModel()
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = "ValidCalculationName1234",
            };

            var controller = BuildTestClass(this.Fixture, HttpStatusCode.OK);

            // Act
            var result = await controller.RunCalculator(calculatorRunModel) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
            Assert.AreNotEqual(ActionNames.RunCalculatorConfirmation, result.ViewName);
        }

        [TestMethod]
        public async Task RunCalculator_WhenCalculationName__IsAplhaNumeric_WithSpace_IsValid()
        {
            // Arrange
            var calculatorRunModel = new InitiateCalculatorRunModel()
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = "ValidCalculationName 123",
            };

            var controller = BuildTestClass(this.Fixture, HttpStatusCode.OK);

            // Act
            var result = await controller.RunCalculator(calculatorRunModel) as ViewResult;

            // Assert
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

            var controller = BuildTestClass(this.Fixture, HttpStatusCode.OK);
            _controller.ModelState.AddModelError("CalculationName", "Enter a name for this calculation");
            var result = await _controller.RunCalculator(calculatorRunModel) as ViewResult;
            Assert.IsNotNull(result);
            var initiateCalculatorRunModel = result.Model as InitiateCalculatorRunModel;
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
            Assert.IsTrue(initiateCalculatorRunModel.Errors is ErrorViewModel);
            var errorViewModel = initiateCalculatorRunModel.Errors as ErrorViewModel;
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
            var initiateCalculatorRunModel = result.Model as InitiateCalculatorRunModel;

            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
            Assert.IsTrue(initiateCalculatorRunModel.Errors is ErrorViewModel);
            var errorViewModel = initiateCalculatorRunModel.Errors as ErrorViewModel;
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
            var initiateCalculatorRunModel = result.Model as InitiateCalculatorRunModel;

            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
            Assert.IsTrue(initiateCalculatorRunModel.Errors is ErrorViewModel);
            var errorViewModel = initiateCalculatorRunModel.Errors as ErrorViewModel;
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

            var controller = BuildTestClass(
                this.Fixture,
                HttpStatusCode.OK, "{}");

            // Act
            var result = await controller.RunCalculator(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            var calculatorRunModel = result.Model as InitiateCalculatorRunModel;
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
            Assert.IsNotNull(calculatorRunModel.Errors);
            Assert.AreEqual(ErrorMessages.CalculationRunNameExists, ((ErrorViewModel)calculatorRunModel.Errors).ErrorMessage);
        }

        [TestMethod]
        public async Task RunCalculator_ValidModel_CalculationNameDoesNotExist_ShouldRedirectToConfirmation()
        {
            // Arrange
            var model = new InitiateCalculatorRunModel
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = "UniqueCalculation",
            };

            var controller = BuildTestClass(
                this.Fixture,
                // Return not found for the first API call, and accepted for the second.
                [(HttpStatusCode.NotFound, "{}"), (HttpStatusCode.Accepted, "{}")]);

            var result = await controller.RunCalculator(model) as RedirectToActionResult;
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
            _controller = new CalculationRunNameController(
                mockConfiguration.Object,
                new Mock<IApiService>().Object,
                mockLogger.Object,
                mockTokenAcquisition.Object,
                new TelemetryClient(),
                new Mock<ICalculatorRunDetailsService>().Object);
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
            var mockSession = new MockHttpSession();
            mockSession.SetString("accessToken", "something");
            mockSession.SetString(SessionConstants.FinancialYear, "2024-25");
            var context = new DefaultHttpContext()
            {
                Session = mockSession
            };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
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

            var controller = BuildTestClass(
                this.Fixture,
                // Return not found for the first API call, and accepted for the second.
                [(HttpStatusCode.NotFound, "{}"), (HttpStatusCode.Accepted, "{}")]);

            var result = await controller.RunCalculator(model);
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
            var result = _controller.Confirmation("Test") as ViewResult;

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
            var controller = new CalculationRunNameController(
                mockConfiguration.Object,
                new Mock<IApiService>().Object,
                mockLogger.Object,
                mockTokenAcquisition.Object,
                new TelemetryClient(),
                new Mock<ICalculatorRunDetailsService>().Object)
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
            // Arrange
            var calculationRunModel = new InitiateCalculatorRunModel
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = "TestRun"
            };

            var controller = BuildTestClass(this.Fixture, HttpStatusCode.UnprocessableEntity);

            // Act
            var result = await controller.RunCalculator(calculationRunModel) as ViewResult;

            // Assert
            Assert.AreEqual("~/Views/Shared/_CalculationRunError.cshtml", result.ViewName);
        }

        [TestMethod]
        public async Task RunCalculator_ShouldRedirectToError_WhenFailedDependency()
        {
            // Arrange
            var calculationRunModel = new InitiateCalculatorRunModel
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = "TestRun",
            };
            var errorMessage = "Default parameter settings and Lapcap data not available for the financial year 2024-2025.";
            var responseContent = $"{{ \"message\": \"{errorMessage}\" }}";

            var controller = BuildTestClass(this.Fixture, HttpStatusCode.FailedDependency, responseContent);

            // Act
            var result = (ViewResult)(await controller.RunCalculator(calculationRunModel));
            var model = result.Model as CalculationRunErrorViewModel;

            // Assert
            Assert.AreEqual("~/Views/Shared/_CalculationRunError.cshtml", result.ViewName);
            Assert.AreEqual(
                "{ \"message\": \"Default parameter settings and Lapcap data not available for the financial year 2024-2025.\" }",
                model.ErrorMessage);
        }

        [TestMethod]
        public async Task RunCalculator_ShouldRedirectToError_WhenUnprocessableEntity_ParsingError()
        {
            // Arrange
            var calculationRunModel = new InitiateCalculatorRunModel
            {
                CurrentUser = Fixture.Create<string>(),
                CalculationName = "TestRun"
            };

            var controller = BuildTestClass(this.Fixture, HttpStatusCode.UnprocessableEntity);

            // Act
            var result = await controller.RunCalculator(calculationRunModel);

            // Assert
            var redirectResult = result as ViewResult;
            Assert.AreEqual("~/Views/Shared/_CalculationRunError.cshtml", redirectResult.ViewName);
        }

        /// <summary>
        /// Builds a test class.
        /// </summary>
        /// <remarks>Accepts an object that will be serialised for
        /// the API service to return as it's response.</remarks>
        private CalculationRunNameController BuildTestClass(
            Fixture fixture,
            HttpStatusCode apiReturnStatusCode,
            CalculatorRunDto apiResponseData = null,
            CalculatorRunDetailsViewModel details = null)
            => BuildTestClass(
                fixture,
                apiReturnStatusCode,
                System.Text.Json.JsonSerializer.Serialize(apiResponseData ?? MockData.GetCalculatorRun()),
                details);

        /// <summary>
        /// Builds a test class.
        /// </summary>
        /// <remarks>Accepts a string for the API service to return as it's response.</remarks>
        private CalculationRunNameController BuildTestClass(
            Fixture fixture,
            HttpStatusCode apiReturnStatusCode,
            string apiResponseMessage,
            CalculatorRunDetailsViewModel details = null)
            => BuildTestClass(fixture, [(apiReturnStatusCode, apiResponseMessage)], details);

        /// <summary>
        /// Builds a test class where the api service returns a list of results that will be
        /// returned in sequence when the service is called multiple times.
        /// </summary>
        private CalculationRunNameController BuildTestClass(
            Fixture fixture,
            (HttpStatusCode ReturnStatusCode, string ResponseMessage)[] apiReturn,
            CalculatorRunDetailsViewModel details = null)
        {
            mockClientFactory = new Mock<IHttpClientFactory>();
            mockLogger = new Mock<ILogger<CalculationRunNameController>>();
            mockTokenAcquisition = new Mock<ITokenAcquisition>();
            mockTokenAcquisition
                .Setup(x => x.GetAccessTokenForUserAsync(It.IsAny<IEnumerable<string>>(), null, null, null, null))
                .ReturnsAsync("somevalue");

            details = details ?? Fixture.Create<CalculatorRunDetailsViewModel>();
            var mockApiService = TestMockUtils.BuildMockApiService(apiReturn)
                .Object;

            var testClass = new CalculationRunNameController(
                ConfigurationItems.GetConfigurationValues(),
                mockApiService,
                mockLogger.Object,
                mockTokenAcquisition.Object,
                new TelemetryClient(),
                TestMockUtils.BuildMockCalculatorRunDetailsService(details).Object);
            testClass.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                {
                    Session = TestMockUtils.BuildMockSession(fixture).Object,
                }
            };

            return testClass;
        }
    }
}