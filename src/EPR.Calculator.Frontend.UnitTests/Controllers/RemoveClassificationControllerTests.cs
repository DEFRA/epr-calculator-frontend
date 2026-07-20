using System.Net;
using System.Security.Claims;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests.Controllers;

[TestClass]
public class RemoveClassificationControllerTests
{
    private const int RunId = 123;
    private const string UserName = "Test User";
    private const string UpdateClassificationRelativePath = "v2/calculatorRuns";

    private Mock<IEprCalculatorApiService> apiService = null!;
    private RemoveClassificationController controller = null!;
    private Mock<ILogger<RemoveClassificationController>> logger = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        apiService = new Mock<IEprCalculatorApiService>();
        logger = new Mock<ILogger<RemoveClassificationController>>();
        controller = new RemoveClassificationController(apiService.Object, logger.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, UserName)
        }));
        httpContext.Request.Headers.Referer = $"https://localhost/{ControllerNames.CalculationRunDetails}/{RunId}";

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [TestMethod]
    public async Task Index_WhenCalculatorRunExists_ReturnsViewWithExpectedModel()
    {
        // Arrange
        apiService.Setup(service => service.GetCalculatorRun(RunId)).ReturnsAsync(BuildRun());

        // Act
        var result = await controller.Index(RunId) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.RemoveClassification, result.ViewName);

        var model = result.Model as RemoveRunClassificationViewModel;
        Assert.IsNotNull(model);
        Assert.AreEqual(RunId, model.RunId);
        Assert.IsNull(model.ClassifyRunType);
        apiService.Verify(service => service.GetCalculatorRun(RunId), Times.Once);
    }

    [TestMethod]
    public async Task Index_WhenCalculatorRunIsMissing_RedirectsToStandardError()
    {
        // Arrange
        apiService.Setup(service => service.GetCalculatorRun(RunId)).ReturnsAsync((CalculatorRunDto?)null);

        // Act
        var result = await controller.Index(RunId) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ActionName);
        Assert.AreEqual("StandardError", result.ControllerName);
        apiService.Verify(service => service.GetCalculatorRun(RunId), Times.Once);
    }

    [TestMethod]
    public async Task Submit_WhenModelStateIsInvalid_ReturnsViewWithRehydratedModel()
    {
        // Arrange
        var submittedModel = BuildSubmitModel((int)RunClassification.TEST_RUN);
        controller.ModelState.AddModelError(nameof(RemoveRunClassificationViewModel.ClassifyRunType), "Required");
        apiService.Setup(service => service.GetCalculatorRun(RunId)).ReturnsAsync(BuildRun());

        // Act
        var result = await controller.Submit(submittedModel) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.RemoveClassification, result.ViewName);

        var model = result.Model as RemoveRunClassificationViewModel;
        Assert.IsNotNull(model);
        Assert.AreEqual(RunId, model.RunId);
        apiService.Verify(service => service.GetCalculatorRun(RunId), Times.Once);
        apiService.Verify(service => service.CallApi(
                HttpMethod.Put,
                UpdateClassificationRelativePath,
                It.IsAny<IDictionary<string, string?>?>(),
                It.IsAny<object?>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Submit_WhenDeleteSelected_RedirectsToCalculationRunDelete()
    {
        // Arrange
        var model = BuildSubmitModel((int)RunClassification.DELETED);

        // Act
        var result = await controller.Submit(model) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ActionNames.Index, result.ActionName);
        Assert.AreEqual(ControllerNames.CalculationRunDelete, result.ControllerName);
        Assert.AreEqual(RunId, result.RouteValues!["runId"]);
        apiService.Verify(service => service.CallApi(
                HttpMethod.Put,
                UpdateClassificationRelativePath,
                It.IsAny<IDictionary<string, string?>?>(),
                It.IsAny<object?>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Submit_WhenTestRunSelectedAndApiReturnsCreated_RedirectsToClassifyRunConfirmation()
    {
        // Arrange
        var model = BuildSubmitModel((int)RunClassification.TEST_RUN);
        apiService
            .Setup(service => service.CallApi(
                HttpMethod.Put,
                UpdateClassificationRelativePath,
                It.IsAny<IDictionary<string, string?>?>(),
                It.IsAny<object?>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Created));

        // Act
        var result = await controller.Submit(model) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ActionNames.Index, result.ActionName);
        Assert.AreEqual(ControllerNames.ClassifyRunConfirmation, result.ControllerName);
        Assert.AreEqual(RunId, result.RouteValues!["runId"]);
        apiService.Verify(service => service.CallApi(
                HttpMethod.Put,
                UpdateClassificationRelativePath,
                It.IsAny<IDictionary<string, string?>?>(),
                It.Is<object?>(body => IsExpectedTestRunClassification(body))),
            Times.Once);
    }

    [TestMethod]
    public async Task Submit_WhenTestRunSelectedAndApiReturnsNonCreated_RedirectsToStandardError()
    {
        // Arrange
        var model = BuildSubmitModel((int)RunClassification.TEST_RUN);
        apiService
            .Setup(service => service.CallApi(
                HttpMethod.Put,
                UpdateClassificationRelativePath,
                It.IsAny<IDictionary<string, string?>?>(),
                It.IsAny<object?>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

        // Act
        var result = await controller.Submit(model) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ActionName);
        Assert.AreEqual("StandardError", result.ControllerName);
        apiService.Verify(service => service.CallApi(
                HttpMethod.Put,
                UpdateClassificationRelativePath,
                It.IsAny<IDictionary<string, string?>?>(),
                It.Is<object?>(body => IsExpectedTestRunClassification(body))),
            Times.Once);
    }

    [TestMethod]
    public async Task Submit_WhenClassificationTypeIsUnexpected_RedirectsToStandardError()
    {
        // Arrange
        var model = BuildSubmitModel(999);

        // Act
        var result = await controller.Submit(model) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ActionName);
        Assert.AreEqual("StandardError", result.ControllerName);
        apiService.Verify(service => service.CallApi(
                HttpMethod.Put,
                UpdateClassificationRelativePath,
                It.IsAny<IDictionary<string, string?>?>(),
                It.IsAny<object?>()),
            Times.Never);
    }

    private static RemoveRunClassificationFormModel BuildSubmitModel(int? classifyRunType)
    {
        return new RemoveRunClassificationFormModel
        {
            RunId = RunId,
            ClassifyRunType = classifyRunType
        };
    }

    private static CalculatorRunDto BuildRun()
    {
        return new CalculatorRunDto
        {
            RunId = RunId,
            RunClassification = RunClassification.UNCLASSIFIED,
            RelativeYear = new RelativeYear(2025),
            RunName = $"Run {RunId}",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = UserName,
            BillingRunStatus = BillingRunStatus.None
        };
    }

    private static bool IsExpectedTestRunClassification(object? body)
    {
        var dto = body as ClassificationDto;
        return dto != null
               && dto.RunId == RunId
               && dto.ClassificationId == (int)RunClassification.TEST_RUN;
    }
}
