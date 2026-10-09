using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using EPR.Calculator.Frontend.ViewModels.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests.Controllers;

[TestClass]
public class CalculationRunDetailsNewControllerTests
{
    private Mock<IEprCalculatorApiService> apiService = null!;
    private Fixture fixture = null!;
    private DefaultHttpContext httpContext = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        fixture = new Fixture();
        apiService = new Mock<IEprCalculatorApiService>();
        httpContext = new DefaultHttpContext();
    }

    [TestMethod]
    public async Task Index_RunExists_ReturnsDetailsViewWithRunData()
    {
        // Arrange
        var run = BuildRun();
        SetupGetCalculatorRun(run.RunId, run);
        var controller = BuildController();

        // Act
        var result = await controller.Index(run.RunId) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.CalculationRunDetailsNewIndex, result.ViewName);
        Assert.IsTrue(controller.ModelState.IsValid);

        var expectedModel = new CalculatorRunDetailsNewViewModel
        {
            RunId = run.RunId,
            RunName = run.RunName,
            RunClassification = run.RunClassification,
            CalculationRunStatus = run.CalculationRunStatus,
            BillingRunStatus = run.BillingRunStatus,
            RelativeYear = run.RelativeYear,
            CreatedAt = run.CreatedAt,
            CreatedBy = run.CreatedBy
        };
        Assert.AreEqual(expectedModel, result.Model);
    }

    [TestMethod]
    public async Task Index_RunNotFound_Throws_Exception()
    {
        // Arrange
        var runId = fixture.Create<int>();
        SetupGetCalculatorRun(runId, null);
        var controller = BuildController();

        // Act & Assert
        await Assert.ThrowsExceptionAsync<BadHttpRequestException>(() => controller.Index(runId));
    }

    [TestMethod]
    [DataRow(CalculationRunStatus.Errored, BillingRunStatus.None, ErrorMessages.RunDetailError)]
    [DataRow(CalculationRunStatus.Completed, BillingRunStatus.Errored, ErrorMessages.BillingRunDetailError)]
    public async Task Index_RunErrored_ReturnsErrorPageWithRunError(
        CalculationRunStatus calculationRunStatus,
        BillingRunStatus billingRunStatus,
        string expectedError)
    {
        // Arrange
        var run = BuildRun() with
        {
            CalculationRunStatus = calculationRunStatus,
            BillingRunStatus = billingRunStatus
        };
        SetupGetCalculatorRun(run.RunId, run);
        var controller = BuildController();

        // Act
        var result = await controller.Index(run.RunId) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.CalculationRunDetailsNewErrorPage, result.ViewName);
        CollectionAssert.AreEqual(new[] { expectedError }, GetErrorMessages(controller, run.RunName));

        var model = result.Model as CalculatorRunDetailsNewViewModel;
        Assert.IsNotNull(model);
        Assert.AreEqual(run.RunId, model.RunId);
    }

    [TestMethod]
    public async Task Index_CalculationAndBillingRunErrored_ReturnsErrorPageWithBothErrors()
    {
        // Arrange
        var run = BuildRun() with
        {
            CalculationRunStatus = CalculationRunStatus.Errored,
            BillingRunStatus = BillingRunStatus.Errored
        };
        SetupGetCalculatorRun(run.RunId, run);
        var controller = BuildController();

        // Act
        var result = await controller.Index(run.RunId) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.CalculationRunDetailsNewErrorPage, result.ViewName);
        CollectionAssert.AreEqual(
            new[] { ErrorMessages.RunDetailError, ErrorMessages.BillingRunDetailError },
            GetErrorMessages(controller, run.RunName));
    }

    [TestMethod]
    public async Task Submit_InvalidRunId_RedirectsToStandardError()
    {
        // Arrange
        var controller = BuildController();
        controller.ModelState.AddModelError(nameof(CalculatorRunDetailsNewFormModel.RunId), "Invalid run ID");

        // Act
        var result = await controller.Submit(BuildFormModel(0, CalculationRunOption.OutputClassify)) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ActionNames.Index, result.ActionName);
        Assert.AreEqual("StandardError", result.ControllerName);
        apiService.Verify(service => service.GetCalculatorRun(It.IsAny<int>()), Times.Never);
    }

    [TestMethod]
    public async Task Submit_InvalidModelState_ReturnsDetailsViewWithRepopulatedModel()
    {
        // Arrange
        var run = BuildRun();
        SetupGetCalculatorRun(run.RunId, run);
        var controller = BuildController();
        controller.ModelState.AddModelError("key", "model error");

        // Act
        var result = await controller.Submit(BuildFormModel(run.RunId, null)) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.CalculationRunDetailsNewIndex, result.ViewName);

        var model = result.Model as CalculatorRunDetailsNewViewModel;
        Assert.IsNotNull(model);
        Assert.AreEqual(run.RunId, model.RunId);
    }

    [TestMethod]
    public async Task Submit_InvalidModelStateWithValidRunId_ReturnsDetailsViewWithRepopulatedModel()
    {
        // Arrange
        var run = BuildRun();
        SetupGetCalculatorRun(run.RunId, run);
        var controller = BuildController();
        controller.ModelState.MarkFieldValid(nameof(CalculatorRunDetailsNewFormModel.RunId));
        controller.ModelState.AddModelError(
            nameof(CalculatorRunDetailsNewFormModel.SelectedCalcRunOption),
            ErrorMessages.CalcRunOptionNotSelected);

        // Act
        var result = await controller.Submit(BuildFormModel(run.RunId, null)) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.CalculationRunDetailsNewIndex, result.ViewName);

        var model = result.Model as CalculatorRunDetailsNewViewModel;
        Assert.IsNotNull(model);
        Assert.AreEqual(run.RunId, model.RunId);
    }

    [TestMethod]
    public async Task Submit_OutputClassifySelected_RedirectsToClassificationController()
    {
        // Arrange
        var runId = fixture.Create<int>();
        var controller = BuildController();

        // Act
        var result = await controller.Submit(BuildFormModel(runId, CalculationRunOption.OutputClassify)) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ActionNames.Index, result.ActionName);
        Assert.AreEqual(ControllerNames.ClassifyingCalculationRun, result.ControllerName);
        Assert.AreEqual(runId, (int)result.RouteValues!["RunId"]!);
    }

    [TestMethod]
    public async Task Submit_OutputDeleteSelected_RedirectsToDeleteController()
    {
        // Arrange
        var runId = fixture.Create<int>();
        var controller = BuildController();

        // Act
        var result = await controller.Submit(BuildFormModel(runId, CalculationRunOption.OutputDelete)) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ActionNames.Index, result.ActionName);
        Assert.AreEqual(ControllerNames.CalculationRunDelete, result.ControllerName);
        Assert.AreEqual(runId, (int)result.RouteValues!["RunId"]!);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow(CalculationRunOption.None)]
    [DataRow(CalculationRunOption.OutputTest)]
    public async Task Submit_NoActionableOptionSelected_RedirectsToIndexWithRunId(CalculationRunOption? selectedOption)
    {
        // Arrange
        var runId = fixture.Create<int>();
        var controller = BuildController();

        // Act
        var result = await controller.Submit(BuildFormModel(runId, selectedOption)) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ActionNames.Index, result.ActionName);
        Assert.IsNull(result.ControllerName);
        Assert.AreEqual(runId, (int)result.RouteValues!["RunId"]!);
    }

    private CalculatorRunDto BuildRun() => new()
    {
        RunId = fixture.Create<int>(),
        RunName = fixture.Create<string>(),
        RunClassification = RunClassification.None,
        RelativeYear = new RelativeYear(2025),
        CreatedAt = fixture.Create<DateTime>(),
        CreatedBy = fixture.Create<string>(),
        CalculationRunStatus = CalculationRunStatus.Completed,
        BillingRunStatus = BillingRunStatus.None
    };

    private void SetupGetCalculatorRun(int runId, CalculatorRunDto? run)
    {
        apiService.Setup(service => service.GetCalculatorRun(runId)).ReturnsAsync(run);
    }

    private static string[] GetErrorMessages(ControllerBase controller, string key) =>
        controller.ModelState[key]?.Errors.Select(error => error.ErrorMessage).ToArray() ?? [];

    private static CalculatorRunDetailsNewFormModel BuildFormModel(int runId, CalculationRunOption? selectedOption)
    {
        return new CalculatorRunDetailsNewFormModel
        {
            RunId = runId,
            SelectedCalcRunOption = selectedOption
        };
    }

    private CalculationRunDetailsNewController BuildController()
    {
        var controller = new CalculationRunDetailsNewController(apiService.Object);
        controller.ControllerContext.HttpContext = httpContext;
        return controller;
    }
}
