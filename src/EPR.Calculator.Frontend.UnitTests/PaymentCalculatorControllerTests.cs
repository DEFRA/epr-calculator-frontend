using System.ComponentModel.DataAnnotations;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class PaymentCalculatorControllerTests
    {
        private readonly IConfiguration _configuration = ConfigurationItems.GetConfigurationValues();
        private Mock<ITokenAcquisition> _mockTokenAcquisition;
        private TelemetryClient _telemetryClient;
        private PaymentCalculatorController _controller;
        private Mock<HttpContext> _mockHttpContext;

        [TestInitialize]
        public void Setup()
        {
            _mockTokenAcquisition = new Mock<ITokenAcquisition>();
            _telemetryClient = new TelemetryClient();
            _mockHttpContext = new Mock<HttpContext>();

            _controller = new PaymentCalculatorController(
                   _configuration,
                   _mockTokenAcquisition.Object,
                   _telemetryClient)
            {
                // Setting the mocked HttpContext for the controller
                ControllerContext = new ControllerContext
                {
                    HttpContext = _mockHttpContext.Object
                }
            };
        }

        [TestMethod]
        public void Index_ReturnsViewResult_WithAcceptInvoiceInstructionsViewModel()
        {
            // Arrange
            int runId = 1;
            // Mocking HttpContext.User.Identity.Name to simulate a logged-in user
            _mockHttpContext.Setup(ctx => ctx.User.Identity.Name).Returns("TestUser");

            // Act
            var result = _controller.Index(runId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(AcceptInvoiceInstructionsViewModel));
            var model = result.Model as AcceptInvoiceInstructionsViewModel;
            Assert.AreEqual(runId, model.RunId);
            Assert.IsFalse(model.AcceptAll);
            Assert.AreEqual("Calculation Run 99", model.CalculationRunTitle);
            Assert.AreEqual(ControllerNames.ClassifyRunConfirmation, model.BackLink);
        }

        [TestMethod]
        public void AcceptInvoiceInstructions_Post_ValidModelState_RedirectsToCalculationRunOverview()
        {
            // Arrange
            var model = new AcceptInvoiceInstructionsViewModel()
            {
                RunId = 99,
                AcceptAll = true, // Ensure this satisfies the [MustBeTrue] validation
                BackLink = "dummy",
                CurrentUser = "dummyuser",
                CalculationRunTitle = "Calculation Run 99"
            };

            // Act
            var result = _controller.Submit(model) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result); // Ensure the result is a RedirectToActionResult
            Assert.AreEqual(ActionNames.Index, result.ActionName); // Ensure it redirects to the correct action
            Assert.AreEqual(model.RunId, result.RouteValues["runId"]); // Ensure the route values are correct
        }

        [TestMethod]
        public void Submit_InvalidModelState_RedirectsToIndex()
        {
            // Arrange
            var model = new AcceptInvoiceInstructionsViewModel { RunId = 1 };
            _controller.ModelState.AddModelError("AcceptAll", "Required");

            // Act
            var result = _controller.Submit(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ViewName);
        }

        [TestMethod]
        public void Submit_ValidModelState_RedirectsToCalculationRunOverview()
        {
            // Arrange
            var model = new AcceptInvoiceInstructionsViewModel { RunId = 1, AcceptAll = true };

            // Act
            var result = _controller.Submit(model) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(ControllerNames.CalculationRunOverview, result.ControllerName);
            Assert.AreEqual(model.RunId, result.RouteValues["RunId"]);
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
    }
}
