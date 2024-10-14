using System.Text;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using EPR.Calculator.Frontend.Validators;
using EPR.Calculator.Frontend.ViewModels;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class CalculationRunNameControllerTests
    {
        private CalculationRunNameController _controller;
        private CalculatorRunNameValidator _validationRules;

        [TestInitialize]
        public void Setup()
        {
            _controller = new CalculationRunNameController();
            _validationRules = new CalculatorRunNameValidator();
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
            _controller.ModelState.AddModelError("CalculationName", "Enter a name for this calculation");
            var calculatorRunModel = new InitiateCalculatorRunModel() { CalculationName = null };
            var result = _controller.RunCalculator(null) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
            var errorViewModel = _controller.ViewBag.Errors as ErrorViewModel;
            Assert.IsNotNull(errorViewModel);
            Assert.AreEqual(ViewControlNames.CalculationRunName, errorViewModel.DOMElementId);
            Assert.AreEqual(ErrorMessages.CalculationRunNameEmpty, errorViewModel.ErrorMessage);
        }

        [TestMethod]
        public void RunCalculator_ShouldRedirect_IsOnlyAlphabets_WhenCalculationNameIsValid()
        {
            var controller = new CalculationRunNameController();
            var mockHttpSession = new MockHttpSession();

            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var calculatorRunModel = new InitiateCalculatorRunModel() { CalculationName = "ValidCalculationName" };
            var result = controller.RunCalculator(calculatorRunModel) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.RunCalculatorConfirmation, result.ActionName);
            Assert.AreEqual("ValidCalculationName", mockHttpSession.GetString(SessionConstants.CalculationName));
        }

        [TestMethod]
        public void RunCalculator_ShouldRedirect_IsOnlyNumeric_WhenCalculationNameIsValid()
        {
            var controller = new CalculationRunNameController();
            var mockHttpSession = new MockHttpSession();

            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var calculatorRunModel = new InitiateCalculatorRunModel() { CalculationName = "1234" };
            var result = controller.RunCalculator(calculatorRunModel) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.RunCalculatorConfirmation, result.ActionName);
            Assert.AreEqual("1234", mockHttpSession.GetString(SessionConstants.CalculationName));
        }

        [TestMethod]
        public void RunCalculator_ShouldRedirect_IsAplhaNumeric_WithNoSpace_WhenCalculationNameIsValid()
        {
            var controller = new CalculationRunNameController();
            var mockHttpSession = new MockHttpSession();

            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var calculatorRunModel = new InitiateCalculatorRunModel() { CalculationName = "ValidCalculationName1234" };
            var result = controller.RunCalculator(calculatorRunModel) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.RunCalculatorConfirmation, result.ActionName);
            Assert.AreEqual("ValidCalculationName1234", mockHttpSession.GetString(SessionConstants.CalculationName));
        }

        [TestMethod]
        public void RunCalculator_WhenCalculationName__IsAplhaNumeric_WithSpace_IsValid()
        {
            var controller = new CalculationRunNameController();
            var mockHttpSession = new MockHttpSession();

            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var calculatorRunModel = new InitiateCalculatorRunModel() { CalculationName = "ValidCalculationName 123" };
            var result = controller.RunCalculator(calculatorRunModel) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.RunCalculatorConfirmation, result.ActionName);
            Assert.AreEqual("ValidCalculationName 123", mockHttpSession.GetString(SessionConstants.CalculationName));
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

            var calculatorRunModel = new InitiateCalculatorRunModel() { CalculationName = "TestCalculation" };
            byte[] calculationNameBytes = Encoding.UTF8.GetBytes(calculatorRunModel.CalculationName);

            var result = controller.RunCalculator(calculatorRunModel) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.RunCalculatorConfirmation, result.ActionName);
            mockSession.Verify(s => s.Set(SessionConstants.CalculationName, calculationNameBytes), Times.Once);
        }

        [TestMethod]
        public void RunCalculatorConfirmation_ReturnsViewResult_WithCorrectViewName()
        {
            var controller = new CalculationRunNameController();
            var result = controller.Confirmation() as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunConfirmation, result.ViewName);
        }

        [TestMethod]
        public void RunCalculator_WhenCalculationNameIsEmpty_ShouldReturnViewWithError()
        {
            var calculatorRunModel = new InitiateCalculatorRunModel() { CalculationName = string.Empty };
            _controller.ModelState.AddModelError("CalculationName", "Enter a name for this calculation");
            var result = _controller.RunCalculator(calculatorRunModel) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
            Assert.IsTrue(_controller.ViewBag.Errors is ErrorViewModel);
            var errorViewModel = _controller.ViewBag.Errors as ErrorViewModel;
            Assert.AreEqual(ErrorMessages.CalculationRunNameEmpty, errorViewModel.ErrorMessage);
        }

        [TestMethod]
        public void RunCalculator_WhenCalculationNameIsTooLong_ShouldReturnViewWithError()
        {
            _controller.ModelState.AddModelError("CalculationName", "Calculation name must contain no more than 100 characters");
            var calculatorRunModel = new InitiateCalculatorRunModel() { CalculationName = new string('a', 101) };
            var result = _controller.RunCalculator(calculatorRunModel) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
            Assert.IsTrue(_controller.ViewBag.Errors is ErrorViewModel);
            var errorViewModel = _controller.ViewBag.Errors as ErrorViewModel;
            Assert.AreEqual(ErrorMessages.CalculationRunNameMaxLengthExceeded, errorViewModel.ErrorMessage);
        }

        [TestMethod]
        public void RunCalculator_WhenCalculationName_IsNotAlphaNumeric_ShouldReturnViewWithError()
        {
            _controller.ModelState.AddModelError("CalculationName", "Calculation name must only contain numbers and letters");
            var calculatorRunModel = new InitiateCalculatorRunModel() { CalculationName = "%^&*$@" };
            var result = _controller.RunCalculator(calculatorRunModel) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
            Assert.IsTrue(_controller.ViewBag.Errors is ErrorViewModel);
            var errorViewModel = _controller.ViewBag.Errors as ErrorViewModel;
            Assert.AreEqual(ErrorMessages.CalculationRunNameMustBeAlphaNumeric, errorViewModel.ErrorMessage);
        }

        [TestMethod]
        public void RunCalculator_Validator_Should_Have_Error_Is_Empty()
        {
            var model = new InitiateCalculatorRunModel { CalculationName = string.Empty };
            var result = _validationRules.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CalculationName)
                .WithErrorMessage(ErrorMessages.CalculationRunNameEmpty);
        }

        [TestMethod]
        public void RunCalculator_Validator_Should_Have_Error_Exceeds_MaxLength()
        {
            var model = new InitiateCalculatorRunModel { CalculationName = new string('a', 101) };
            var result = _validationRules.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CalculationName)
                .WithErrorMessage(ErrorMessages.CalculationRunNameMaxLengthExceeded);
        }

        [TestMethod]
        public void RunCalculator_Validator_Should_Have_Error_When_Is_Not_AlphaNumeric()
        {
            var model = new InitiateCalculatorRunModel { CalculationName = "test_123" };
            var result = _validationRules.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CalculationName)
                .WithErrorMessage(ErrorMessages.CalculationRunNameMustBeAlphaNumeric);
        }

        [TestMethod]
        public void RunCalculator_Validator_Should_Not_Have_Error_Is_Valid()
        {
            var model = new InitiateCalculatorRunModel { CalculationName = "test123" };
            var result = _validationRules.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.CalculationName);
        }

        [TestMethod]
        public void RunCalculator_Validator_Should_Not_Have_Error_Has_Spaces()
        {
            var model = new InitiateCalculatorRunModel { CalculationName = "test 123" };
            var result = _validationRules.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.CalculationName);
        }

        [TestMethod]
        public void TriggerException_ThrowsInvalidOperationException()
        {
            var controller = new CalculationRunNameController();

            var exception = Assert.ThrowsException<InvalidOperationException>(() => controller.TriggerException());
            Assert.AreEqual("Paycal Internal Server Error Testing", exception.Message);
        }
    }
}