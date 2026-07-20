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
        var runId = fixture.Create<int>();
        SetupGetCalculatorRun(runId, new CalculatorRunDto { RunId = runId, RunClassification = RunClassification.UNCLASSIFIED });
        var controller = BuildController();

        // Act
        var result = await controller.Index(runId) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.CalculationRunDetailsNewIndex, result.ViewName);

        var model = result.Model as CalculatorRunDetailsNewViewModel;
        Assert.IsNotNull(model);
        Assert.AreEqual(runId, model.RunId);
    }

    [TestMethod]
    public async Task Index_RunNotFound_RedirectsToStandardError()
    {
        // Arrange
        var runId = fixture.Create<int>();
        SetupGetCalculatorRun(runId, null);
        var controller = BuildController();

        // Act
        var result = await controller.Index(runId) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ActionName);
        Assert.AreEqual("StandardError", result.ControllerName);
    }

    [TestMethod]
    public async Task Index_RunClassificationIsError_ReturnsErrorPageView()
    {
        // Arrange
        var runId = fixture.Create<int>();
        SetupGetCalculatorRun(runId, new CalculatorRunDto { RunId = runId, RunClassification = RunClassification.ERROR });
        var controller = BuildController();

        // Act
        var result = await controller.Index(runId) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.CalculationRunDetailsNewErrorPage, result.ViewName);
        Assert.IsFalse(controller.ModelState.IsValid);
    }

    [TestMethod]
    public async Task Submit_InvalidModelState_ReturnsDetailsViewWithRepopulatedModel()
    {
        // Arrange
        var runId = fixture.Create<int>();
        SetupGetCalculatorRun(runId, new CalculatorRunDto { RunId = runId, RunClassification = RunClassification.UNCLASSIFIED });
        var controller = BuildController();
        controller.ModelState.AddModelError("key", "model error");

        // Act
        var result = await controller.Submit(BuildFormModel(runId, null)) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.CalculationRunDetailsNewIndex, result.ViewName);

        var model = result.Model as CalculatorRunDetailsNewViewModel;
        Assert.IsNotNull(model);
        Assert.AreEqual(runId, model.RunId);
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
    public async Task Submit_NoOptionSelected_RedirectsToIndexWithRunId()
    {
        // Arrange
        var runId = fixture.Create<int>();
        var controller = BuildController();

        // Act
        var result = await controller.Submit(BuildFormModel(runId, null)) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ActionNames.Index, result.ActionName);
        Assert.IsNull(result.ControllerName);
        Assert.AreEqual(runId, (int)result.RouteValues!["RunId"]!);
    }

    private void SetupGetCalculatorRun(int runId, CalculatorRunDto? run)
    {
        apiService.Setup(service => service.GetCalculatorRun(runId)).ReturnsAsync(run);
    }

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
