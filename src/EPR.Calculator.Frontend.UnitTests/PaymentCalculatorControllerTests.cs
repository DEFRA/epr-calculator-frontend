using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Claims;

namespace EPR.Calculator.UnitTests.Controllers
{
    [TestClass]
    public class PaymentCalculatorControllerTests
    {
        private PaymentCalculatorController controller;

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
            Assert.IsTrue(controller.ModelState.ContainsKey("AcceptAll"));
        }
    }
}
