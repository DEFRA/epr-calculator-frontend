using System.Net;
using System.Security.Claims;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Helpers;
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
public class SendBillingFileControllerTests
{
    private const string TestUserName = "Test User";

    private Mock<IEprCalculatorApiService> apiService = null!;
    private DefaultHttpContext httpContext = null!;
    private TelemetryClient telemetryClient = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        apiService = new Mock<IEprCalculatorApiService>();
        telemetryClient = new TelemetryClient(new TelemetryConfiguration());
        httpContext = CreateHttpContext();
    }

    [TestMethod]
    public async Task Index_RunNotFound_RedirectsToStandardError()
    {
        // Arrange
        const int runId = 123;
        apiService.Setup(service => service.GetCalculatorRun(runId))
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
    public async Task Index_BillingFileNotLatest_RedirectsToStandardError()
    {
        // Arrange
        const int runId = 123;
        apiService.Setup(service => service.GetCalculatorRun(runId))
            .ReturnsAsync(CreateRunDto(runId, "Run 123", false));
        var controller = BuildController();

        // Act
        var result = await controller.Index(runId) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ActionName);
        Assert.AreEqual("StandardError", result.ControllerName);
    }

    [TestMethod]
    public async Task Index_BillingFileLatest_ReturnsViewModelWithBackLink()
    {
        // Arrange
        const int runId = 456;
        const string runName = "Quarterly Billing Run";
        apiService.Setup(service => service.GetCalculatorRun(runId))
            .ReturnsAsync(CreateRunDto(runId, runName, true));
        var controller = BuildController();

        // Act
        var result = await controller.Index(runId) as ViewResult;

        // Assert
        Assert.IsNotNull(result);

        var model = result.Model as SendBillingFileViewModel;
        Assert.IsNotNull(model);
        Assert.AreEqual(runId, model.RunId);
        Assert.AreEqual(runName, model.CalcRunName);
    }

    [TestMethod]
    public async Task Submit_ModelStateInvalid_ReturnsViewAndDoesNotCallApi()
    {
        // Arrange
        const int runId = 13;
        apiService.Setup(service => service.GetCalculatorRun(runId))
            .ReturnsAsync(CreateRunDto(runId, $"Run {runId}", true));
        var controller = BuildController();
        var model = CreateFormModel(runId, true);
        controller.ModelState.AddModelError(nameof(model.ConfirmSend), "Invalid");

        // Act
        var result = await controller.Submit(model) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.SendBillingFileIndex, result.ViewName);

        var resultModel = result.Model as SendBillingFileViewModel;
        Assert.IsNotNull(resultModel);
        Assert.AreEqual(runId, resultModel.RunId);
        apiService.Verify(service => service.CallApi(
            It.IsAny<HttpMethod>(),
            It.IsAny<string>(),
            It.IsAny<IDictionary<string, string?>?>(),
            It.IsAny<object?>()), Times.Never);
    }

    [TestMethod]
    public async Task Submit_ApiAccepted_RedirectsToBillingFileSuccess()
    {
        // Arrange
        const int runId = 42;
        var controller = BuildController();
        var model = CreateFormModel(runId, true);

        HttpMethod? capturedMethod = null;
        string? capturedRelativePath = null;

        apiService.Setup(service => service.CallApi(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                It.IsAny<IDictionary<string, string?>?>(),
                It.IsAny<object?>()))
            .Callback<HttpMethod, string, IDictionary<string, string?>?, object?>((method, relativePath, _, _) =>
            {
                capturedMethod = method;
                capturedRelativePath = relativePath;
            })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Accepted));

        // Act
        var result = await controller.Submit(model) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ActionNames.BillingFileSuccess, result.ActionName);
        Assert.AreEqual(CommonUtil.GetControllerName(typeof(BillingInstructionsController)), result.ControllerName);
        Assert.AreEqual(HttpMethod.Post, capturedMethod);
        Assert.AreEqual($"v2/prepareBillingFileSendToFSS/{runId}", capturedRelativePath);
    }

    [TestMethod]
    public async Task Submit_ApiUnprocessableEntity_ReturnsIndexViewAndFlagsStaleBillingFile()
    {
        // Arrange
        const int runId = 43;
        apiService.Setup(service => service.GetCalculatorRun(runId))
            .ReturnsAsync(CreateRunDto(runId, $"Run {runId}", false));
        var controller = BuildController();
        var model = CreateFormModel(runId, true);

        apiService.Setup(service => service.CallApi(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                It.IsAny<IDictionary<string, string?>?>(),
                It.IsAny<object?>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.UnprocessableEntity));

        // Act
        var result = await controller.Submit(model) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ActionNames.Index, result.ViewName);

        var resultModel = result.Model as SendBillingFileViewModel;
        Assert.IsNotNull(resultModel);
        Assert.IsFalse(resultModel.IsBillingFileLatest);
    }

    [TestMethod]
    public async Task Submit_ApiError_RedirectsToStandardError()
    {
        // Arrange
        const int runId = 44;
        var controller = BuildController();
        var model = CreateFormModel(runId, true);

        apiService.Setup(service => service.CallApi(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                It.IsAny<IDictionary<string, string?>?>(),
                It.IsAny<object?>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("error")
            });

        // Act
        var result = await controller.Submit(model) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ActionName);
        Assert.AreEqual("StandardError", result.ControllerName);
    }

    private SendBillingFileController BuildController()
    {
        return new SendBillingFileController(telemetryClient, apiService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }

    private static SendBillingFileFormModel CreateFormModel(int runId, bool? confirmSend)
    {
        return new SendBillingFileFormModel
        {
            RunId = runId,
            ConfirmSend = confirmSend
        };
    }

    private static CalculatorRunDto CreateRunDto(int runId, string runName, bool isBillingFileLatest)
    {
        return new CalculatorRunDto
        {
            RunId = runId,
            RunName = runName,
            RelativeYear = new RelativeYear(2026),
            BillingFile = new CalculatorRunDto.BillingFileDto
            {
                Id = 1,
                IsLatest = isBillingFileLatest,
                HasBeenSentToFss = false,
                CsvFileName = "billing.csv",
                JsonFileName = "billing.json",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "test-user"
            }
        };
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        return new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, TestUserName)
            ]))
        };
    }
}
