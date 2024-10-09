using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class CalculationRunNameControllerTests
    {
        private readonly CalculationRunNameController _controller;
        private readonly Mock<ILogger<CalculationRunNameController>> _loggerMock;

        public CalculationRunNameControllerTests()
        {
            _loggerMock = new Mock<ILogger<CalculationRunNameController>>();
            _controller = new CalculationRunNameController(_loggerMock.Object);
        }

        [TestMethod]
        public void CalculationRunNameController_View_Test()
        {
            var controller = new CalculationRunNameController(_loggerMock.Object);
            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
        }

        [TestMethod]
        public void RunCalculator_ShouldReturnView_WhenCalculationNameIsInvalid()
        {
            var controller = new CalculationRunNameController(_loggerMock.Object);
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
            var controller = new CalculationRunNameController(_loggerMock.Object);
            var result = controller.RunCalculator("ValidCalculationName") as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("CalculationRunConfirmation", result.ControllerName);
        }
    }
}