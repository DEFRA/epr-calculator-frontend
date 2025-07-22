namespace EPR.Calculator.Frontend.UnitTests.Controllers
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using EPR.Calculator.Frontend.Constants;
    using EPR.Calculator.Frontend.Controllers;
    using EPR.Calculator.Frontend.UnitTests.HelpersTest;
    using EPR.Calculator.Frontend.ViewModels;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Identity.Web;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ReasonForRejectionControllerTests
    {
        public ReasonForRejectionControllerTests()
        {
            this.Fixture = new Fixture();
            this.Configuration = ConfigurationItems.GetConfigurationValues();
            this.MockHttpContext = new Mock<HttpContext>();
            this.MockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
            this.MockMessageHandler = TestMockUtils.BuildMockMessageHandler(HttpStatusCode.Created);
            MockClientFactory = TestMockUtils.BuildMockHttpClientFactory(MockMessageHandler.Object);
        }

        private Fixture Fixture { get; init; }

        private IConfiguration Configuration { get; init; }

        private Mock<HttpContext> MockHttpContext { get; init; }

        private Mock<HttpMessageHandler> MockMessageHandler { get; init; }

        private Mock<IHttpClientFactory> MockClientFactory { get; init; }

        [TestMethod]
        public async Task CanCallIndex()
        {
            // Arrange
            int runId = this.Fixture.Create<int>();

            // Arrange
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            var controller = new ReasonForRejectionController(
                this.Configuration,
                new Mock<ITokenAcquisition>().Object,
                new TelemetryClient(),
                this.MockClientFactory.Object);

            controller.ControllerContext.HttpContext = this.MockHttpContext.Object;
            tempData[nameof(AcceptRejectConfirmationViewModel.Reason)] = "Some rejection reason";

            controller.TempData = tempData;

            // Act
            var result = await controller.Index(runId) as ViewResult;

            var resultModel = result.Model as AcceptRejectConfirmationViewModel;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.ReasonForRejectionIndex, result.ViewName);
            Assert.AreEqual(runId, resultModel.CalculationRunId);
            Assert.AreEqual(tempData[nameof(AcceptRejectConfirmationViewModel.Reason)], resultModel.Reason);
        }

        [TestMethod]
        public void CanCallIndexPost()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var runId = fixture.Create<int>();
            var model = fixture.Create<AcceptRejectConfirmationViewModel>();

            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            var controller = new ReasonForRejectionController(
                this.Configuration,
                new Mock<ITokenAcquisition>().Object,
                new TelemetryClient(),
                this.MockClientFactory.Object);
            controller.TempData = tempData;

            controller.ControllerContext.HttpContext = this.MockHttpContext.Object;

            // Act
            var result = controller.IndexPost(runId, model);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CanCallIndexPost_WithNoReason()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var runId = fixture.Create<int>();
            var model = new AcceptRejectConfirmationViewModel()
            {
                Reason = string.Empty,
                CalculationRunId = runId,
                Status = Enums.BillingStatus.Rejected,
                BackLinkViewModel = new BackLinkViewModel()
                {
                    BackLink = ViewNames.BillingInstructionsIndex,
                    CurrentUser = "Test user"
                },
            };

            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            var controller = new ReasonForRejectionController(
                this.Configuration,
                new Mock<ITokenAcquisition>().Object,
                new TelemetryClient(),
                this.MockClientFactory.Object);

            controller.ControllerContext.HttpContext = this.MockHttpContext.Object;
            controller.TempData = tempData;
            controller.ModelState.AddModelError("No Reason", "Provide a reason that applies to all the billing instructions you selected for rejection.");

            // Act
            var result = controller.IndexPost(runId, model) as ViewResult;
            var resultModel = result.Model as AcceptRejectConfirmationViewModel;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.ReasonForRejectionIndex, result.ViewName);
            Assert.AreEqual(runId, resultModel.CalculationRunId);
        }
    }
}