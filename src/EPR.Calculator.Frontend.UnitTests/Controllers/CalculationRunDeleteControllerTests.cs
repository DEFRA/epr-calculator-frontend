using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests.Controllers;

[TestClass]
public class CalculationRunDeleteControllerTests
{
    private const string RefererUrl = "https://calculator/details/4";
    private Mock<IEprCalculatorApiService> apiService = null!;

    private Fixture fixture = null!;
    private DefaultHttpContext httpContext = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        fixture = new Fixture();
        apiService = new Mock<IEprCalculatorApiService>();
        httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Referer = RefererUrl;
    }

    [TestMethod]
    public async Task Index_ReturnsDeleteConfirmationViewWithRunDetails()
    {
        // Arrange
        var runId = fixture.Create<int>();
        var runName = fixture.Create<string>();
        apiService
            .Setup(service => service.GetCalculatorRun(runId))
            .ReturnsAsync(new CalculatorRunDto { RunId = runId, RunName = runName });
        var controller = BuildController();

        // Act
        var result = await controller.Index(runId) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.CalculationRunDeleteIndex, result.ViewName);

        var model = result.Model as CalculationRunDeleteViewModel;
        Assert.IsNotNull(model);
        Assert.AreEqual(runId, model.RunId);
        Assert.AreEqual(runName, model.RunName);
    }

    [TestMethod]
    public async Task DeleteConfirmationSuccess_ReturnsSuccessViewWhenApiReturnsCreated()
    {
        // Arrange
        var runId = fixture.Create<int>();
        var runName = fixture.Create<string>();
        apiService
            .Setup(service => service.GetCalculatorRun(runId))
            .ReturnsAsync(new CalculatorRunDto { RunId = runId, RunName = runName });
        var controller = BuildController();

        // Act
        var result = await controller.DeleteConfirmationSuccess(BuildRun(runId)) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.CalculationRunDeleteConfirmationSuccess, result.ViewName);

        var model = result.Model as CalculationRunDeleteViewModel;
        Assert.IsNotNull(model);
        Assert.AreEqual(runId, model.RunId);
        Assert.AreEqual(runName, model.RunName);
        apiService.Verify(service => service.DeleteCalculatorRun(runId), Times.Once);
    }

    [TestMethod]
    public async Task DeleteConfirmationSuccess_RedirectsToErrorWhenApiDoesNotReturnCreated()
    {
        // Arrange
        var runId = fixture.Create<int>();
        apiService
            .Setup(service => service.GetCalculatorRun(runId))
            .ReturnsAsync(new CalculatorRunDto { RunId = runId, RunName = fixture.Create<string>() });
        apiService
            .Setup(service => service.DeleteCalculatorRun(It.IsAny<int>()))
            .ThrowsAsync(new Exception());

        var controller = BuildController();

        // Act
        var result = await controller.DeleteConfirmationSuccess(BuildRun(runId)) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ActionName);
        Assert.AreEqual("StandardError", result.ControllerName);
    }

    private static CalculationRunDeleteFormModel BuildRun(int runId)
    {
        return new CalculationRunDeleteFormModel
        {
            RunId = runId
        };
    }

    private CalculationRunDeleteController BuildController()
    {
        var controller = new CalculationRunDeleteController(
            apiService.Object,
            new TelemetryClient(new TelemetryConfiguration()));
        controller.ControllerContext.HttpContext = httpContext;
        return controller;
    }
}
