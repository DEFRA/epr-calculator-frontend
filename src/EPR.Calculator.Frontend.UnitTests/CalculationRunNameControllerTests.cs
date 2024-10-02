using System.Net.Http;
using System.Text;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class CalculationRunNameControllerTests
    {
        [TestMethod]
        public void CalculationRunNameController_View_Test()
        {
            var controller = new CalculationRunNameController();
            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
        }

        [TestMethod]
        public void RunCalculator_ShouldReturnView_WhenCalculationNameIsInvalid()
        {
            var controller = new CalculationRunNameController();
            var result = controller.RunCalculator(null) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
            var errorViewModel = controller.ViewBag.Errors as ErrorViewModel;
            Assert.IsNotNull(errorViewModel);
            Assert.AreEqual(ViewControlNames.CalculationRunName, errorViewModel.DOMElementId);
            Assert.AreEqual(ErrorMessages.CalculationRunNameEmpty, errorViewModel.ErrorMessage);
        }

        [TestMethod]
        public void RunCalculator_ShouldRedirect_WhenCalculationNameIsValid()
        {
            var controller = new CalculationRunNameController();
            var mockHttpSession = new MockHttpSession();

            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var result = controller.RunCalculator("ValidCalculationName") as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Confirmation", result.ActionName);
            Assert.AreEqual("ValidCalculationName", mockHttpSession.GetString("CalculationName"));
        }

        [TestMethod]
        public void RunCalculator_WhenCalculationNameIsProvided_ShouldSetSessionAndRedirect()
        {
            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            mockHttpContext.Setup(s => s.Session).Returns(mockSession.Object);

            var controller = new CalculationRunNameController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };

            string calculationName = "TestCalculation";
            byte[] calculationNameBytes = Encoding.UTF8.GetBytes(calculationName);

            var result = controller.RunCalculator(calculationName) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Confirmation", result.ActionName);
            mockSession.Verify(s => s.Set("CalculationName", calculationNameBytes), Times.Once);
        }

        [TestMethod]
        public void RunCalculatorConfirmation_ReturnsViewResult_WithCorrectViewName()
        {
            var controller = new CalculationRunNameController();
            var result = controller.Confirmation() as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunConfirmation, result.ViewName);
        }
    }
}