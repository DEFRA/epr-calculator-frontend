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
public class CompletedRunControllerTests
{
    private const int RunId = 10;
    private const int RelativeYearValue = 2025;
    private const string UserName = "Test User";

    private Mock<IEprCalculatorApiService> apiService = null!;
    private DefaultHttpContext httpContext = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        apiService = new Mock<IEprCalculatorApiService>();
        httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, UserName)
            ]))
        };
    }

    [TestMethod]
    public async Task Index_WhenRunExistsAndClassificationIsEligible_ReturnsPostBillingFileViewWithExpectedModel()
    {
        // Arrange
        var run = BuildRun(RunClassification.FINAL_RUN_COMPLETED);
        apiService.Setup(service => service.GetCalculatorRun(RunId)).ReturnsAsync(run);
        var controller = BuildController();

        // Act
        var result = await controller.Index(RunId);

        // Assert
        var viewResult = result as ViewResult;
        Assert.IsNotNull(viewResult);
        Assert.AreEqual(ViewNames.PostBillingFileIndex, viewResult.ViewName);

        var model = viewResult.Model as PostBillingFileViewModel;
        Assert.IsNotNull(model);
        Assert.AreEqual(run, model.CalculatorRunStatus);
        apiService.Verify(service => service.GetCalculatorRun(RunId), Times.Once);
    }

    [TestMethod]
    public async Task Index_WhenRunIsMissing_RedirectsToStandardError()
    {
        // Arrange
        apiService.Setup(service => service.GetCalculatorRun(RunId)).ReturnsAsync((CalculatorRunDto?)null);
        var controller = BuildController();

        // Act
        var result = await controller.Index(RunId);

        // Assert
        var redirectResult = result as RedirectToActionResult;
        Assert.IsNotNull(redirectResult);
        Assert.AreEqual("Index", redirectResult.ActionName);
        Assert.AreEqual("StandardError", redirectResult.ControllerName);
        apiService.Verify(service => service.GetCalculatorRun(RunId), Times.Once);
    }

    [TestMethod]
    public async Task Index_WhenRunClassificationIsNotEligible_RedirectsToStandardError()
    {
        // Arrange
        var run = BuildRun(RunClassification.TEST_RUN);
        apiService.Setup(service => service.GetCalculatorRun(RunId)).ReturnsAsync(run);
        var controller = BuildController();

        // Act
        var result = await controller.Index(RunId);

        // Assert
        var redirectResult = result as RedirectToActionResult;
        Assert.IsNotNull(redirectResult);
        Assert.AreEqual("Index", redirectResult.ActionName);
        Assert.AreEqual("StandardError", redirectResult.ControllerName);
        apiService.Verify(service => service.GetCalculatorRun(RunId), Times.Once);
    }

    private CompletedRunController BuildController()
    {
        var controller = new CompletedRunController(apiService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };

        return controller;
    }

    private static CalculatorRunDto BuildRun(RunClassification runClassification)
    {
        return new CalculatorRunDto
        {
            RunId = RunId,
            RunName = $"Run {RunId}",
            RunClassification = runClassification,
            RelativeYear = new RelativeYear(RelativeYearValue),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = UserName,
            BillingRunStatus = BillingRunStatus.None
        };
    }
}
