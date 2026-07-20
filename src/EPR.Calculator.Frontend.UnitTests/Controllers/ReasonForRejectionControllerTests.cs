using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests.Controllers;

[TestClass]
public class ReasonForRejectionControllerTests
{
    private const int RelativeYearValue = 2025;
    private const string TestUserName = "Test User";

    private Mock<IEprCalculatorApiService> apiService = null!;
    private DefaultHttpContext httpContext = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        apiService = new Mock<IEprCalculatorApiService>();
        httpContext = new DefaultHttpContext();
    }

    [TestMethod]
    public async Task Index_WhenRunExists_ReturnsReasonForRejectionViewWithEmptyReason()
    {
        // Arrange
        const int runId = 101;
        const string runName = "Run 101";
        apiService
            .Setup(service => service.GetCalculatorRun(runId))
            .ReturnsAsync(BuildRun(runId, runName));
        var controller = BuildController();

        // Act
        var result = await controller.Index(runId) as ViewResult;
        var model = result?.Model as AcceptRejectConfirmationViewModel;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.ReasonForRejectionIndex, result.ViewName);
        Assert.IsNotNull(model);
        Assert.AreEqual(runId, model.RunId);
        Assert.AreEqual(runName, model.RunName);
        Assert.AreEqual(BillingStatus.Rejected, model.Status);
        Assert.IsNull(model.Reason);
        apiService.Verify(service => service.GetCalculatorRun(runId), Times.Once);
    }

    [TestMethod]
    public async Task Index_WhenRunNotFound_RedirectsToStandardError()
    {
        // Arrange
        const int runId = 202;
        apiService
            .Setup(service => service.GetCalculatorRun(runId))
            .ReturnsAsync((CalculatorRunDto?)null);
        var controller = BuildController();

        // Act
        var result = await controller.Index(runId) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ActionName);
        Assert.AreEqual("StandardError", result.ControllerName);
    }

    [TestMethod]
    public async Task IndexPost_WhenModelStateIsInvalid_RedisplaysReasonForRejectionViewWithPostedReason()
    {
        // Arrange
        const int runId = 303;
        const string runName = "Run 303";
        const string postedReason = "   "; // Would be flagged by FluentValidation in the pipeline
        apiService
            .Setup(service => service.GetCalculatorRun(runId))
            .ReturnsAsync(BuildRun(runId, runName));
        var model = BuildModel(runId, postedReason);
        var controller = BuildController();
        controller.ModelState.AddModelError(nameof(model.Reason), ErrorMessages.ReasonForRejectionRequired);

        // Act
        var result = await controller.IndexPost(runId, model) as ViewResult;
        var resultModel = result?.Model as AcceptRejectConfirmationViewModel;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.ReasonForRejectionIndex, result.ViewName);
        Assert.IsNotNull(resultModel);
        Assert.AreEqual(runId, resultModel.RunId);
        Assert.AreEqual(runName, resultModel.RunName);
        Assert.AreEqual(BillingStatus.Rejected, resultModel.Status);
        Assert.AreEqual(postedReason, resultModel.Reason);
        Assert.IsFalse(controller.ModelState.IsValid);
    }

    [TestMethod]
    public async Task IndexPost_WhenModelStateIsValid_ReturnsAcceptRejectConfirmationViewWithPostedReason()
    {
        // Arrange
        const int runId = 404;
        const string runName = "Run 404";
        const string rejectionReason = "Threshold validation failed";
        apiService
            .Setup(service => service.GetCalculatorRun(runId))
            .ReturnsAsync(BuildRun(runId, runName));
        var model = BuildModel(runId, rejectionReason);
        var controller = BuildController();

        // Act
        var result = await controller.IndexPost(runId, model) as ViewResult;
        var resultModel = result?.Model as AcceptRejectConfirmationViewModel;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.AcceptRejectConfirmationIndex, result.ViewName);
        Assert.IsNotNull(resultModel);
        Assert.AreEqual(runId, resultModel.RunId);
        Assert.AreEqual(runName, resultModel.RunName);
        Assert.AreEqual(BillingStatus.Rejected, resultModel.Status);
        Assert.AreEqual(rejectionReason, resultModel.Reason);
        Assert.IsTrue(controller.ModelState.IsValid);
    }

    [TestMethod]
    public async Task IndexPost_WhenRunNotFound_RedirectsToStandardError()
    {
        // Arrange
        const int runId = 505;
        apiService
            .Setup(service => service.GetCalculatorRun(runId))
            .ReturnsAsync((CalculatorRunDto?)null);
        var model = BuildModel(runId, "Any reason");
        var controller = BuildController();

        // Act
        var result = await controller.IndexPost(runId, model) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ActionName);
        Assert.AreEqual("StandardError", result.ControllerName);
    }

    private ReasonForRejectionController BuildController()
    {
        return new ReasonForRejectionController(apiService.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }

    private static ReasonForRejectionFormModel BuildModel(int runId, string? reason)
    {
        return new ReasonForRejectionFormModel
        {
            RunId = runId,
            Reason = reason
        };
    }

    private static CalculatorRunDto BuildRun(int runId, string runName)
    {
        return new CalculatorRunDto
        {
            RunId = runId,
            RunName = runName,
            RunClassification = RunClassification.UNCLASSIFIED,
            RelativeYear = new RelativeYear(RelativeYearValue),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = TestUserName,
            BillingRunStatus = BillingRunStatus.None
        };
    }
}
