using System.Net;
using System.Security.Claims;
using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
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

        public BillingInstructionsControllerTests()
        {
            this.Fixture = new Fixture();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockTokenAcquisition = new Mock<ITokenAcquisition>();
            _mockTelemetryClient = new TelemetryClient();
            _mockClientFactory = new Mock<IHttpClientFactory>();

            _controller = BuildTestClass(
                Fixture,
                HttpStatusCode.OK,
                MockData.GetCalculatorRun(),
                Fixture.Create<CalculatorRunDetailsViewModel>(),
                _mockConfiguration.Object);

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

        private Fixture Fixture { get; init; }

        [TestMethod]
        public void Index_InvalidCalculationRunId_RedirectsToError()
        {
            // Arrange
            var calculationRunId = -1;
            var request = new PaginationRequestViewModel { Page = 1, PageSize = 10 };

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
            var request = new PaginationRequestViewModel { Page = 1, PageSize = 10 };

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
                new TelemetryClient(),
                mockApiService,
                TestMockUtils.BuildMockCalculatorRunDetailsService(details).Object);
            testClass.ControllerContext.HttpContext = new DefaultHttpContext()
            {
                Session = TestMockUtils.BuildMockSession(fixture).Object,
            };

            return testClass;
        }
    }
}
