using System.Net;
using System.Security.Claims;
using System.Text;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Mappers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Moq;
using Moq.Protected;

namespace EPR.Calculator.Frontend.UnitTests.Controllers
{
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
            _mockTokenAcquisition = new Mock<ITokenAcquisition>();
            _mockTelemetryClient = new TelemetryClient();
            _mockClientFactory = new Mock<IHttpClientFactory>();
            _mockMapper = new Mock<IBillingInstructionsMapper>();

            _controller = new BillingInstructionsController(
                _configuration,
                _mockTokenAcquisition.Object,
                _mockTelemetryClient,
                _mockClientFactory.Object,
                _mockMapper.Object);

            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContext.Setup(context => context.User)
                .Returns(new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Name, "Test User")
                ])));

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
            var controller = CreateControllerWithFactory(mockFactory);

            // Act
            var result = await controller.IndexAsync(calculationRunId, request) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(BillingInstructionsViewModel));
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
            _mockMapper.Setup(m => m.MapToViewModel(It.IsAny<ProducerBillingInstructionsResponseDto>(), It.IsAny<PaginationRequestViewModel>(), It.IsAny<string>()))
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
                    It.IsAny<string>()))
                .Returns(new BillingInstructionsViewModel());

            var mockFactory = GetMockHttpClientFactoryWithObjectResponse(billingData);
            var controller = CreateControllerWithFactory(mockFactory);

            // Act
            await controller.IndexAsync(calculationRunId, request);

            // Assert
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
                    "Test User"),
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

        private static DefaultHttpContext CreateTestHttpContext(string userName = "Test User")
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, userName) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            var mockSession = new MockHttpSession();
            mockSession.SetString("accessToken", "something");
            mockSession.SetString(SessionConstants.FinancialYear, "2024-25");

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
                    TotalRecords = billingData.TotalRecords,
                    RouteName = BillingInstructionConstants.BillingInstructionsIndexRouteName,
                    RouteValues = new Dictionary<string, object?>
                    {
                        { BillingInstructionConstants.CalculationRunIdKey, calculationRunId }
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
                    It.IsAny<string>()))
                .Returns(expectedViewModel);
        }

        private BillingInstructionsController CreateControllerWithFactory(Mock<IHttpClientFactory> mockFactory)
        {
            var controller = new BillingInstructionsController(
                _configuration,
                _mockTokenAcquisition.Object,
                _mockTelemetryClient,
                mockFactory.Object,
                _mockMapper.Object);

            controller.ControllerContext = new ControllerContext { HttpContext = CreateTestHttpContext() };
            return controller;
        }

        [TestMethod]
        public void Index_WithValidOrganisationId_FiltersOrganisations()
        {
            // Arrange
            var calculationRunId = 1;
            var model = new PaginationRequestViewModel
            {
                OrganisationId = 215150,
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = _controller.Index(calculationRunId, model) as ViewResult;
            var viewModel = result?.Model as BillingInstructionsViewModel;

            // Assert
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(1, viewModel.TablePaginationModel.Records.Count());
            Assert.AreEqual(215150, ((Organisation)viewModel.TablePaginationModel.Records.First()).OrganisationId);
            Assert.AreEqual(CommonConstants.BillingTableHeader, viewModel.TablePaginationModel.Caption);
        }
    }
}
