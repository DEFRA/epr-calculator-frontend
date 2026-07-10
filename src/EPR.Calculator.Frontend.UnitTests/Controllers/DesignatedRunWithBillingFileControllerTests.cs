using System.Net;
using System.Security.Claims;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Enums;
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
public class DesignatedRunWithBillingFileControllerTests
{
    private const string TestUserName = "Test User";

    private Mock<IEprCalculatorApiService> apiService = null!;
    private DesignatedRunWithBillingFileController controller = null!;
    private TelemetryClient telemetryClient = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        apiService = new Mock<IEprCalculatorApiService>(MockBehavior.Strict);
        telemetryClient = new TelemetryClient(new TelemetryConfiguration());
        controller = BuildController(CreateHttpContext(TestUserName));
    }

    [TestMethod]
    public async Task Index_RunMissing_RedirectsToStandardError()
    {
        // Arrange
        const int runId = 1;
        apiService
            .Setup(service => service.GetCalculatorRun(runId))
            .ReturnsAsync((CalculatorRunDto?)null);

        // Act
        var result = await controller.Index(runId) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ActionName);
        Assert.AreEqual("StandardError", result.ControllerName);
    }

    [DataTestMethod]
    [DataRow("https://testhost/", false)]
    [DataRow("https://testhost/CalculationRunDetailsNew/123", true)]
    public async Task Index_RunFound_SetsBackLinkVisibilityFromReferer(string referer, bool expectedHideBackLink)
    {
        // Arrange
        const int runId = 7;
        var runDto = CreateCalculatorRun(runId);
        controller = BuildController(CreateHttpContext(TestUserName, referer));
        apiService
            .Setup(service => service.GetCalculatorRun(runId))
            .ReturnsAsync(runDto);

        // Act
        var result = await controller.Index(runId) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.CalculationRunOverviewIndex, result.ViewName);

        var viewModel = result.Model as CalculatorRunOverviewViewModel;
        Assert.IsNotNull(viewModel);
        Assert.AreEqual(runDto, viewModel.Run);
    }

    [TestMethod]
    public async Task Index_UserNameMissing_UsesUnknownUser()
    {
        // Arrange
        const int runId = 11;
        var runDto = CreateCalculatorRun(runId);
        controller = BuildController(CreateHttpContext(null));
        apiService
            .Setup(service => service.GetCalculatorRun(runId))
            .ReturnsAsync(runDto);

        // Act
        var result = await controller.Index(runId) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        var viewModel = result.Model as CalculatorRunOverviewViewModel;
        Assert.IsNotNull(viewModel);
    }

    [TestMethod]
    public void Submit_ModelStateInvalid_RedirectsToIndexWithRunId()
    {
        // Arrange
        const int runId = 5;
        controller.ModelState.AddModelError("test-error", "invalid");

        // Act
        var result = controller.Submit(runId) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ActionNames.Index, result.ActionName);
        Assert.AreEqual(runId, result.RouteValues!["runId"]);
    }

    [TestMethod]
    public void Submit_ModelStateValid_RedirectsToSendBillingFile()
    {
        // Arrange
        const int runId = 6;

        // Act
        var result = controller.Submit(runId) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ActionNames.Index, result.ActionName);
        Assert.AreEqual(ControllerNames.SendBillingFile, result.ControllerName);
        Assert.AreEqual(runId, result.RouteValues!["runId"]);
    }

    [TestMethod]
    public async Task GenerateDraftBillingFile_ApiSucceeds_RedirectsToRunOverview()
    {
        // Arrange
        const int runId = 15;
        HttpMethod? capturedMethod = null;
        string? capturedPath = null;

        apiService
            .Setup(service => service.CallApi(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                It.IsAny<IDictionary<string, string?>?>(),
                It.IsAny<object?>()))
            .Callback<HttpMethod, string, IDictionary<string, string?>?, object?>((method, relativePath, _, _) =>
            {
                capturedMethod = method;
                capturedPath = relativePath;
            })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        var result = await controller.GenerateDraftBillingFile(runId) as RedirectToRouteResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ActionNames.Index, result.RouteValues!["action"]);
        Assert.AreEqual(ControllerNames.CalculationRunOverview, result.RouteValues["controller"]);
        Assert.AreEqual(runId, result.RouteValues["runId"]);
        Assert.AreEqual(HttpMethod.Put, capturedMethod);
        Assert.AreEqual($"v1/producerBillingInstructionsAccept/{runId}", capturedPath);
    }

    [TestMethod]
    public async Task GenerateDraftBillingFile_ApiFails_ThrowsInvalidOperationException()
    {
        // Arrange
        const int runId = 22;
        HttpMethod? capturedMethod = null;
        string? capturedPath = null;

        apiService
            .Setup(service => service.CallApi(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                It.IsAny<IDictionary<string, string?>?>(),
                It.IsAny<object?>()))
            .Callback<HttpMethod, string, IDictionary<string, string?>?, object?>((method, relativePath, _, _) =>
            {
                capturedMethod = method;
                capturedPath = relativePath;
            })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        // Act
        var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => controller.GenerateDraftBillingFile(runId));

        // Assert
        Assert.AreEqual($"Failed to generate draft billing file for calculation run {runId}.", exception.Message);
        Assert.AreEqual(HttpMethod.Put, capturedMethod);
        Assert.AreEqual($"v1/producerBillingInstructionsAccept/{runId}", capturedPath);
    }

    private DesignatedRunWithBillingFileController BuildController(HttpContext context)
    {
        var controller = new DesignatedRunWithBillingFileController(apiService.Object, telemetryClient)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = context
            }
        };

        return controller;
    }

    private static DefaultHttpContext CreateHttpContext(string? userName, string? referer = null)
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

    private static CalculatorRunDto CreateCalculatorRun(int runId)
    {
        return new CalculatorRunDto
        {
            RunId = runId,
            RunClassification = RunClassification.INITIAL_RUN,
            RelativeYear = new RelativeYear(2026),
            RunName = $"Run {runId}",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test-user",
            BillingRunStatus = BillingRunStatus.Completed,
            BillingFile = new CalculatorRunDto.BillingFileDto
            {
                Id = 1,
                IsLatest = true,
                HasBeenSentToFss = false,
                CsvFileName = "billing.csv",
                JsonFileName = "billing.json",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "test-user"
            }
        };
    }
}
