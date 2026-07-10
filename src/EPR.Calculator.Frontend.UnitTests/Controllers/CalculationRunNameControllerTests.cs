using System.Net;
using System.Security.Claims;
using System.Text;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.Validators;
using EPR.Calculator.Frontend.ViewModels;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests.Controllers;

[TestClass]
public class CalculationRunNameControllerTests
{
    private const string TestUser = "Test User";
    private const int RelativeYearValue = 2025;
    private Mock<IEprCalculatorApiService> apiService = null!;

    private IConfiguration configuration = null!;
    private DefaultHttpContext httpContext = null!;
    private Mock<ILogger<CalculationRunNameController>> logger = null!;
    private CalculatorRunNameValidator validator = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [CommonConstants.RelativeYearStartingMonth] = "4"
            })
            .Build();

        apiService = new Mock<IEprCalculatorApiService>();
        logger = new Mock<ILogger<CalculationRunNameController>>();
        validator = new CalculatorRunNameValidator();
        httpContext = new DefaultHttpContext
        {
            Session = BuildSession(),
            User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, TestUser)
            ], "TestAuth"))
        };
        httpContext.Session.SetInt32(SessionConstants.RelativeYear, RelativeYearValue);
    }

    [TestMethod]
    public void Index_ReturnsCalculationRunNameView_WithCurrentUser()
    {
        // Arrange
        var controller = BuildController();

        // Act
        var result = controller.Index() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);

        var model = result.Model as InitiateCalculatorRunViewModel;
        Assert.IsNotNull(model);
    }

    [TestMethod]
    public async Task RunCalculator_WhenModelStateIsInvalid_ReturnsIndexViewWithValidationError()
    {
        // Arrange
        var controller = BuildController();
        controller.ModelState.AddModelError(
            nameof(InitiateCalculatorRunFormModel.CalculationName),
            ErrorMessages.CalculationRunNameEmpty);
        var model = BuildModel(null);

        // Act
        var result = await controller.RunCalculator(model) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);

        var returnedModel = result.Model as InitiateCalculatorRunViewModel;
        Assert.IsNotNull(returnedModel);
        Assert.IsNotNull(returnedModel.Errors);
        Assert.AreEqual(ViewControlNames.CalculationRunName, returnedModel.Errors.DOMElementId);
        Assert.AreEqual(ErrorMessages.CalculationRunNameEmpty, returnedModel.Errors.ErrorMessage);
        apiService.Verify(service => service.GetCalculatorRun(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task RunCalculator_WhenCalculationNameExists_ReturnsIndexViewWithNameExistsError()
    {
        // Arrange
        var calculationName = "Existing Calculation";
        apiService
            .Setup(service => service.GetCalculatorRun(calculationName))
            .ReturnsAsync(BuildExistingRun(calculationName));
        var controller = BuildController();
        var model = BuildModel(calculationName);

        // Act
        var result = await controller.RunCalculator(model) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);

        var returnedModel = result.Model as InitiateCalculatorRunViewModel;
        Assert.IsNotNull(returnedModel);
        Assert.IsNotNull(returnedModel.Errors);
        Assert.AreEqual(ErrorMessages.CalculationRunNameExists, returnedModel.Errors.ErrorMessage);
        apiService.Verify(service => service.CallApi(
            It.IsAny<HttpMethod>(),
            It.IsAny<string>(),
            It.IsAny<IDictionary<string, string?>?>(),
            It.IsAny<object?>()), Times.Never);
    }

    [TestMethod]
    public async Task RunCalculator_WhenNameIsUniqueAndCreateIsAccepted_RedirectsToConfirmation()
    {
        // Arrange
        const string userInputName = "  Unique Run  ";
        const string trimmedName = "Unique Run";
        CreateCalculatorRunDto? capturedCreateRunDto = null;

        apiService
            .Setup(service => service.GetCalculatorRun(trimmedName))
            .ReturnsAsync((CalculatorRunDto?)null);

        apiService
            .Setup(service => service.CallApi(
                HttpMethod.Post,
                "v1/calculatorRun",
                It.IsAny<IDictionary<string, string?>?>(),
                It.IsAny<object?>()))
            .Callback<HttpMethod, string, IDictionary<string, string?>?, object?>((_, _, _, body) => { capturedCreateRunDto = body as CreateCalculatorRunDto; })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Accepted));

        var controller = BuildController();
        var model = BuildModel(userInputName);

        // Act
        var result = await controller.RunCalculator(model) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ActionNames.RunCalculatorConfirmation, result.ActionName);
        apiService.Verify(service => service.GetCalculatorRun(trimmedName), Times.Once);
        apiService.Verify(service => service.CallApi(
            HttpMethod.Post,
            "v1/calculatorRun",
            It.IsAny<IDictionary<string, string?>?>(),
            It.IsAny<object?>()), Times.Once);
        Assert.IsNotNull(capturedCreateRunDto);
        Assert.AreEqual(trimmedName, capturedCreateRunDto.CalculatorRunName);
        Assert.AreEqual(TestUser, capturedCreateRunDto.CreatedBy);
        Assert.AreEqual(new RelativeYear(RelativeYearValue), capturedCreateRunDto.RelativeYear);
    }

    [TestMethod]
    public async Task RunCalculator_WhenApiReturnsUnprocessableEntityWithMessage_ReturnsErrorViewWithParsedMessage()
    {
        // Arrange
        const string calculationName = "Run One";
        const string errorMessage = "Already a calculator run running, wait your turn.";
        SetupUniqueNameScenario(
            calculationName,
            CreateJsonResponse(
                HttpStatusCode.UnprocessableEntity,
                $$"""{"message":"{{errorMessage}}"}"""));
        var controller = BuildController();
        var model = BuildModel(calculationName);

        // Act
        var result = await controller.RunCalculator(model) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.CalculationRunErrorIndex, result.ViewName);
        var errorViewModel = result.Model as CalculationRunErrorViewModel;
        Assert.IsNotNull(errorViewModel);
        Assert.AreEqual(errorMessage, errorViewModel.ErrorMessage);
    }

    [TestMethod]
    public async Task RunCalculator_WhenApiReturnsUnprocessableEntityWithoutMessage_ReturnsErrorViewWithDefaultMessage()
    {
        // Arrange
        const string calculationName = "Run Two";
        SetupUniqueNameScenario(
            calculationName,
            CreateJsonResponse(HttpStatusCode.UnprocessableEntity, "{}"));
        var controller = BuildController();
        var model = BuildModel(calculationName);

        // Act
        var result = await controller.RunCalculator(model) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.CalculationRunErrorIndex, result.ViewName);
        var errorViewModel = result.Model as CalculationRunErrorViewModel;
        Assert.IsNotNull(errorViewModel);
        Assert.AreEqual("An error occurred. Please try again.", errorViewModel.ErrorMessage);
    }

    [TestMethod]
    public async Task RunCalculator_WhenApiReturnsUnprocessableEntityWithInvalidJson_ReturnsErrorViewWithFallbackMessage()
    {
        // Arrange
        const string calculationName = "Run Three";
        SetupUniqueNameScenario(
            calculationName,
            CreateJsonResponse(HttpStatusCode.UnprocessableEntity, "not-json"));
        var controller = BuildController();
        var model = BuildModel(calculationName);

        // Act
        var result = await controller.RunCalculator(model) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.CalculationRunErrorIndex, result.ViewName);
        var errorViewModel = result.Model as CalculationRunErrorViewModel;
        Assert.IsNotNull(errorViewModel);
        Assert.AreEqual("Unable to process the error response.", errorViewModel.ErrorMessage);
    }

    [TestMethod]
    public async Task RunCalculator_WhenApiReturnsFailedDependency_ReturnsErrorViewWithRawMessage()
    {
        // Arrange
        const string calculationName = "Run Four";
        const string failedDependencyMessage =
            "{ \"message\": \"Default parameter settings and Lapcap data not available for the financial year 2024-2025.\" }";
        SetupUniqueNameScenario(
            calculationName,
            CreateJsonResponse(HttpStatusCode.FailedDependency, failedDependencyMessage));
        var controller = BuildController();
        var model = BuildModel(calculationName);

        // Act
        var result = await controller.RunCalculator(model) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.CalculationRunErrorIndex, result.ViewName);
        var errorViewModel = result.Model as CalculationRunErrorViewModel;
        Assert.IsNotNull(errorViewModel);
        Assert.AreEqual(failedDependencyMessage, errorViewModel.ErrorMessage);
    }

    [TestMethod]
    public async Task RunCalculator_WhenApiReturnsUnexpectedStatus_RedirectsToStandardError()
    {
        // Arrange
        const string calculationName = "Run Five";
        SetupUniqueNameScenario(
            calculationName,
            CreateJsonResponse(HttpStatusCode.BadRequest, "{}"));
        var controller = BuildController();
        var model = BuildModel(calculationName);

        // Act
        var result = await controller.RunCalculator(model) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ActionName);
        Assert.AreEqual("StandardError", result.ControllerName);
    }

    [TestMethod]
    public void Confirmation_ReturnsConfirmationViewWithExpectedContent()
    {
        // Arrange
        const string calculationName = "Test Calculation";
        var controller = BuildController();

        // Act
        var result = controller.Confirmation(calculationName) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.CalculationRunConfirmation, result.ViewName);

        var model = result.Model as ConfirmationViewModel;
        Assert.IsNotNull(model);
        Assert.AreEqual(CalculatorRunNames.Title, model.Title);
        Assert.AreEqual(calculationName, model.Body);
        Assert.AreEqual(CalculatorRunNames.AdditionalParagraphs.Count, model.AdditionalParagraphs.Count);
        Assert.AreEqual(CalculatorRunNames.AdditionalParagraphs[0], model.AdditionalParagraphs[0]);
    }

    [TestMethod]
    public void CalculatorRunNameValidator_WhenCalculationNameIsEmpty_ReturnsValidationError()
    {
        // Arrange
        var model = BuildModel(string.Empty);

        // Act
        var result = validator.TestValidate(model);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.CalculationName)
            .WithErrorMessage(ErrorMessages.CalculationRunNameEmpty);
    }

    [TestMethod]
    public void CalculatorRunNameValidator_WhenCalculationNameExceeds100Characters_ReturnsValidationError()
    {
        // Arrange
        var model = BuildModel(new string('a', 101));

        // Act
        var result = validator.TestValidate(model);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.CalculationName)
            .WithErrorMessage(ErrorMessages.CalculationRunNameMaxLengthExceeded);
    }

    [TestMethod]
    public void CalculatorRunNameValidator_WhenCalculationNameIsNotAlphaNumeric_ReturnsValidationError()
    {
        // Arrange
        var model = BuildModel("test_123");

        // Act
        var result = validator.TestValidate(model);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.CalculationName)
            .WithErrorMessage(ErrorMessages.CalculationRunNameMustBeAlphaNumeric);
    }

    [TestMethod]
    public void CalculatorRunNameValidator_WhenCalculationNameIsAlphaNumeric_ReturnsNoValidationError()
    {
        // Arrange
        var model = BuildModel("test123");

        // Act
        var result = validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CalculationName);
    }

    [TestMethod]
    public void CalculatorRunNameValidator_WhenCalculationNameContainsSpaces_ReturnsNoValidationError()
    {
        // Arrange
        var model = BuildModel("test 123");

        // Act
        var result = validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CalculationName);
    }

    private CalculationRunNameController BuildController()
    {
        var controller = new CalculationRunNameController(
            configuration,
            apiService.Object,
            logger.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    private void SetupUniqueNameScenario(string calculationName, HttpResponseMessage createResponse)
    {
        apiService
            .Setup(service => service.GetCalculatorRun(calculationName))
            .ReturnsAsync((CalculatorRunDto?)null);

        apiService
            .Setup(service => service.CallApi(
                HttpMethod.Post,
                "v1/calculatorRun",
                It.IsAny<IDictionary<string, string?>?>(),
                It.IsAny<object?>()))
            .ReturnsAsync(createResponse);
    }

    private static CalculatorRunDto BuildExistingRun(string runName)
    {
        return new CalculatorRunDto
        {
            RunId = 1,
            RunName = runName,
            RelativeYear = new RelativeYear(RelativeYearValue),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = TestUser
        };
    }

    private static InitiateCalculatorRunFormModel BuildModel(string? calculationName)
    {
        return new InitiateCalculatorRunFormModel
        {
            CalculationName = calculationName
        };
    }

    private static HttpResponseMessage CreateJsonResponse(HttpStatusCode statusCode, string content)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };
    }

    private static ISession BuildSession()
    {
        var storage = new Dictionary<string, byte[]>();
        var session = new Mock<ISession>();
        session.SetupGet(x => x.Id).Returns("unit-test-session");
        session.SetupGet(x => x.IsAvailable).Returns(true);
        session.SetupGet(x => x.Keys).Returns(() => storage.Keys);
        session.Setup(x => x.Clear()).Callback(storage.Clear);
        session.Setup(x => x.Remove(It.IsAny<string>())).Callback<string>(key => storage.Remove(key));
        session.Setup(x => x.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Callback<string, byte[]>((key, value) => storage[key] = value);
        session.Setup(x => x.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]?>.IsAny))
            .Returns((string key, out byte[]? value) =>
            {
                var found = storage.TryGetValue(key, out var data);
                value = data;
                return found;
            });
        session.Setup(x => x.LoadAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        session.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        return session.Object;
    }
}
