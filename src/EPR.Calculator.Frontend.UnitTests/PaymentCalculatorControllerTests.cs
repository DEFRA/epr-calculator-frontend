using System.Security.Claims;
using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class PaymentCalculatorControllerTests
    {
        private PaymentCalculatorController controller;

        public PaymentCalculatorControllerTests()
        {
            this.Fixture = new Fixture();
            this.MockHttpContext = new Mock<HttpContext>();
            this.MockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
        }

        private Fixture Fixture { get; init; }

        private Mock<HttpContext> MockHttpContext { get; init; }

        [TestInitialize]
        public void Setup()
        {
            controller = new PaymentCalculatorController();

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TestMethod]
        public void AcceptInvoiceInstructions_Get_ReturnsViewWithModel()
        {
            // Act
            var result = controller.AcceptInvoiceInstructions();

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);

            var model = viewResult.Model as AcceptInvoiceInstructionsViewModel;
            Assert.IsNotNull(model);
            Assert.IsFalse(model.AcceptAll);
            Assert.AreEqual("Calculation run 99", model.CalculationRunTitle);
        }

        [TestMethod]
        public void AcceptInvoiceInstructions_Post_WhenAccepted_ReturnsRedirectToOverview()
        {
            // Arrange
            var model = new AcceptInvoiceInstructionsViewModel
            {
                AcceptAll = true,
                CurrentUser = "Test User",
                CalculationRunTitle = "Calculation run 99"
            };

            // Act
            var result = controller.AcceptInvoiceInstructions(model);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Overview", redirectResult.ActionName);
        }

        [TestMethod]
        public void AcceptInvoiceInstructions_Post_WhenNotAccepted_ReturnsViewWithError()
        {
            // Arrange
            var model = new AcceptInvoiceInstructionsViewModel
            {
                AcceptAll = false,
                CurrentUser = "Test User",
                CalculationRunTitle = "Calculation run 99"
            };

            // Act
            var result = controller.AcceptInvoiceInstructions(model);

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);

            var returnedModel = viewResult.Model as AcceptInvoiceInstructionsViewModel;
            Assert.IsNotNull(returnedModel);
            Assert.IsFalse(returnedModel.AcceptAll);
            var error = returnedModel.Errors.SingleOrDefault(e => e.DOMElementId == nameof(model.AcceptAll));
            Assert.IsNotNull(error);
        }

        [TestMethod]
        public void Index_ReturnsViewResult_WithCorrectViewName()
        {
            // Arrange
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Assert
            var viewResult = controller.BillingFileSuccess() as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(ViewNames.BillingConfirmationSuccess, viewResult.ViewName);
        }

        [TestMethod]
        public void BillingFileSuccess_ReturnsViewResult_WithCorrectViewModel()
        {
            // Arrange
            var controller = new PaymentCalculatorController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = this.MockHttpContext.Object
                }
            };

            // Act
            var result = controller.BillingFileSuccess() as ViewResult;

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
            Assert.AreEqual(CommonConstants.DashBoard, confirmationModel.RedirectController);
        }
    }
}
