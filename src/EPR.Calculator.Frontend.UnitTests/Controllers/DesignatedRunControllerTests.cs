using System.Security.Claims;
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
public class DesignatedRunControllerTests
{
    private static readonly RunClassification[] EligibleRunClassifications =
    [
        RunClassification.INITIAL_RUN,
        RunClassification.INTERIM_RECALCULATION_RUN,
        RunClassification.FINAL_RUN,
        RunClassification.FINAL_RECALCULATION_RUN,
        RunClassification.TEST_RUN
    ];

    private DesignatedRunController controller = null!;

    private Mock<IEprCalculatorApiService> mockApiService = null!;

    public static IEnumerable<object[]> IneligibleRunClassifications =>
        Enum.GetValues<RunClassification>()
            .Except(EligibleRunClassifications)
            .Select(runClassification => new object[] { runClassification });

    [TestInitialize]
    public void TestInitialize()
    {
        mockApiService = new Mock<IEprCalculatorApiService>(MockBehavior.Strict);
        controller = CreateController(CreateHttpContext("Test User", "https://testhost/CalculationRunDetailsNew/1"));
    }

    [TestMethod]
    [DataRow(RunClassification.INITIAL_RUN)]
    [DataRow(RunClassification.INTERIM_RECALCULATION_RUN)]
    [DataRow(RunClassification.FINAL_RUN)]
    [DataRow(RunClassification.FINAL_RECALCULATION_RUN)]
    [DataRow(RunClassification.TEST_RUN)]
    public async Task Index_WhenRunClassificationIsEligible_ReturnsClassifyRunConfirmationView(RunClassification runClassification)
    {
        // Arrange
        var run = CreateCalculatorRun(1, runClassification);
        mockApiService
            .Setup(service => service.GetCalculatorRun(run.RunId))
            .ReturnsAsync(run);

        // Act
        var result = await controller.Index(run.RunId);

        // Assert
        var viewResult = result as ViewResult;
        Assert.IsNotNull(viewResult);
        Assert.AreEqual(ViewNames.ClassifyRunConfirmationIndex, viewResult.ViewName);

        var viewModel = viewResult.Model as ClassifyRunConfirmationViewModel;
        Assert.IsNotNull(viewModel);
        Assert.AreEqual(run.RunId, viewModel.CalculatorRunDetails.RunId);
        Assert.AreEqual(runClassification, viewModel.CalculatorRunDetails.RunClassification);
    }

    [TestMethod]
    [DynamicData(nameof(IneligibleRunClassifications))]
    public async Task Index_WhenRunClassificationIsIneligible_RedirectsToStandardError(RunClassification runClassification)
    {
        // Arrange
        const int runId = 10;
        mockApiService
            .Setup(service => service.GetCalculatorRun(runId))
            .ReturnsAsync(CreateCalculatorRun(runId, runClassification));

        // Act
        var result = await controller.Index(runId);

        // Assert
        var redirectResult = result as RedirectToActionResult;
        Assert.IsNotNull(redirectResult);
        Assert.AreEqual("Index", redirectResult.ActionName);
        Assert.AreEqual("StandardError", redirectResult.ControllerName);
    }

    [TestMethod]
    public async Task Index_WhenRunIsMissing_RedirectsToStandardError()
    {
        // Arrange
        const int runId = 123;
        mockApiService
            .Setup(service => service.GetCalculatorRun(runId))
            .ReturnsAsync((CalculatorRunDto?)null);

        // Act
        var result = await controller.Index(runId);

        // Assert
        var redirectResult = result as RedirectToActionResult;
        Assert.IsNotNull(redirectResult);
        Assert.AreEqual("Index", redirectResult.ActionName);
        Assert.AreEqual("StandardError", redirectResult.ControllerName);
    }

    [TestMethod]
    public async Task Index_WhenReferrerIsDashboard_ShowsBackLink()
    {
        // Arrange
        const int runId = 1;
        controller = CreateController(CreateHttpContext("Test User", "https://testhost/"));
        mockApiService
            .Setup(service => service.GetCalculatorRun(runId))
            .ReturnsAsync(CreateCalculatorRun(runId, RunClassification.INITIAL_RUN));

        // Act
        var result = await controller.Index(runId);

        // Assert
        var viewResult = result as ViewResult;
        Assert.IsNotNull(viewResult);

        var viewModel = viewResult.Model as ClassifyRunConfirmationViewModel;
        Assert.IsNotNull(viewModel);
    }

    [TestMethod]
    public async Task Index_WhenUserNameIsMissing_UsesUnknownUser()
    {
        // Arrange
        const int runId = 1;
        controller = CreateController(CreateHttpContext(null, "https://testhost/CalculationRunDetailsNew/1"));
        mockApiService
            .Setup(service => service.GetCalculatorRun(runId))
            .ReturnsAsync(CreateCalculatorRun(runId, RunClassification.FINAL_RUN));

        // Act
        var result = await controller.Index(runId);

        // Assert
        var viewResult = result as ViewResult;
        Assert.IsNotNull(viewResult);

        var viewModel = viewResult.Model as ClassifyRunConfirmationViewModel;
        Assert.IsNotNull(viewModel);
    }

    private DesignatedRunController CreateController(HttpContext httpContext)
    {
        var controller = new DesignatedRunController(mockApiService.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    private static DefaultHttpContext CreateHttpContext(string? userName, string? referer)
    {
        var identity = userName is null
            ? new ClaimsIdentity()
            : new ClaimsIdentity([new Claim(ClaimTypes.Name, userName)]);

        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };

        if (!string.IsNullOrWhiteSpace(referer))
            context.Request.Headers.Referer = referer;

        return context;
    }

    private static CalculatorRunDto CreateCalculatorRun(int runId, RunClassification runClassification)
    {
        return new CalculatorRunDto
        {
            RunId = runId,
            RunClassification = runClassification,
            RelativeYear = new RelativeYear(2024),
            RunName = "Test run",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "Test User"
        };
    }
}
