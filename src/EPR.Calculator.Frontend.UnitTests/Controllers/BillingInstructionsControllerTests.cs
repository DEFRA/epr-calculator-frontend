using System.Security.Claims;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests.Controllers
{
    [TestClass]
    public class BillingInstructionsControllerTests
    {
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<ITokenAcquisition> _mockTokenAcquisition;
        private TelemetryClient _mockTelemetryClient;
        private Mock<IHttpClientFactory> _mockClientFactory;
        private BillingInstructionsController _controller;
        private Mock<HttpContext> _mockHttpContext;

        [TestInitialize]
        public void Setup()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockTokenAcquisition = new Mock<ITokenAcquisition>();
            _mockTelemetryClient = new TelemetryClient();
            _mockClientFactory = new Mock<IHttpClientFactory>();

            _controller = new BillingInstructionsController(
                _mockConfiguration.Object,
                _mockTokenAcquisition.Object,
                _mockTelemetryClient,
                _mockClientFactory.Object);

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
        public void Index_InvalidCalculationRunId_RedirectsToError()
        {
            // Arrange
            var calculationRunId = -1;
            var request = new BillingInstructionViewModel { Page = 1, PageSize = 10 };

            // Act
            var result = _controller.Index(calculationRunId, request) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
        }

        [TestMethod]
        public void Index_ValidCalculationRunId_ReturnsViewResult()
        {
            // Arrange
            var calculationRunId = 1;
            var request = new BillingInstructionViewModel { Page = 1, PageSize = 10 };

            // Act
            var result = _controller.Index(calculationRunId, request) as ViewResult;

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
            Assert.AreEqual(calculationRunId, result.RouteValues["calculationRunId"]);
        }

        [TestMethod]
        public void Index_WithValidOrganisationId_FiltersOrganisations()
        {
            // Arrange
            var calculationRunId = 1;
            var model = new BillingInstructionViewModel
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
        }
    }
}
