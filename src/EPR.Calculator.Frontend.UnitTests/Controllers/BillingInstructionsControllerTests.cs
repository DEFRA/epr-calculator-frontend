namespace EPR.Calculator.Frontend.UnitTests.Controllers
{
    using System;
    using System.Net;
    using System.Security.Claims;
    using System.Text;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using EPR.Calculator.Frontend.Constants;
    using EPR.Calculator.Frontend.Controllers;
    using EPR.Calculator.Frontend.Enums;
    using EPR.Calculator.Frontend.Extensions;
    using EPR.Calculator.Frontend.Helpers;
    using EPR.Calculator.Frontend.Mappers;
    using EPR.Calculator.Frontend.Models;
    using EPR.Calculator.Frontend.Services;
    using EPR.Calculator.Frontend.UnitTests.HelpersTest;
    using EPR.Calculator.Frontend.UnitTests.Mocks;
    using EPR.Calculator.Frontend.ViewModels;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Identity.Web;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Moq.Protected;

    [TestClass]
    public class BillingInstructionsControllerTests
    {
        private readonly IConfiguration _configuration = ConfigurationItems.GetConfigurationValues();
        private readonly Mock<ITokenAcquisition> _mockTokenAcquisition;
        private readonly TelemetryClient _mockTelemetryClient;
        private readonly Mock<IHttpClientFactory> _mockClientFactory;
        private readonly BillingInstructionsController _controller;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<IBillingInstructionsMapper> _mockMapper;

        public BillingInstructionsControllerTests()
        {
            this.Fixture = new Fixture();
            // _mockConfiguration = new Mock<IConfiguration>();
            _mockTokenAcquisition = new Mock<ITokenAcquisition>();
            _mockTelemetryClient = new TelemetryClient();
            _mockClientFactory = new Mock<IHttpClientFactory>();
            _mockMapper = new Mock<IBillingInstructionsMapper>();

            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(context => context.User)
                .Returns(new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Name, "Test User")
                ])));

            _controller = BuildTestClass(
                Fixture,
                HttpStatusCode.OK,
                MockData.GetCalculatorRun(),
                Fixture.Create<CalculatorRunDetailsViewModel>(),
                _configuration);

            var mockSession = new MockHttpSession();
            mockSession.SetString("accessToken", "something");
            mockSession.SetString(SessionConstants.FinancialYear, "2024-25");
            var context = new DefaultHttpContext()
            {
                Session = mockSession
            };

            // Setting the mocked HttpContext for the controller
            _controller.ControllerContext = new ControllerContext { HttpContext = context };
        }

        private Fixture Fixture { get; init; }

        [TestMethod]
        public async Task Index_InvalidCalculationRunId_RedirectsToError()
        {
            // Arrange
            var calculationRunId = -1;
            var request = new PaginationRequestViewModel { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.IndexAsync(calculationRunId, request) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
        }

        [TestMethod]
        public async Task Index_ValidCalculationRunId_ReturnsViewResult()
        {
            // Arrange
            var calculationRunId = 1;
            var request = new PaginationRequestViewModel { Page = 1, PageSize = 10 };

            // Prepare a fake response DTO
            var billingData = CreateDefaultBillingData(calculationRunId);

            var expectedViewModel = CreateDefaultViewModel(calculationRunId, billingData, request);

            SetupMockMapper(expectedViewModel);

            var mockFactory = GetMockHttpClientFactoryWithObjectResponse(billingData);
            var controller = this.BuildTestClass(
                this.Fixture,
                HttpStatusCode.OK,
                billingData);

            // Act
            var result = await controller.IndexAsync(calculationRunId, request) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(BillingInstructionsViewModel));
        }

        [TestMethod]
        public async Task Index_WithValidOrganisationId_FiltersOrganisations()
        {
            // Arrange
            var calculationRunId = 1;
            var request = new PaginationRequestViewModel { Page = 1, PageSize = 10, OrganisationId = 123 };

            // Prepare a fake response DTO
            var billingData = CreateDefaultBillingData(calculationRunId);

            var expectedViewModel = CreateDefaultViewModel(calculationRunId, billingData, request);

            SetupMockMapper(expectedViewModel);

            var controller = BuildTestClass(
                this.Fixture,
                HttpStatusCode.OK,
                billingData);

            // Act
            var result = await controller.IndexAsync(calculationRunId, request) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(BillingInstructionsViewModel));
        }

        [TestMethod]
        public async Task Index_WithInValidOrganisationId_FiltersOrganisations()
        {
            // Arrange
            var calculationRunId = 1;
            var request = new PaginationRequestViewModel
            {
                Page = 1,
                PageSize = 10,
                OrganisationId = 1234 // invalid ID
            };

            // Simulate no records for the invalid OrganisationId
            var billingData = new ProducerBillingInstructionsResponseDto
            {
                Records = new List<ProducerBillingInstructionsDto>(),
                TotalRecords = 0,
                CalculatorRunId = calculationRunId,
                RunName = "Test Run",
                PageNumber = 1,
                PageSize = 10
            };

            var expectedViewModel = CreateDefaultViewModel(calculationRunId, billingData, request);

            SetupMockMapper(expectedViewModel);

            var controller = BuildTestClass(
                this.Fixture,
                HttpStatusCode.OK,
                billingData,
                null,
                this._configuration);

            // Act
            var result = await controller.IndexAsync(calculationRunId, request) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(BillingInstructionsViewModel));

            var model = result.Model as BillingInstructionsViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(0, model.TablePaginationModel.TotalTableRecords);
        }

        [TestMethod]
        public void ProcessSelection_RedirectsToIndex()
        {
            // Arrange
            var calculationRunId = 1;
            var selections = new OrganisationSelectionsViewModel
            {
                SelectedOrganisationIds = new List<int> { 1, 2, 3 }
            };

            // Act
            var result = _controller.ProcessSelection(calculationRunId, selections) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual(calculationRunId, result.RouteValues[BillingInstructionConstants.CalculationRunIdKey]);
        }

        [TestMethod]
        public async Task IndexAsync_ApiCallFails_RedirectsToError()
        {
            // Arrange
            var calculationRunId = 1;
            var request = new PaginationRequestViewModel { Page = 1, PageSize = 10 };

            // Use the helper to simulate a failed API response
            var mockFactory = GetMockHttpClientFactoryWithObjectResponse(null, HttpStatusCode.InternalServerError);
            var controller = CreateControllerWithFactory(mockFactory);

            // Act
            var result = await controller.IndexAsync(calculationRunId, request) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
        }

        [TestMethod]
        public async Task IndexAsync_ApiReturnsNull_RedirectsToError()
        {
            // Arrange
            var calculationRunId = 1;
            var request = new PaginationRequestViewModel { Page = 1, PageSize = 10 };

            // Simulate API returns empty/invalid JSON (so deserialization returns null)
            var mockFactory = GetMockHttpClientFactoryWithObjectResponse(null, HttpStatusCode.OK);
            var controller = CreateControllerWithFactory(mockFactory);

            // Act
            var result = await controller.IndexAsync(calculationRunId, request) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
        }

        [TestMethod]
        public async Task IndexAsync_MapperThrowsException_RedirectsToError()
        {
            // Arrange
            var calculationRunId = 1;
            var request = new PaginationRequestViewModel { Page = 1, PageSize = 10 };
            var billingData = CreateDefaultBillingData(calculationRunId);

            // Mapper throws exception
            _mockMapper.Setup(m => m.MapToViewModel(It.IsAny<ProducerBillingInstructionsResponseDto>(), It.IsAny<PaginationRequestViewModel>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Throws(new Exception("Mapper failed"));

            // Use the helper to simulate a successful API response
            var mockFactory = GetMockHttpClientFactoryWithObjectResponse(billingData);
            var controller = CreateControllerWithFactory(mockFactory);

            // Act
            var result = await controller.IndexAsync(calculationRunId, request) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
        }

        [TestMethod]
        public async Task IndexAsync_MapperCalledWithCorrectArguments()
        {
            // Arrange
            var calculationRunId = 1;
            var request = new PaginationRequestViewModel { Page = 1, PageSize = 10 };
            var billingData = CreateDefaultBillingData(calculationRunId);

            // Setup the mapper to return any view model
            _mockMapper.Setup(m => m.MapToViewModel(
                    It.IsAny<ProducerBillingInstructionsResponseDto>(),
                    It.IsAny<PaginationRequestViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(new BillingInstructionsViewModel());

            var mockFactory = GetMockHttpClientFactoryWithObjectResponse(billingData);
            var controller = BuildTestClass(
                this.Fixture,
                HttpStatusCode.OK,
                billingData,
                null,
                this._configuration);

            // Act
            await controller.IndexAsync(calculationRunId, request);

            // Assert
            _mockMapper.Verify(
                m => m.MapToViewModel(
                    It.IsAny<ProducerBillingInstructionsResponseDto>(),
                    It.IsAny<PaginationRequestViewModel>(),
                    "Test User",
                    It.IsAny<bool>(),
                    It.IsAny<bool>()),
                Times.Once);

            _mockMapper.Verify(
                m => m.MapToViewModel(
                    It.Is<ProducerBillingInstructionsResponseDto>(dto =>
                        dto.CalculatorRunId == billingData.CalculatorRunId &&
                        dto.TotalRecords == billingData.TotalRecords &&
                        dto.RunName == billingData.RunName &&
                        dto.PageNumber == billingData.PageNumber &&
                        dto.PageSize == billingData.PageSize &&
                        dto.Records.Count == billingData.Records.Count &&
                        dto.Records[0].ProducerId == billingData.Records[0].ProducerId),
                    It.Is<PaginationRequestViewModel>(r =>
                        r.Page == request.Page && r.PageSize == request.PageSize),
                    "Test User", false, false),
                Times.Once);
        }

        [TestMethod]
        public async Task IndexAsync_PaginationEdgeCases_HandledGracefully()
        {
            // Arrange
            var calculationRunId = 1;
            var request = new PaginationRequestViewModel { Page = 0, PageSize = int.MaxValue };

            // Simulate API returns 400 Bad Request with a message
            var errorMessage = "Page Number must be higher than 0";
            var mockFactory = GetMockHttpClientFactoryWithObjectResponse(
                responseObject: errorMessage,
                code: HttpStatusCode.BadRequest);

            var controller = CreateControllerWithFactory(mockFactory);

            // Act
            var result = await controller.IndexAsync(calculationRunId, request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual(ActionNames.StandardErrorIndex, redirectResult.ActionName);
        }

        [TestMethod]
        public async Task Index_ValidCalculationRunId_EmptySession()
        {
            // Arrange
            var calculationRunId = 1;
            var request = new PaginationRequestViewModel { Page = 1, PageSize = 10 };
            var billingData = CreateDefaultBillingData(calculationRunId);

            // Setup the mapper to return any view model
            _mockMapper.Setup(m => m.MapToViewModel(
                    It.IsAny<ProducerBillingInstructionsResponseDto>(),
                    It.IsAny<PaginationRequestViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(new BillingInstructionsViewModel());

            var controller = BuildTestClass(this.Fixture, HttpStatusCode.OK, billingData);

            // Act
            var result = await controller.IndexAsync(calculationRunId, request) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(BillingInstructionsViewModel));
        }

        [TestMethod]
        public async Task CanCallSelectAll()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var model = fixture.Create<BillingInstructionsViewModel>();
            var currentPage = fixture.Create<int>();
            var pageSize = fixture.Create<int>();

            var calculationRunId = 1;
            var request = new PaginationRequestViewModel { Page = 1, PageSize = 10 };
            var billingData = CreateDefaultBillingData(calculationRunId);

            // Setup the mapper to return any view model
            _mockMapper.Setup(m => m.MapToViewModel(
                    It.IsAny<ProducerBillingInstructionsResponseDto>(),
                    It.IsAny<PaginationRequestViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(new BillingInstructionsViewModel());

            var mockFactory = GetMockHttpClientFactoryWithObjectResponse(billingData);
            var controller = CreateControllerWithFactory(mockFactory);
            // Act
            var result = controller.SelectAll(model, currentPage, pageSize, 0, null, null) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.RouteName.Contains("Index"));
            Assert.AreEqual(model.CalculationRun.Id, result.RouteValues["calculationRunId"]);
        }

        [TestMethod]
        public async Task CanCallSelectAll_FilterAccepted()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var model = fixture.Create<BillingInstructionsViewModel>();
            var currentPage = fixture.Create<int>();
            var pageSize = fixture.Create<int>();

            var calculationRunId = 1;
            var request = new PaginationRequestViewModel { Page = 1, PageSize = 10 };
            var billingData = CreateDefaultBillingData(calculationRunId);

            // Setup the mapper to return any view model
            _mockMapper.Setup(m => m.MapToViewModel(
                    It.IsAny<ProducerBillingInstructionsResponseDto>(),
                    It.IsAny<PaginationRequestViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(new BillingInstructionsViewModel());

            var mockFactory = GetMockHttpClientFactoryWithObjectResponse(billingData);
            var controller = CreateControllerWithFactory(mockFactory);
            // Act
            var result = controller.SelectAll(model, currentPage, pageSize, 0, BillingStatus.Accepted, null) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.RouteName.Contains("Index"));
            Assert.AreEqual(model.CalculationRun.Id, result.RouteValues["calculationRunId"]);
        }

        [TestMethod]
        public async Task CanCallSelectAll_FilterRejected()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var model = fixture.Create<BillingInstructionsViewModel>();
            var currentPage = fixture.Create<int>();
            var pageSize = fixture.Create<int>();

            var calculationRunId = 1;
            var request = new PaginationRequestViewModel { Page = 1, PageSize = 10 };
            var billingData = CreateDefaultBillingData(calculationRunId);

            // Setup the mapper to return any view model
            _mockMapper.Setup(m => m.MapToViewModel(
                    It.IsAny<ProducerBillingInstructionsResponseDto>(),
                    It.IsAny<PaginationRequestViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(new BillingInstructionsViewModel());

            var mockFactory = GetMockHttpClientFactoryWithObjectResponse(billingData);
            var controller = CreateControllerWithFactory(mockFactory);
            // Act
            var result = controller.SelectAll(model, currentPage, pageSize, 0, BillingStatus.Rejected, null) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.RouteName.Contains("Index"));
            Assert.AreEqual(model.CalculationRun.Id, result.RouteValues["calculationRunId"]);
        }

        [TestMethod]
        public async Task CanCallSelectAll_FilterPending()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var model = fixture.Create<BillingInstructionsViewModel>();
            var currentPage = fixture.Create<int>();
            var pageSize = fixture.Create<int>();

            var calculationRunId = 1;
            var request = new PaginationRequestViewModel { Page = 1, PageSize = 10 };
            var billingData = CreateDefaultBillingData(calculationRunId);

            // Setup the mapper to return any view model
            _mockMapper.Setup(m => m.MapToViewModel(
                    It.IsAny<ProducerBillingInstructionsResponseDto>(),
                    It.IsAny<PaginationRequestViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(new BillingInstructionsViewModel());

            var mockFactory = GetMockHttpClientFactoryWithObjectResponse(billingData);
            var controller = CreateControllerWithFactory(mockFactory);
            // Act
            var result = controller.SelectAll(model, currentPage, pageSize, 0, BillingStatus.Pending, null) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.RouteName.Contains("Index"));
            Assert.AreEqual(model.CalculationRun.Id, result.RouteValues["calculationRunId"]);
        }

        [TestMethod]
        public async Task CanSelectAll_WhenOrgIdIsPresent()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var model = fixture.Create<BillingInstructionsViewModel>();
            var currentPage = fixture.Create<int>();
            var pageSize = fixture.Create<int>();
            var organisationId = fixture.Create<int>();

            var calculationRunId = 1;
            var request = new PaginationRequestViewModel { Page = 1, PageSize = 10 };
            var billingData = CreateDefaultBillingData(calculationRunId);

            // Setup the mapper to return any view model
            _mockMapper.Setup(m => m.MapToViewModel(
                    It.IsAny<ProducerBillingInstructionsResponseDto>(),
                    It.IsAny<PaginationRequestViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(new BillingInstructionsViewModel());

            var mockFactory = GetMockHttpClientFactoryWithObjectResponse(billingData);
            var controller = CreateControllerWithFactory(mockFactory);

            // Act
            var result = controller.SelectAll(model, currentPage, pageSize, organisationId, null, null) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.RouteName.Contains("Index"));
            Assert.AreEqual(model.CalculationRun.Id, result.RouteValues["calculationRunId"]);
        }

        [TestMethod]
        public async Task CanCallSelectAll_Page()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var model = fixture.Create<BillingInstructionsViewModel>();
            var currentPage = fixture.Create<int>();
            var pageSize = fixture.Create<int>();
            var organisationId = fixture.Create<int>();

            var calculationRunId = 1;
            var request = new PaginationRequestViewModel { Page = 1, PageSize = 10 };
            var billingData = CreateDefaultBillingData(calculationRunId);

            var mockSession = new MockHttpSession();
            mockSession.SetString("IsRedirected", "true");

            var context = new DefaultHttpContext()
            {
                Session = mockSession
            };

            // Setup the mapper to return any view model
            _mockMapper.Setup(m => m.MapToViewModel(
                    It.IsAny<ProducerBillingInstructionsResponseDto>(),
                    It.IsAny<PaginationRequestViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(new BillingInstructionsViewModel());

            var mockFactory = GetMockHttpClientFactoryWithObjectResponse(billingData);
            var controller = CreateControllerWithFactory(mockFactory);

            controller.ControllerContext = new ControllerContext { HttpContext = context };

            // Act
            var result = controller.SelectAll(model, currentPage, pageSize, organisationId, null, null) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.RouteName.Contains("Index"));
            Assert.AreEqual(model.CalculationRun.Id, result.RouteValues["calculationRunId"]);
        }

        [TestMethod]
        public async Task SelectAllPage_SetsSessionAndReturnsViewWithSelectAllViewModel()
        {
            // Arrange
            var calculationRunId = 1;
            var request = new PaginationRequestViewModel { Page = 1, PageSize = 10 };
            var billingData = CreateDefaultBillingData(calculationRunId);

            // Setup the mapper to return any view model
            _mockMapper.Setup(m => m.MapToViewModel(
                    It.IsAny<ProducerBillingInstructionsResponseDto>(),
                    It.IsAny<PaginationRequestViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(new BillingInstructionsViewModel()
                { OrganisationBillingInstructions = new List<Organisation>() { new Organisation { Id = 1, BillingInstruction = Enums.BillingInstruction.Initial, IsSelected = true, InvoiceAmount = 100, OrganisationId = 1, OrganisationName = "Test", Status = Enums.BillingStatus.Accepted } } });

            var controller = BuildTestClass(this.Fixture, HttpStatusCode.OK, billingData);

            // Act
            var result = await controller.IndexAsync(calculationRunId, request);

            // Assert
            Assert.IsNotNull(result);
            var redirectResult = (ViewResult)result;
            Assert.IsNotNull(redirectResult);

            var model = redirectResult.Model as BillingInstructionsViewModel;

            Assert.IsFalse(model.OrganisationSelections.SelectAll);
            Assert.IsFalse(model.OrganisationSelections.SelectPage);
            Assert.IsTrue(model.OrganisationBillingInstructions.First().IsSelected);
            Assert.AreEqual(1, model.OrganisationBillingInstructions.First().OrganisationId);
        }

        [TestMethod]
        public async Task CanCallSelectAllPage()
        {
            // Arrange
            var model = this.Fixture.Create<BillingInstructionsViewModel>();
            var currentPage = this.Fixture.Create<int>();
            var pageSize = this.Fixture.Create<int>();
            var calculationRunId = 1;
            var billingData = CreateDefaultBillingData(calculationRunId);
            var controller = BuildTestClass(this.Fixture, HttpStatusCode.OK, billingData);

            // Act
            var result = await controller.SelectAllPage(model, currentPage, pageSize, 0, null, null) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.RouteName.Contains("Index"));
            Assert.AreEqual(model.CalculationRun.Id, result.RouteValues["calculationRunId"]);
        }

        [TestMethod]
        public async Task CanCallSelectAllPage_WhenOrgIdIsPresent()
        {
            // Arrange
            var model = this.Fixture.Create<BillingInstructionsViewModel>();
            var currentPage = this.Fixture.Create<int>();
            var pageSize = this.Fixture.Create<int>();
            var organisationId = this.Fixture.Create<int>();
            var calculationRunId = 1;

            // Setup the mapper to return any view model
            _mockMapper.Setup(m => m.MapToViewModel(
                    It.IsAny<ProducerBillingInstructionsResponseDto>(),
                    It.IsAny<PaginationRequestViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(new BillingInstructionsViewModel());

            var billingData = CreateDefaultBillingData(calculationRunId);
            var controller = BuildTestClass(this.Fixture, HttpStatusCode.OK, billingData);

            // Act
            var result = await controller.SelectAllPage(model, currentPage, pageSize, organisationId, null, null) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.RouteName.Contains("Index"));
            Assert.AreEqual(model.CalculationRun.Id, result.RouteValues["calculationRunId"]);
        }

        [TestMethod]
        public async Task CanCallSelectAllPage_FilterPending()
        {
            // Arrange
            var model = this.Fixture.Create<BillingInstructionsViewModel>();
            var currentPage = this.Fixture.Create<int>();
            var pageSize = this.Fixture.Create<int>();
            var organisationId = this.Fixture.Create<int>();
            var calculationRunId = 1;

            // Setup the mapper to return any view model
            _mockMapper.Setup(m => m.MapToViewModel(
                    It.IsAny<ProducerBillingInstructionsResponseDto>(),
                    It.IsAny<PaginationRequestViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(new BillingInstructionsViewModel());

            var billingData = CreateDefaultBillingData(calculationRunId);
            var controller = BuildTestClass(this.Fixture, HttpStatusCode.OK, billingData);

            // Act
            var result = await controller.SelectAllPage(model, currentPage, pageSize, 0, BillingStatus.Pending, null) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.RouteName.Contains("Index"));
            Assert.AreEqual(model.CalculationRun.Id, result.RouteValues["calculationRunId"]);
        }

        [TestMethod]
        public async Task CanCallSelectAllPage_FilterAccepted()
        {
            // Arrange
            var model = this.Fixture.Create<BillingInstructionsViewModel>();
            var currentPage = this.Fixture.Create<int>();
            var pageSize = this.Fixture.Create<int>();
            var organisationId = this.Fixture.Create<int>();
            var calculationRunId = 1;

            // Setup the mapper to return any view model
            _mockMapper.Setup(m => m.MapToViewModel(
                    It.IsAny<ProducerBillingInstructionsResponseDto>(),
                    It.IsAny<PaginationRequestViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(new BillingInstructionsViewModel());

            var billingData = CreateDefaultBillingData(calculationRunId);
            var controller = BuildTestClass(this.Fixture, HttpStatusCode.OK, billingData);

            // Act
            var result = await controller.SelectAllPage(model, currentPage, pageSize, 0, BillingStatus.Accepted, null) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.RouteName.Contains("Index"));
            Assert.AreEqual(model.CalculationRun.Id, result.RouteValues["calculationRunId"]);
        }

        [TestMethod]
        public async Task CanCallSelectAllPage_FilterRejected()
        {
            // Arrange
            var model = this.Fixture.Create<BillingInstructionsViewModel>();
            var currentPage = this.Fixture.Create<int>();
            var pageSize = this.Fixture.Create<int>();
            var organisationId = this.Fixture.Create<int>();
            var calculationRunId = 1;

            // Setup the mapper to return any view model
            _mockMapper.Setup(m => m.MapToViewModel(
                    It.IsAny<ProducerBillingInstructionsResponseDto>(),
                    It.IsAny<PaginationRequestViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(new BillingInstructionsViewModel());

            var billingData = CreateDefaultBillingData(calculationRunId);
            var controller = BuildTestClass(this.Fixture, HttpStatusCode.OK, billingData);

            // Act
            var result = await controller.SelectAllPage(model, currentPage, pageSize, 0, BillingStatus.Rejected, null) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.RouteName.Contains("Index"));
            Assert.AreEqual(model.CalculationRun.Id, result.RouteValues["calculationRunId"]);
        }

        [TestMethod]
        public async Task CanCallSelectAllPage_UnChecked()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var model = fixture.Create<BillingInstructionsViewModel>();
            var currentPage = fixture.Create<int>();
            var pageSize = fixture.Create<int>();
            model.OrganisationSelections.SelectPage = false;

            var calculationRunId = 1;
            var request = new PaginationRequestViewModel { Page = 1, PageSize = 10 };
            var billingData = CreateDefaultBillingData(calculationRunId);

            // Setup the mapper to return any view model
            _mockMapper.Setup(m => m.MapToViewModel(
                    It.IsAny<ProducerBillingInstructionsResponseDto>(),
                    It.IsAny<PaginationRequestViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(new BillingInstructionsViewModel());

            var mockFactory = GetMockHttpClientFactoryWithObjectResponse(billingData);
            var controller = BuildTestClass(this.Fixture, HttpStatusCode.OK, billingData);

            // Act
            var result = await controller.SelectAllPage(model, currentPage, pageSize, 0, null, null) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.RouteName.Contains("Index"));
            Assert.AreEqual(model.CalculationRun.Id, result.RouteValues["calculationRunId"]);
        }

        [TestMethod]
        public void CanCallSelectAll_FilterInitialInstruction()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var model = fixture.Create<BillingInstructionsViewModel>();
            var currentPage = fixture.Create<int>();
            var pageSize = fixture.Create<int>();

            var calculationRunId = 1;
            var billingData = CreateDefaultBillingData(calculationRunId);

            _mockMapper.Setup(m => m.MapToViewModel(
                    It.IsAny<ProducerBillingInstructionsResponseDto>(),
                    It.IsAny<PaginationRequestViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(new BillingInstructionsViewModel());

            var mockFactory = GetMockHttpClientFactoryWithObjectResponse(billingData);
            var controller = CreateControllerWithFactory(mockFactory);

            // Act
            var result = controller.SelectAll(model, currentPage, pageSize, 0, null, BillingInstruction.Initial) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.RouteName.Contains("Index"));
            Assert.AreEqual(model.CalculationRun.Id, result.RouteValues["calculationRunId"]);
            Assert.AreEqual(BillingInstruction.Initial, result.RouteValues["BillingInstruction"]);
            Assert.IsNull(result.RouteValues["BillingStatus"]);
        }

        [TestMethod]
        public void CanCallSelectAll_FilterDeltaInstruction()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var model = fixture.Create<BillingInstructionsViewModel>();
            var currentPage = fixture.Create<int>();
            var pageSize = fixture.Create<int>();

            var calculationRunId = 1;
            var billingData = CreateDefaultBillingData(calculationRunId);

            _mockMapper.Setup(m => m.MapToViewModel(
                    It.IsAny<ProducerBillingInstructionsResponseDto>(),
                    It.IsAny<PaginationRequestViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(new BillingInstructionsViewModel());

            var mockFactory = GetMockHttpClientFactoryWithObjectResponse(billingData);
            var controller = CreateControllerWithFactory(mockFactory);

            var result = controller.SelectAll(model, currentPage, pageSize, 0, null, BillingInstruction.Delta) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.RouteName.Contains("Index"));
            Assert.AreEqual(model.CalculationRun.Id, result.RouteValues["calculationRunId"]);
            Assert.AreEqual(BillingInstruction.Delta, result.RouteValues["BillingInstruction"]);
            Assert.IsNull(result.RouteValues["BillingStatus"]);
        }

        [TestMethod]
        public void CanCallSelectAll_FilterRebillInstruction()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var model = fixture.Create<BillingInstructionsViewModel>();
            var currentPage = fixture.Create<int>();
            var pageSize = fixture.Create<int>();

            var calculationRunId = 1;
            var billingData = CreateDefaultBillingData(calculationRunId);

            _mockMapper.Setup(m => m.MapToViewModel(
                    It.IsAny<ProducerBillingInstructionsResponseDto>(),
                    It.IsAny<PaginationRequestViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(new BillingInstructionsViewModel());

            var mockFactory = GetMockHttpClientFactoryWithObjectResponse(billingData);
            var controller = CreateControllerWithFactory(mockFactory);

            var result = controller.SelectAll(model, currentPage, pageSize, 0, null, BillingInstruction.Rebill) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.RouteName.Contains("Index"));
            Assert.AreEqual(model.CalculationRun.Id, result.RouteValues["calculationRunId"]);
            Assert.AreEqual(BillingInstruction.Rebill, result.RouteValues["BillingInstruction"]);
            Assert.IsNull(result.RouteValues["BillingStatus"]);
        }

        [TestMethod]
        public void CanCallSelectAll_FilterCancelBillInstruction()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var model = fixture.Create<BillingInstructionsViewModel>();
            var currentPage = fixture.Create<int>();
            var pageSize = fixture.Create<int>();

            var calculationRunId = 1;
            var billingData = CreateDefaultBillingData(calculationRunId);

            _mockMapper.Setup(m => m.MapToViewModel(
                    It.IsAny<ProducerBillingInstructionsResponseDto>(),
                    It.IsAny<PaginationRequestViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(new BillingInstructionsViewModel());

            var mockFactory = GetMockHttpClientFactoryWithObjectResponse(billingData);
            var controller = CreateControllerWithFactory(mockFactory);

            var result = controller.SelectAll(model, currentPage, pageSize, 0, null, BillingInstruction.Cancel) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.RouteName.Contains("Index"));
            Assert.AreEqual(model.CalculationRun.Id, result.RouteValues["calculationRunId"]);
            Assert.AreEqual(BillingInstruction.Cancel, result.RouteValues["BillingInstruction"]);
            Assert.IsNull(result.RouteValues["BillingStatus"]);
        }

        [TestMethod]
        public void CanCallSelectAll_FilterNoActionInstruction()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var model = fixture.Create<BillingInstructionsViewModel>();
            var currentPage = fixture.Create<int>();
            var pageSize = fixture.Create<int>();

            var calculationRunId = 1;
            var billingData = CreateDefaultBillingData(calculationRunId);

            _mockMapper.Setup(m => m.MapToViewModel(
                    It.IsAny<ProducerBillingInstructionsResponseDto>(),
                    It.IsAny<PaginationRequestViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(new BillingInstructionsViewModel());

            var mockFactory = GetMockHttpClientFactoryWithObjectResponse(billingData);
            var controller = CreateControllerWithFactory(mockFactory);

            var result = controller.SelectAll(model, currentPage, pageSize, 0, null, BillingInstruction.Noaction) as RedirectToRouteResult;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.RouteName.Contains("Index"));
            Assert.AreEqual(model.CalculationRun.Id, result.RouteValues["calculationRunId"]);
            Assert.AreEqual(BillingInstruction.Noaction, result.RouteValues["BillingInstruction"]);
            Assert.IsNull(result.RouteValues["BillingStatus"]);
        }

        [TestMethod]
        public void UpdateOrganisationSelectionAsync_WhenIsSelectedTrue_AddsToSessionAndReturnsOk()
        {
            // Arrange
            var controller = _controller;
            var orgDto = new BillingSelectionOrgDto { Id = 42, IsSelected = true };
            var mockSession = new MockHttpSession();
            var context = new DefaultHttpContext { Session = mockSession };
            controller.ControllerContext = new ControllerContext { HttpContext = context };

            // Act
            var result = controller.UpdateOrganisationSelectionAsync(orgDto) as OkResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
            var stored = mockSession.GetObject<IEnumerable<int>>(EPR.Calculator.Frontend.Constants.SessionConstants.ProducerIds);
            Assert.IsTrue(stored != null && stored.Contains(42));
        }

        [TestMethod]
        public void UpdateOrganisationSelectionAsync_WhenIsSelectedFalse_RemovesFromSessionAndUnsetsSelectAll()
        {
            // Arrange
            var controller = _controller;
            var orgDto = new BillingSelectionOrgDto { Id = 99, IsSelected = false };
            var mockSession = new MockHttpSession();
            // Pre-populate session with the ID
            mockSession.SetObject(EPR.Calculator.Frontend.Constants.SessionConstants.ProducerIds, new List<int> { 99 });
            mockSession.SetString(EPR.Calculator.Frontend.Constants.SessionConstants.IsSelectAll, "true");
            var context = new DefaultHttpContext { Session = mockSession };
            controller.ControllerContext = new ControllerContext { HttpContext = context };

            // Act
            var result = controller.UpdateOrganisationSelectionAsync(orgDto) as OkResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
            var stored = mockSession.GetObject<IEnumerable<int>>(EPR.Calculator.Frontend.Constants.SessionConstants.ProducerIds);
            Assert.IsFalse(stored != null && stored.Contains(99));
            // Check that the select all flag is now false
            Assert.AreEqual("False", mockSession.GetString(EPR.Calculator.Frontend.Constants.SessionConstants.IsSelectAll));
        }

        [TestMethod]
        public async Task IndexAsync_WhenIsSelectAllAndProducerIdsExist_AddsProducerIdsToSession()
        {
            // Arrange
            var calculationRunId = 1;
            var request = new PaginationRequestViewModel { Page = 1, PageSize = 10 };

            // Set up session with IsSelectAll = true
            var mockSession = new MockHttpSession();
            mockSession.SetString(SessionConstants.IsSelectAll, "true");
            mockSession.SetString("accessToken", "something");
            mockSession.SetString(SessionConstants.FinancialYear, "2024-25");
            var context = new DefaultHttpContext { Session = mockSession };
            context.Request.Headers.Append("Referer", "https://calculator/details/4");

            // Prepare a BillingInstructionsViewModel with ProducerIds
            var expectedProducerIds = new List<int> { 101, 102 };
            var expectedViewModel = new BillingInstructionsViewModel
            {
                ProducerIds = expectedProducerIds,
                OrganisationSelections = new OrganisationSelectionsViewModel(),
                OrganisationBillingInstructions = new List<Organisation>(),
                CalculationRun = new CalculationRunForBillingInstructionsDto { Id = calculationRunId, Name = "Test Run" },
                TablePaginationModel = new PaginationViewModel()
            };

            // Setup the mapper to return the expected view model for any parameters
            _mockMapper
                .Setup(m => m.MapToViewModel(
                    It.IsAny<ProducerBillingInstructionsResponseDto>(),
                    It.IsAny<PaginationRequestViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(expectedViewModel);

            var billingData = CreateDefaultBillingData(calculationRunId);
            var mockFactory = GetMockHttpClientFactoryWithObjectResponse(billingData);

            var controller = BuildTestClass(this.Fixture, HttpStatusCode.OK, billingData);
            controller.ControllerContext = new ControllerContext { HttpContext = context };

            // Act
            var result = await controller.IndexAsync(calculationRunId, request);

            // Assert
            var stored = mockSession.GetObject<IEnumerable<int>>(SessionConstants.ProducerIds);
            Assert.IsNotNull(stored, "ProducerIds should be set in session.");
            CollectionAssert.AreEquivalent(expectedProducerIds, stored.ToList());
        }

        [TestMethod]
        public void AcceptSelected_ReturnsRedirectToAcceptRejectConfirmation()
        {
            // Arrange
            int testRunId = 123;

            // Act
            var result = _controller.AcceptSelected(testRunId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(ControllerNames.AcceptRejectConfirmationController, result.ControllerName);
            Assert.AreEqual(testRunId, result.RouteValues["calculationRunId"]);
        }

        [TestMethod]
        public void RejectSelected_ReturnsRedirectToReasonForRejection()
        {
            // Arrange
            int testRunId = 46023;
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;

            // Act
            var result = _controller.RejectSelected(testRunId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(ControllerNames.ReasonForRejectionController, result.ControllerName);
            Assert.AreEqual(testRunId, result.RouteValues["calculationRunId"]);
        }

        [TestMethod]
        public async Task GenerateBillingFile_Returns_Success()
        {
            // Arrange
            int testRunId = 1;

            var mockFactory = GetMockHttpClientFactoryWithObjectResponse(null, HttpStatusCode.OK);

            var controller = BuildTestClass(Fixture, HttpStatusCode.OK, null, null, _configuration);

            // Act
            var result = await controller.GenerateDraftBillingFile(testRunId) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.RouteValues["action"]);
            Assert.AreEqual(ControllerNames.CalculationRunOverview, result.RouteValues["controller"]);
            Assert.AreEqual(testRunId, result.RouteValues["runId"]);
        }

        [TestMethod]
        public async Task GenerateBillingFile_Returns_Failure()
        {
            // Arrange
            int testRunId = 1;

            var controller = BuildTestClass(
                this.Fixture,
                HttpStatusCode.InternalServerError,
                null,
                null,
                this._configuration);

            // Act
            var result = await controller.GenerateDraftBillingFile(testRunId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(CommonUtil.GetControllerName(typeof(StandardErrorController)), result.ControllerName);
        }

        [TestMethod]
        public void ClearSelection_Verifys_ClearsSession()
        {
            // Arrange
            int testRunId = 1;

            // Set up session with IsSelectAll = true
            var mockSession = new MockHttpSession();
            mockSession.SetString("accessToken", "something");
            mockSession.SetString(SessionConstants.FinancialYear, "2024-25");
            var context = new DefaultHttpContext { Session = mockSession };

            // Act
            var result = _controller.ClearSelection(testRunId, 1, 10) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(RouteNames.BillingInstructionsIndex, result.RouteName);
            Assert.AreEqual(testRunId, result.RouteValues["calculationRunId"]);
        }

        [TestMethod]
        public void Index_ReturnsViewResult_WithCorrectViewName()
        {
            // Mocking HttpContext.User.Identity.Name to simulate a logged-in user
            _mockHttpContext.Setup(ctx => ctx.User.Identity.Name).Returns("TestUser");

            // Assert
            var viewResult = _controller.BillingFileSuccess() as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(ViewNames.BillingConfirmationSuccess, viewResult.ViewName);
        }

        [TestMethod]
        public void BillingFileSuccess_ReturnsViewResult_WithCorrectViewModel()
        {
            // Mocking HttpContext.User.Identity.Name to simulate a logged-in user
            _mockHttpContext.Setup(ctx => ctx.User.Identity.Name).Returns("TestUser");

            // Act
            var result = _controller.BillingFileSuccess() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.BillingConfirmationSuccess, result.ViewName);

            var model = result.Model as BillingFileSuccessViewModel;
            Assert.IsNotNull(model);

            var confirmationModel = model.ConfirmationViewModel;
            Assert.IsNotNull(confirmationModel);
            Assert.AreEqual(ConfirmationMessages.BillingFileSuccessTitle, confirmationModel.Title);
            Assert.AreEqual(ConfirmationMessages.BillingFileSuccessBody, confirmationModel.Body);
            CollectionAssert.AreEqual(ConfirmationMessages.BillingFileSuccessAdditionalParagraphs, confirmationModel.AdditionalParagraphs);
            Assert.AreEqual(ControllerNames.Dashboard, confirmationModel.RedirectController);
        }

        private static DefaultHttpContext CreateTestHttpContext(string userName = "Test User")
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, userName) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);
            var prodIds = new List<int>() { 1, 2, 3 };

            var mockSession = new MockHttpSession();
            mockSession.SetString("accessToken", "something");
            mockSession.SetString(SessionConstants.FinancialYear, "2024-25");
            mockSession.SetObject(SessionConstants.ProducerIds, prodIds);
            mockSession.SetObject(SessionConstants.IsRedirected, "true");

            var context = new DefaultHttpContext
            {
                User = principal,
                Session = mockSession
            };

            return context;
        }

        private static Mock<IHttpClientFactory> GetMockHttpClientFactoryWithObjectResponse(object? responseObject, HttpStatusCode code = HttpStatusCode.OK)
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() =>
                {
                    var response = new HttpResponseMessage
                    {
                        StatusCode = code
                    };

                    if (responseObject != null)
                    {
                        var json = System.Text.Json.JsonSerializer.Serialize(responseObject);
                        response.Content = new StringContent(json, Encoding.UTF8, "application/json");
                    }

                    return response;
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            return mockHttpClientFactory;
        }

        private static ProducerBillingInstructionsResponseDto CreateDefaultBillingData(int calculationRunId = 1)
        {
            return new ProducerBillingInstructionsResponseDto
            {
                Records = new List<ProducerBillingInstructionsDto>
                {
                    new ProducerBillingInstructionsDto
                    {
                        ProducerName = "Test Producer",
                        ProducerId = 123,
                        SuggestedBillingInstruction = "Initial",
                        SuggestedInvoiceAmount = 100.0m,
                        BillingInstructionAcceptReject = "Accepted"
                    }
                },
                TotalRecords = 1,
                CalculatorRunId = calculationRunId,
                RunName = "Test Run",
                PageNumber = 1,
                PageSize = 10
            };
        }

        private static BillingInstructionsViewModel CreateDefaultViewModel(int calculationRunId = 1, ProducerBillingInstructionsResponseDto? billingData = null, PaginationRequestViewModel? request = null)
        {
            billingData ??= CreateDefaultBillingData(calculationRunId);
            request ??= new PaginationRequestViewModel { Page = 1, PageSize = 10 };

            return new BillingInstructionsViewModel
            {
                CurrentUser = "Test User",
                CalculationRun = new CalculationRunForBillingInstructionsDto
                {
                    Id = calculationRunId,
                    Name = billingData.RunName ?? "Test Run"
                },
                TablePaginationModel = new PaginationViewModel
                {
                    Records = billingData.Records,
                    CurrentPage = request.Page,
                    PageSize = request.PageSize,
                    TotalTableRecords = billingData.TotalRecords,
                    RouteName = RouteNames.BillingInstructionsIndex,
                    RouteValues = new Dictionary<string, object?>
                    {
                        { BillingInstructionConstants.CalculationRunIdKey, calculationRunId },
                        { BillingInstructionConstants.OrganisationIdKey, request.OrganisationId },
                    }
                }
            };
        }

        private void SetupMockMapper(BillingInstructionsViewModel expectedViewModel)
        {
            _mockMapper
                .Setup(m => m.MapToViewModel(
                    It.IsAny<ProducerBillingInstructionsResponseDto>(),
                    It.IsAny<PaginationRequestViewModel>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(expectedViewModel);
        }

        private BillingInstructionsController CreateControllerWithFactory(Mock<IHttpClientFactory> mockFactory)
        {
            var controller = new BillingInstructionsController(
                _configuration,
                _mockTokenAcquisition.Object,
                _mockTelemetryClient,
                _mockMapper.Object,
                new Mock<IApiService>().Object,
                new Mock<ICalculatorRunDetailsService>().Object);

            controller.ControllerContext = new ControllerContext { HttpContext = CreateTestHttpContext() };
            return controller;
        }

        private BillingInstructionsController BuildTestClass(
            Fixture fixture,
            HttpStatusCode httpStatusCode,
            object data = null,
            CalculatorRunDetailsViewModel details = null,
            IConfiguration configurationItems = null)
        {
            data = data ?? MockData.GetCalculatorRun();
            configurationItems = configurationItems ?? ConfigurationItems.GetConfigurationValues();
            details = details ?? Fixture.Create<CalculatorRunDetailsViewModel>();
            var mockApiService = TestMockUtils.BuildMockApiService(
                httpStatusCode,
                System.Text.Json.JsonSerializer.Serialize(data ?? MockData.GetCalculatorRun())).Object;

            var testClass = new BillingInstructionsController(
                configurationItems,
                new Mock<ITokenAcquisition>().Object,
                _mockTelemetryClient,
                _mockMapper.Object,
                mockApiService,
                TestMockUtils.BuildMockCalculatorRunDetailsService(details).Object);
            testClass.ControllerContext.HttpContext = new DefaultHttpContext
            {
                Session = TestMockUtils.BuildMockSession(fixture).Object,
            };
            testClass.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Name, "Test User")
                ]));
            testClass.ControllerContext.HttpContext.Request.Headers.Append("Referer", "https://calculator/details/4");

            return testClass;
        }
    }
}
