using System.Net;
using System.Security.Claims;
using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests.Controllers;

[TestClass]
public class AcceptRejectConfirmationControllerTests
{
    private Mock<IEprCalculatorApiService> apiService = null!;
    private Fixture fixture = null!;
    private DefaultHttpContext httpContext = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        fixture = new Fixture();
        apiService = new Mock<IEprCalculatorApiService>();
        httpContext = new DefaultHttpContext
        {
            Session = new MockHttpSession(),
            User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, "Test User")
            ]))
        };
    }

    [TestMethod]
    public async Task IndexAsync_InvalidCalculationRunId_RedirectsToStandardError()
    {
        // Arrange
        var controller = BuildController();

        // Act
        var result = await controller.IndexAsync(0) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ActionName);
        Assert.AreEqual("StandardError", result.ControllerName);
    }

    [TestMethod]
    public async Task IndexAsync_RunNotFound_RedirectsToStandardError()
    {
        // Arrange
        var runId = Math.Abs(fixture.Create<int>()) + 1;
        apiService.Setup(service => service.GetCalculatorRun(runId)).ReturnsAsync((CalculatorRunDto?)null);
        var controller = BuildController();

        // Act
        var result = await controller.IndexAsync(runId) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ActionName);
        Assert.AreEqual("StandardError", result.ControllerName);
    }

    [TestMethod]
    public async Task IndexAsync_ValidRunId_ReturnsViewWithExpectedModel()
    {
        // Arrange
        var runId = Math.Abs(fixture.Create<int>()) + 1;
        var runName = fixture.Create<string>();
        apiService.Setup(service => service.GetCalculatorRun(runId))
            .ReturnsAsync(new CalculatorRunDto { RunId = runId, RunName = runName });
        var controller = BuildController();

        // Act
        var result = await controller.IndexAsync(runId) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.AcceptRejectConfirmationIndex, result.ViewName);

        var model = result.Model as AcceptRejectConfirmationViewModel;
        Assert.IsNotNull(model);
        Assert.AreEqual(runId, model.RunId);
        Assert.AreEqual(runName, model.RunName);
        Assert.AreEqual(BillingStatus.Accepted, model.Status);
    }

    [TestMethod]
    public async Task Submit_InvalidModelStateWithApproveDataError_ReturnsViewAndAddsSummaryError()
    {
        // Arrange
        var runId = Math.Abs(fixture.Create<int>()) + 1;
        var runName = fixture.Create<string>();
        apiService.Setup(service => service.GetCalculatorRun(runId))
            .ReturnsAsync(new CalculatorRunDto { RunId = runId, RunName = runName });
        var controller = BuildController();
        var model = BuildModel(runId, approveData: null);
        controller.ModelState.AddModelError(nameof(model.ApproveData), "ApproveData is required");

        // Act
        var result = await controller.Submit(model) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.AcceptRejectConfirmationIndex, result.ViewName);

        var resultModel = result.Model as AcceptRejectConfirmationViewModel;
        Assert.IsNotNull(resultModel);
        Assert.AreEqual(runId, resultModel.RunId);
        Assert.AreEqual(runName, resultModel.RunName);
        Assert.IsTrue(controller.ModelState.ContainsKey($"Summary_{nameof(model.ApproveData)}"));
        Assert.AreEqual(
            ErrorMessages.AcceptRejectConfirmationApproveDataRequiredSummary,
            controller.ModelState[$"Summary_{nameof(model.ApproveData)}"]!.Errors.First().ErrorMessage);
        apiService.Verify(service => service.CallApi(
            It.IsAny<HttpMethod>(),
            It.IsAny<string>(),
            It.IsAny<IDictionary<string, string?>>(),
            It.IsAny<object>()), Times.Never);
    }

    [TestMethod]
    public async Task Submit_InvalidModelStateWithRejectedStatus_PreservesStatusAndReasonOnRedisplay()
    {
        // Arrange
        var runId = Math.Abs(fixture.Create<int>()) + 1;
        var runName = fixture.Create<string>();
        var reason = fixture.Create<string>();
        apiService.Setup(service => service.GetCalculatorRun(runId))
            .ReturnsAsync(new CalculatorRunDto { RunId = runId, RunName = runName });
        var controller = BuildController();
        var model = BuildModel(runId, BillingStatus.Rejected, null, reason);
        controller.ModelState.AddModelError(nameof(model.ApproveData), "ApproveData is required");

        // Act
        var result = await controller.Submit(model) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.AcceptRejectConfirmationIndex, result.ViewName);

        var resultModel = result.Model as AcceptRejectConfirmationViewModel;
        Assert.IsNotNull(resultModel);
        Assert.AreEqual(runId, resultModel.RunId);
        Assert.AreEqual(runName, resultModel.RunName);
        Assert.AreEqual(BillingStatus.Rejected, resultModel.Status);
        Assert.AreEqual(reason, resultModel.Reason);
    }

    [TestMethod]
    public async Task Submit_ApproveDataFalse_RedirectsWithoutCallingApiOrClearingSelectedOrganisations()
    {
        // Arrange
        var runId = Math.Abs(fixture.Create<int>()) + 1;
        var selectedOrganisationIds = new[] { 10, 20, 30 };
        SeedSelectedOrganisations(selectedOrganisationIds);
        var controller = BuildController();
        var model = BuildModel(runId, approveData: false);

        // Act
        var result = await controller.Submit(model) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ActionNames.Index, result.ActionName);
        Assert.AreEqual(ControllerNames.BillingInstructionsController, result.ControllerName);
        Assert.AreEqual(runId, result.RouteValues!["runId"]);
        CollectionAssert.AreEquivalent(
            selectedOrganisationIds,
            ARJourneySessionHelper.GetFromSession(httpContext.Session).ToArray());
        apiService.Verify(service => service.CallApi(
            It.IsAny<HttpMethod>(),
            It.IsAny<string>(),
            It.IsAny<IDictionary<string, string?>>(),
            It.IsAny<object>()), Times.Never);
    }

    [TestMethod]
    public async Task Submit_ApproveDataTrue_CallsApiWithExpectedPayloadAndClearsSession()
    {
        // Arrange
        var runId = Math.Abs(fixture.Create<int>()) + 1;
        var selectedOrganisationIds = new[] { 11, 22, 33 };
        SeedSelectedOrganisations(selectedOrganisationIds);
        var model = BuildModel(runId, BillingStatus.Rejected, true, "Invalid billing data");
        HttpMethod? actualMethod = null;
        string? actualRelativePath = null;
        ProducerBillingInstructionsHttpPutRequestDto? actualBody = null;
        apiService.Setup(service => service.CallApi(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                It.IsAny<IDictionary<string, string?>>(),
                It.IsAny<object>()))
            .Callback<HttpMethod, string, IDictionary<string, string?>?, object?>((method, path, _, body) =>
            {
                actualMethod = method;
                actualRelativePath = path;
                actualBody = body as ProducerBillingInstructionsHttpPutRequestDto;
            })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
        var controller = BuildController();

        // Act
        var result = await controller.Submit(model) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ActionNames.Index, result.ActionName);
        Assert.AreEqual(ControllerNames.BillingInstructionsController, result.ControllerName);
        Assert.AreEqual(runId, result.RouteValues!["runId"]);
        Assert.AreEqual(HttpMethod.Put, actualMethod);
        Assert.AreEqual($"v2/producerBillingInstructions/{runId}", actualRelativePath);
        Assert.IsNotNull(actualBody);
        CollectionAssert.AreEquivalent(selectedOrganisationIds, actualBody.OrganisationIds.ToArray());
        Assert.AreEqual(BillingStatus.Rejected.ToString(), actualBody.Status);
        Assert.AreEqual("Invalid billing data", actualBody.ReasonForRejection);
        Assert.AreEqual(0, ARJourneySessionHelper.GetFromSession(httpContext.Session).Count);
        apiService.Verify(service => service.CallApi(
            It.IsAny<HttpMethod>(),
            It.IsAny<string>(),
            It.IsAny<IDictionary<string, string?>>(),
            It.IsAny<object>()), Times.Once);
    }

    [TestMethod]
    public async Task Submit_ApiFailure_RedirectsToError()
    {
        // Arrange
        var runId = Math.Abs(fixture.Create<int>()) + 1;
        var selectedOrganisationIds = new[] { 100, 200 };
        SeedSelectedOrganisations(selectedOrganisationIds);
        var model = BuildModel(runId, approveData: true);
        apiService.Setup(service => service.CallApi(
                It.IsAny<HttpMethod>(),
                It.IsAny<string>(),
                It.IsAny<IDictionary<string, string?>>(),
                It.IsAny<object>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));
        var controller = BuildController();

        // Act
        var result = await controller.Submit(model) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ActionName);
        Assert.AreEqual("StandardError", result.ControllerName);
    }

    private void SeedSelectedOrganisations(IEnumerable<int> organisationIds)
    {
        ARJourneySessionHelper.AddToSession(httpContext.Session, organisationIds);
    }

    private static AcceptRejectConfirmationFormModel BuildModel(
        int runId,
        BillingStatus status = BillingStatus.Accepted,
        bool? approveData = true,
        string? reason = null)
    {
        return new AcceptRejectConfirmationFormModel
        {
            RunId = runId,
            Status = status,
            ApproveData = approveData,
            Reason = reason
        };
    }

    private AcceptRejectConfirmationController BuildController()
    {
        var controller = new AcceptRejectConfirmationController(apiService.Object, new Mock<ILogger<AcceptRejectConfirmationController>>().Object);
        controller.ControllerContext.HttpContext = httpContext;
        return controller;
    }
}
