using System.Net;
using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class CalculationRunDeleteControllerTests
    {
        public CalculationRunDeleteControllerTests()
        {
            this.Fixture = new Fixture();
            this.Configuration = ConfigurationItems.GetConfigurationValues();
            this.MockHttpContext = new Mock<HttpContext>();
            this.MockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
            var headers = new HeaderDictionary
            {
                { "Referer", "https://calculator/details/4" }
            };
            this.MockHttpContext.Setup(c => c.Request.Headers).Returns(headers);
            this.MockMessageHandler = TestMockUtils.BuildMockMessageHandler(HttpStatusCode.Created);
            MockClientFactory = TestMockUtils.BuildMockHttpClientFactory(MockMessageHandler.Object);
        }

        private Fixture Fixture { get; init; }

        private IConfiguration Configuration { get; init; }

        private Mock<HttpContext> MockHttpContext { get; init; }

        private Mock<HttpMessageHandler> MockMessageHandler { get; init; }

        private Mock<IHttpClientFactory> MockClientFactory { get; init; }

        [TestMethod]
        public async Task CalculationRunDelete_Success_View_Test()
        {
            // Arrange
            int runId = this.Fixture.Create<int>();
            var controller = this.BuildTestClass(HttpStatusCode.OK);

            // Act
            var result = await controller.Index(runId) as ViewResult;

            // Assert
            var resultModel = result.Model as CalculationRunDeleteViewModel;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunDeleteIndex, result.ViewName);
            Assert.AreEqual(runId, resultModel.CalculatorRunStatusData.RunId);
            Assert.AreEqual("details", resultModel.BackLinkViewModel.BackLink);
        }

        /// <summary>
        /// Test that the DeleteConfirmationSuccess method redirects to the confirmation screen
        /// when the API returns success.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task DeleteConfirmationSuccess_RedirectToConfirmationScreenWhenDeletionSucceeds()
        {
            // Arrange
            var controller = BuildTestClass(HttpStatusCode.Created);
            var model = new CalculatorRunDetailsViewModel
            {
                RunId = this.Fixture.Create<int>(),
                RunName = this.Fixture.Create<string>(),
            };

            // Act
            var result = await controller.DeleteConfirmationSuccess(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunDeleteConfirmationSuccess, result.ViewName);
            Assert.IsTrue(typeof(CalculatorRunDetailsNewViewModel) == result.Model.GetType());
        }

        /// <summary>
        /// Tests that the DeleteConfirmationSuccess method
        /// redirects to the error page when the API returns a non-success status code.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        [TestMethod]
        public async Task DeleteConfirmationSuccess_RedirectToErrorWhenApiDoesntReturnSuccess()
        {
            // Arrange
            var controller = BuildTestClass(HttpStatusCode.BadRequest);
            var model = new CalculatorRunDetailsViewModel
            {
                RunId = this.Fixture.Create<int>(),
                RunName = this.Fixture.Create<string>(),
            };

            // Act
            var result = await controller.DeleteConfirmationSuccess(model) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
        }

        private CalculationRunDeleteController BuildTestClass(HttpStatusCode httpStatusCode)
        {
            var testClass = new CalculationRunDeleteController(
                this.Configuration,
                TestMockUtils.BuildMockApiService(httpStatusCode).Object,
                new TelemetryClient(),
                new Mock<ICalculatorRunDetailsService>().Object);
            testClass.ControllerContext.HttpContext = this.MockHttpContext.Object;
            return testClass;
        }
    }
}
