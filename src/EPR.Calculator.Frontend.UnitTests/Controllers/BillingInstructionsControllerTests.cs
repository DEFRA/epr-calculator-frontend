using System.Net;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Extensions;
using EPR.Calculator.Frontend.Mappers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.UnitTests.Controllers;

[TestClass]
public class BillingInstructionsControllerTests
{
    private const int RunId = 1;
    private BillingInstructionsController controller = null!;
    private Mock<IBillingInstructionsMapper> mockMapper = null!;

    private TelemetryClient telemetryClient = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        telemetryClient = new TelemetryClient(new TelemetryConfiguration());
        mockMapper = new Mock<IBillingInstructionsMapper>();
        controller = BuildController();
    }

    [TestMethod]
    public async Task IndexAsync_WhenRunIdIsNotPositive_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var request = new BillingInstructionsIndexModel { Page = 1, PageSize = 10 };

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(() => controller.IndexAsync(-1, request));
    }

    [TestMethod]
    public async Task IndexAsync_WhenApiReturnsNonSuccess_ThrowsHttpRequestExceptionWithDetails()
    {
        // Arrange
        const string errorContent = "API failure response";
        var apiService = TestMockUtils.BuildMockApiService(HttpStatusCode.InternalServerError, errorContent).Object;
        var controllerUnderTest = BuildController(apiService);
        var request = new BillingInstructionsIndexModel { Page = 1, PageSize = 10 };

        // Act
        var act = async () => await controllerUnderTest.IndexAsync(RunId, request);

        // Assert
        var exception = await Assert.ThrowsExceptionAsync<HttpRequestException>(act);
        StringAssert.Contains(exception.Message, $"BillingInstructions API call failed for run {RunId}");
        StringAssert.Contains(exception.Message, "InternalServerError");
        StringAssert.Contains(exception.Message, errorContent);
    }

    [TestMethod]
    public async Task IndexAsync_WhenApiSucceeds_ReturnsViewWithMapperResult()
    {
        // Arrange
        var expectedViewModel = CreateViewModel();
        SetupMapperReturns(expectedViewModel);
        var controllerUnderTest = BuildController(HttpStatusCode.OK, CreateBillingData());
        var request = new BillingInstructionsIndexModel { Page = 1, PageSize = 10 };

        // Act
        var result = await controllerUnderTest.IndexAsync(RunId, request) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreSame(expectedViewModel, result.Model);
    }

    [TestMethod]
    public async Task IndexAsync_MapsApiResponseAndRequestToViewModel()
    {
        // Arrange
        var billingData = CreateBillingData();
        SetupMapperReturns(CreateViewModel());
        var controllerUnderTest = BuildController(HttpStatusCode.OK, billingData);
        var request = new BillingInstructionsIndexModel { Page = 2, PageSize = 25 };

        // Act
        await controllerUnderTest.IndexAsync(RunId, request);

        // Assert
        mockMapper.Verify(
            m => m.MapToViewModel(
                It.Is<ProducerBillingInstructionsResponseDto>(dto =>
                    dto.CalculatorRunId == billingData.CalculatorRunId &&
                    dto.TotalRecords == billingData.TotalRecords &&
                    dto.RunName == billingData.RunName &&
                    dto.Records[0].ProducerId == billingData.Records[0].ProducerId),
                It.Is<BillingInstructionsIndexModel>(r =>
                    r.Page == request.Page && r.PageSize == request.PageSize),
                false,
                false),
            Times.Once);
    }

    [TestMethod]
    public async Task IndexAsync_WhenSelectAllIsSet_AddsMappedProducerIdsToSession()
    {
        // Arrange
        var producerIds = new List<int> { 101, 102 };
        ISession session = new MockHttpSession();
        session.SetString(SessionConstants.IsSelectAll, "true");
        SetupMapperReturns(CreateViewModel(producerIds: producerIds));
        var controllerUnderTest = BuildController(HttpStatusCode.OK, CreateBillingData(), session);
        var request = new BillingInstructionsIndexModel { Page = 1, PageSize = 10 };

        // Act
        await controllerUnderTest.IndexAsync(RunId, request);

        // Assert
        var stored = session.GetObject<IEnumerable<int>>(SessionConstants.ProducerIds);
        Assert.IsNotNull(stored);
        CollectionAssert.AreEquivalent(producerIds, stored.ToList());
    }

    [TestMethod]
    public void ProcessSelection_RedirectsToIndexWithRunId()
    {
        // Arrange
        var selections = new OrganisationSelectionsViewModel();

        // Act
        var result = controller.ProcessSelection(RunId, selections) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ActionNames.Index, result.ActionName);
        Assert.AreEqual(RunId, result.RouteValues!["runId"]);
    }

    [TestMethod]
    public void SelectAll_WhenSelected_SetsSessionFlagAndRedirects()
    {
        // Arrange
        ISession session = new MockHttpSession();
        var controllerUnderTest = BuildController(session: session);
        var model = new BillingInstructionsSelectModel
        {
            RunId = RunId,
            OrganisationSelections = new OrganisationSelectionsViewModel { SelectAll = true }
        };

        // Act
        var result = controllerUnderTest.SelectAll(model) as RedirectToRouteResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(RouteNames.BillingInstructionsIndex, result.RouteName);
        Assert.AreEqual(true.ToString(), session.GetString(SessionConstants.IsSelectAll));
    }

    [TestMethod]
    public void SelectAll_WhenDeselected_ClearsSelectionAndRedirects()
    {
        // Arrange
        ISession session = new MockHttpSession();
        session.SetObject(SessionConstants.ProducerIds, new List<int> { 1, 2, 3 });
        var controllerUnderTest = BuildController(session: session);
        var model = new BillingInstructionsSelectModel
        {
            RunId = RunId,
            OrganisationSelections = new OrganisationSelectionsViewModel { SelectAll = false }
        };

        // Act
        var result = controllerUnderTest.SelectAll(model) as RedirectToRouteResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(RouteNames.BillingInstructionsIndex, result.RouteName);
        Assert.AreEqual("false", session.GetString(SessionConstants.IsSelectAll));
        Assert.IsNull(session.GetObject<IEnumerable<int>>(SessionConstants.ProducerIds));
    }

    [TestMethod]
    public void SelectAll_PreservesFiltersInRedirectRoute()
    {
        // Arrange
        var controllerUnderTest = BuildController();
        var model = new BillingInstructionsSelectModel
        {
            RunId = RunId,
            OrganisationId = 42,
            BillingInstructions = [BillingInstruction.Initial],
            BillingStatuses = [BillingStatus.Rejected],
            OrganisationSelections = new OrganisationSelectionsViewModel { SelectAll = true }
        };

        // Act
        var result = controllerUnderTest.SelectAll(model) as RedirectToRouteResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(42, result.RouteValues![nameof(BillingInstructionsIndexModel.OrganisationId)]);
        CollectionAssert.AreEqual(
            new[] { BillingInstruction.Initial },
            (List<BillingInstruction>)result.RouteValues[nameof(BillingInstructionsIndexModel.BillingInstructions)]!);
        CollectionAssert.AreEqual(
            new[] { BillingStatus.Rejected },
            (List<BillingStatus>)result.RouteValues[nameof(BillingInstructionsIndexModel.BillingStatuses)]!);
    }

    [TestMethod]
    public async Task SelectAllPage_WhenPageSelected_AddsProducerIdsToSessionAndRedirects()
    {
        // Arrange
        ISession session = new MockHttpSession();
        var controllerUnderTest = BuildController(HttpStatusCode.OK, CreateBillingData(), session);
        var model = new BillingInstructionsSelectModel
        {
            RunId = RunId,
            OrganisationSelections = new OrganisationSelectionsViewModel { SelectPage = true }
        };

        // Act
        var result = await controllerUnderTest.SelectAllPage(model) as RedirectToRouteResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(RouteNames.BillingInstructionsIndex, result.RouteName);
        Assert.AreEqual(true.ToString(), session.GetString(SessionConstants.IsSelectAllPage));
        Assert.AreEqual("true", session.GetString(SessionConstants.IsRedirected));

        var stored = session.GetObject<IEnumerable<int>>(SessionConstants.ProducerIds);
        Assert.IsNotNull(stored);
        CollectionAssert.AreEquivalent(new List<int> { 123 }, stored.ToList());
    }

    [TestMethod]
    public async Task SelectAllPage_WhenPageDeselected_ClearsProducerIdsFromSession()
    {
        // Arrange
        ISession session = new MockHttpSession();
        session.SetObject(SessionConstants.ProducerIds, new List<int> { 123, 999 });
        var controllerUnderTest = BuildController(HttpStatusCode.OK, CreateBillingData(), session);
        var model = new BillingInstructionsSelectModel
        {
            RunId = RunId,
            OrganisationSelections = new OrganisationSelectionsViewModel { SelectPage = false }
        };

        // Act
        var result = await controllerUnderTest.SelectAllPage(model) as RedirectToRouteResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("False", session.GetString(SessionConstants.IsSelectAllPage));
        Assert.IsNull(session.GetObject<IEnumerable<int>>(SessionConstants.ProducerIds));
    }

    [TestMethod]
    public void UpdateOrganisationSelection_WhenSelected_AddsToSessionAndReturnsOk()
    {
        // Arrange
        var orgDto = new BillingSelectionOrgDto { Id = 42, IsSelected = true };

        // Act
        var result = controller.UpdateOrganisationSelectionAsync(orgDto) as OkResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
        var stored = controller.HttpContext.Session.GetObject<IEnumerable<int>>(SessionConstants.ProducerIds);
        Assert.IsNotNull(stored);
        Assert.IsTrue(stored.Contains(42));
    }

    [TestMethod]
    public void UpdateOrganisationSelection_WhenDeselected_RemovesFromSessionAndUnsetsSelectAll()
    {
        // Arrange
        var session = controller.HttpContext.Session;
        session.SetObject(SessionConstants.ProducerIds, new List<int> { 99 });
        session.SetString(SessionConstants.IsSelectAll, "true");
        var orgDto = new BillingSelectionOrgDto { Id = 99, IsSelected = false };

        // Act
        var result = controller.UpdateOrganisationSelectionAsync(orgDto) as OkResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
        var stored = session.GetObject<IEnumerable<int>>(SessionConstants.ProducerIds);
        Assert.IsFalse(stored != null && stored.Contains(99));
        Assert.AreEqual("False", session.GetString(SessionConstants.IsSelectAll));
    }

    [TestMethod]
    public void AcceptSelected_RedirectsToAcceptRejectConfirmation()
    {
        // Arrange
        const int testRunId = 123;

        // Act
        var result = controller.AcceptSelected(testRunId) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ActionNames.Index, result.ActionName);
        Assert.AreEqual(ControllerNames.AcceptRejectConfirmationController, result.ControllerName);
        Assert.AreEqual(testRunId, result.RouteValues!["runId"]);
    }

    [TestMethod]
    public void RejectSelected_RedirectsToReasonForRejection()
    {
        // Arrange
        const int testRunId = 46023;

        // Act
        var result = controller.RejectSelected(testRunId) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ActionNames.Index, result.ActionName);
        Assert.AreEqual(ControllerNames.ReasonForRejectionController, result.ControllerName);
        Assert.AreEqual(testRunId, result.RouteValues!["runId"]);
    }

    [TestMethod]
    public async Task GenerateDraftBillingFile_WhenApiSucceeds_RedirectsToCalculationRunOverview()
    {
        // Arrange
        var controllerUnderTest = BuildController();

        // Act
        var result = await controllerUnderTest.GenerateDraftBillingFile(RunId) as RedirectToRouteResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ActionNames.Index, result.RouteValues!["action"]);
        Assert.AreEqual(ControllerNames.CalculationRunOverview, result.RouteValues["controller"]);
        Assert.AreEqual(RunId, result.RouteValues["runId"]);
    }

    [TestMethod]
    public async Task GenerateDraftBillingFile_WhenApiFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var controllerUnderTest = BuildController(HttpStatusCode.InternalServerError);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => controllerUnderTest.GenerateDraftBillingFile(RunId));
    }

    [TestMethod]
    public void ClearSelection_ClearsSessionAndRedirects()
    {
        // Arrange
        ISession session = new MockHttpSession();
        session.SetString(SessionConstants.IsSelectAll, "true");
        session.SetObject(SessionConstants.ProducerIds, new List<int> { 1, 2, 3 });
        var controllerUnderTest = BuildController(session: session);
        var model = new BillingInstructionsClearModel { RunId = RunId, Page = 1, PageSize = 10 };

        // Act
        var result = controllerUnderTest.ClearSelection(model) as RedirectToRouteResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(RouteNames.BillingInstructionsIndex, result.RouteName);
        Assert.AreEqual(RunId, result.RouteValues![nameof(BillingInstructionsClearModel.RunId)]);
        Assert.IsFalse(session.Keys.Contains(SessionConstants.IsSelectAll));
        Assert.IsNull(session.GetObject<IEnumerable<int>>(SessionConstants.ProducerIds));
    }

    [TestMethod]
    public void BillingFileSuccess_ReturnsViewWithConfirmationModel()
    {
        // Act
        var result = controller.BillingFileSuccess() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.BillingConfirmationSuccess, result.ViewName);

        var model = result.Model as BillingFileSuccessViewModel;
        Assert.IsNotNull(model);

        var confirmation = model.ConfirmationViewModel;
        Assert.AreEqual(ConfirmationMessages.BillingFileSuccessTitle, confirmation.Title);
        Assert.AreEqual(ConfirmationMessages.BillingFileSuccessBody, confirmation.Body);
        CollectionAssert.AreEqual(ConfirmationMessages.BillingFileSuccessAdditionalParagraphs, confirmation.AdditionalParagraphs);
        Assert.AreEqual(ControllerNames.Dashboard, confirmation.RedirectController);
    }

    private static ProducerBillingInstructionsResponseDto CreateBillingData(int runId = RunId)
    {
        return new ProducerBillingInstructionsResponseDto
        {
            Records =
            [
                new ProducerBillingInstructionsDto
                {
                    ProducerName = "Test Producer",
                    ProducerId = 123,
                    SuggestedBillingInstruction = "Initial",
                    SuggestedInvoiceAmount = 100.0m,
                    BillingInstructionAcceptReject = "Accepted"
                }
            ],
            TotalRecords = 1,
            CalculatorRunId = runId,
            RunName = "Test Run",
            PageNumber = 1,
            PageSize = 10
        };
    }

    private static BillingInstructionsViewModel CreateViewModel(
        int runId = RunId,
        IEnumerable<int>? producerIds = null,
        ICollection<Organisation>? selectedRows = null,
        int totalRecords = 1)
    {
        return new BillingInstructionsViewModel
        {
            RunId = runId,
            RunName = "Test Run",
            OrganisationId = null,
            BillingInstructions = [],
            BillingStatuses = [],
            InstructionCounts = new Dictionary<BillingInstruction, int>(),
            StatusCounts = new Dictionary<BillingStatus, int>(),
            OrganisationSelections = new OrganisationSelectionsViewModel(),
            SelectedRows = selectedRows ?? new List<Organisation>(),
            TablePaginationModel = new PaginationViewModel { Total = totalRecords },
            ProducerIds = producerIds,
            TotalRecords = totalRecords
        };
    }

    private void SetupMapperReturns(BillingInstructionsViewModel viewModel)
    {
        mockMapper
            .Setup(m => m.MapToViewModel(
                It.IsAny<ProducerBillingInstructionsResponseDto>(),
                It.IsAny<BillingInstructionsIndexModel>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(viewModel);
    }

    private BillingInstructionsController BuildController(
        HttpStatusCode statusCode = HttpStatusCode.OK,
        object? apiResponseData = null,
        ISession? session = null)
    {
        var responseJson = apiResponseData is null ? "{}" : JsonConvert.SerializeObject(apiResponseData);
        var apiService = TestMockUtils.BuildMockApiService(statusCode, responseJson).Object;
        return BuildController(apiService, session);
    }

    private BillingInstructionsController BuildController(IEprCalculatorApiService apiService, ISession? session = null)
    {
        return new BillingInstructionsController(telemetryClient, mockMapper.Object, apiService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Session = session ?? new MockHttpSession() }
            }
        };
    }
}
