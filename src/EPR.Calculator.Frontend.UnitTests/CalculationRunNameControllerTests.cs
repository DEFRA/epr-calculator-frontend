using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class CalculationRunNameControllerTests
    {
        private CalculationRunNameController _controller;

        [TestInitialize]
        public void Setup()
        {
            _controller = new CalculationRunNameController();
        }

        [TestMethod]
        public void RunCalculator_CalculationRunNameController_View_Test()
        {
            var result = _controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
        }

        [TestMethod]
        public void RunCalculator_ShouldReturnView_WhenCalculationNameIsInvalid()
        {
            var result = _controller.RunCalculator(null) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
            var errorViewModel = _controller.ViewBag.Errors as ErrorViewModel;
            Assert.IsNotNull(errorViewModel);
            Assert.AreEqual(ViewControlNames.CalculationRunName, errorViewModel.DOMElementId);
            Assert.AreEqual(ErrorMessages.CalculationRunNameEmpty, errorViewModel.ErrorMessage);
        }

        [TestMethod]
        public void RunCalculator_ShouldRedirect_WhenCalculationNameIsValid()
        {
            var result = _controller.RunCalculator("ValidCalculationName") as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("CalculationRunConfirmation", result.ControllerName);
        }

        [TestMethod]
        public void RunCalculator_WhenCalculationNameIsEmpty_ShouldReturnViewWithError()
        {
            string emptyName = string.Empty;
            var result = _controller.RunCalculator(emptyName) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
            Assert.IsTrue(_controller.ViewBag.Errors is ErrorViewModel);
            var errorViewModel = _controller.ViewBag.Errors as ErrorViewModel;
            Assert.AreEqual("Enter a name for this calculation", errorViewModel.ErrorMessage);
        }

        [TestMethod]
        public void RunCalculator_WhenCalculationNameIsTooLong_ShouldReturnViewWithError()
        {
            string longName = new string('a', 101);
            var result = _controller.RunCalculator(longName) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
            Assert.IsTrue(_controller.ViewBag.Errors is ErrorViewModel);
            var errorViewModel = _controller.ViewBag.Errors as ErrorViewModel;
            Assert.AreEqual("Calculation name must contain no more than 100 characters", errorViewModel.ErrorMessage);
        }
    }
}
