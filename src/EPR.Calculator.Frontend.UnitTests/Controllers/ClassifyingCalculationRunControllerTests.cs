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
public class ClassifyingCalculationRunControllerTests
{
    private const int RelativeYearValue = 2025;
    private const int RunId = 123;
    private Mock<IEprCalculatorApiService> apiService = null!;

    private DefaultHttpContext httpContext = null!;
    private Mock<ILogger<SetRunClassificationController>> logger = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        apiService = new Mock<IEprCalculatorApiService>();
        logger = new Mock<ILogger<SetRunClassificationController>>();
        httpContext = new DefaultHttpContext
        {
            Session = BuildSession(),
            User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, "Test User")
            ]))
        };
        httpContext.Session.SetInt32(SessionConstants.RelativeYear, RelativeYearValue);
    }

    [TestMethod]
    public async Task Index_WhenDependenciesSucceed_ReturnsIndexViewWithExpectedModel()
    {
        // Arrange
        var classificationResponse = BuildClassificationResponse(
        [
            new CalculatorRunClassificationDto
            {
                Id = (int)RunClassification.INITIAL_RUN,
                Status = "INITIAL RUN",
                Description = string.Empty
            }
        ]);
        SetupClassificationApiSequence(classificationResponse);
        apiService.Setup(service => service.GetCalculatorRun(RunId)).ReturnsAsync(BuildRun(RunId));
        var controller = BuildController();

        // Act
        var result = await controller.Index(RunId) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.SetRunClassificationIndex, result.ViewName);

        var model = result.Model as SetRunClassificationViewModel;
        Assert.IsNotNull(model);
        Assert.AreEqual(RunId, model.RunId);
        Assert.AreEqual(CommonConstants.InitialRunDescription, model.RelativeYearClassifications.Classifications[0].Description);
        Assert.AreEqual(CommonConstants.InitialRunStatus, model.RelativeYearClassifications.Classifications[0].Status);
        apiService.Verify(service => service.Get<RelativeYearClassificationResponseDto>(
                "v1/ClassificationByRelativeYear",
                It.Is<IDictionary<string, string?>?>(query =>
                    query != null
                    && query.ContainsKey("RunId")
                    && query["RunId"] == RunId.ToString()
                    && query.ContainsKey("RelativeYearValue")
                    && query["RelativeYearValue"] == RelativeYearValue.ToString())),
            Times.Exactly(1));
    }

    [TestMethod]
    public async Task Index_WhenClassificationsApiReturnsNullPayload_RedirectsToStandardError()
    {
        // Arrange
        SetupClassificationApiSequence([null]);
        var controller = BuildController();

        // Act
        var result = await controller.Index(RunId) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ActionName);
        Assert.AreEqual("StandardError", result.ControllerName);
    }

    [TestMethod]
    public async Task Index_WhenCalculatorRunIsMissing_RedirectsToStandardError()
    {
        // Arrange
        SetupClassificationApiSequence(BuildClassificationResponse());
        apiService.Setup(service => service.GetCalculatorRun(RunId)).ReturnsAsync((CalculatorRunDto?)null);
        var controller = BuildController();

        // Act
        var result = await controller.Index(RunId) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ActionName);
        Assert.AreEqual("StandardError", result.ControllerName);
    }

    [TestMethod]
    public async Task Index_WhenSecondClassificationsApiCallFails_RedirectsToStandardError()
    {
        // Arrange
        SetupClassificationApiSequence(BuildClassificationResponse(), null);
        apiService.Setup(service => service.GetCalculatorRun(RunId)).ReturnsAsync(BuildRun(RunId));
        var controller = BuildController();

        // Act
        var result = await controller.Index(RunId) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ActionName);
        Assert.AreEqual("StandardError", result.ControllerName);
    }

    [TestMethod]
    public async Task Index_WhenOnlyTestRunIsAvailable_SetsDisplayTestRun()
    {
        // Arrange
        var testRunOnlyResponse = BuildClassificationResponse(
            [
                new CalculatorRunClassificationDto
                {
                    Id = (int)RunClassification.TEST_RUN,
                    Status = "TEST RUN",
                    Description = string.Empty
                }
            ],
            []);
        SetupClassificationApiSequence(testRunOnlyResponse);
        apiService.Setup(service => service.GetCalculatorRun(RunId)).ReturnsAsync(BuildRun(RunId));
        var controller = BuildController();

        // Act
        var result = await controller.Index(RunId) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        var model = result.Model as SetRunClassificationViewModel;
        Assert.IsNotNull(model);
        Assert.IsTrue(model.ImportantViewModel.IsDisplayTestRun);
        Assert.IsFalse(model.ImportantViewModel.IsAnyRunInProgress);
    }

    [TestMethod]
    public async Task Submit_WhenModelStateIsInvalid_ReturnsIndexViewWithRehydratedModel()
    {
        // Arrange
        var model = BuildSubmitModel();
        SetupClassificationApiSequence(BuildClassificationResponse());
        apiService.Setup(service => service.GetCalculatorRun(RunId)).ReturnsAsync(BuildRun(RunId));
        var controller = BuildController();
        controller.ModelState.AddModelError(nameof(SetRunClassificationFormModel.ClassifyRunType), "Required");

        // Act
        var result = await controller.Submit(model) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.SetRunClassificationIndex, result.ViewName);
        var returnedModel = result.Model as SetRunClassificationViewModel;
        Assert.IsNotNull(returnedModel);
        Assert.AreEqual(RunId, returnedModel.RunId);
        apiService.Verify(service => service.CallApi(
                HttpMethod.Put,
                "v2/calculatorRuns",
                It.IsAny<IDictionary<string, string?>?>(),
                It.IsAny<object?>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Submit_WhenApiReturnsCreated_RedirectsToClassifyRunConfirmation()
    {
        // Arrange
        var model = BuildSubmitModel();
        ClassificationDto? capturedBody = null;
        apiService
            .Setup(service => service.CallApi(
                HttpMethod.Put,
                "v2/calculatorRuns",
                It.IsAny<IDictionary<string, string?>?>(),
                It.IsAny<object?>()))
            .Callback<HttpMethod, string, IDictionary<string, string?>?, object?>((_, _, _, body) => { capturedBody = body as ClassificationDto; })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Created));
        var controller = BuildController();

        // Act
        var result = await controller.Submit(model) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ActionNames.Index, result.ActionName);
        Assert.AreEqual(ControllerNames.ClassifyRunConfirmation, result.ControllerName);
        Assert.AreEqual(RunId, result.RouteValues!["runId"]);
        Assert.IsNotNull(capturedBody);
        Assert.AreEqual(RunId, capturedBody.RunId);
        Assert.AreEqual((int)RunClassification.INITIAL_RUN, capturedBody.ClassificationId);
    }

    [TestMethod]
    public async Task Submit_WhenApiReturnsNonCreated_RedirectsToStandardError()
    {
        // Arrange
        var model = BuildSubmitModel();
        apiService
            .Setup(service => service.CallApi(
                HttpMethod.Put,
                "v2/calculatorRuns",
                It.IsAny<IDictionary<string, string?>?>(),
                It.IsAny<object?>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));
        var controller = BuildController();

        // Act
        var result = await controller.Submit(model) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ActionName);
        Assert.AreEqual("StandardError", result.ControllerName);
    }

    private SetRunClassificationController BuildController()
    {
        var controller = new SetRunClassificationController(apiService.Object, logger.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        return controller;
    }

    private void SetupClassificationApiSequence(params RelativeYearClassificationResponseDto?[] responses)
    {
        var sequence = apiService
            .Setup(service => service.Get<RelativeYearClassificationResponseDto>("v1/ClassificationByRelativeYear", It.IsAny<IDictionary<string, string?>?>()));

        foreach (var response in responses)
            sequence.ReturnsAsync(response);
    }

    private static SetRunClassificationFormModel BuildSubmitModel()
    {
        return new SetRunClassificationFormModel
        {
            RunId = RunId,
            ClassifyRunType = (int)RunClassification.INITIAL_RUN
        };
    }

    private static CalculatorRunDto BuildRun(int runId, RunClassification classification = RunClassification.UNCLASSIFIED)
    {
        return new CalculatorRunDto
        {
            RunId = runId,
            RunName = $"Run {runId}",
            RunClassification = classification,
            RelativeYear = new RelativeYear(RelativeYearValue),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "Test User",
            BillingRunStatus = BillingRunStatus.None
        };
    }

    private static RelativeYearClassificationResponseDto BuildClassificationResponse(
        IEnumerable<CalculatorRunClassificationDto>? classifications = null,
        IEnumerable<CalculatorRunDto>? classifiedRuns = null)
    {
        return new RelativeYearClassificationResponseDto
        {
            RelativeYear = new RelativeYear(RelativeYearValue),
            Classifications = classifications?.ToList()
                              ??
                              [
                                  new CalculatorRunClassificationDto
                                  {
                                      Id = (int)RunClassification.TEST_RUN,
                                      Status = "TEST RUN",
                                      Description = string.Empty
                                  }
                              ],
            ClassifiedRuns = classifiedRuns?.ToList() ?? []
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
