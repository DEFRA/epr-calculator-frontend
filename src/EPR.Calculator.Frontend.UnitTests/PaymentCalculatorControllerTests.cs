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
        public PaymentCalculatorControllerTests()
        {
            this.Fixture = new Fixture();
            this.MockHttpContext = new Mock<HttpContext>();
            this.MockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
        }

        private Fixture Fixture { get; init; }

        private Mock<HttpContext> MockHttpContext { get; init; }

        [TestMethod]
        public void Index_ReturnsViewResult_WithCorrectViewName()
        {
            // Arrange
            var controller = new PaymentCalculatorController();
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
            Assert.AreEqual(ConfirmationMessages.BillingFileSuccess.Title, confirmationModel.Title);
            Assert.AreEqual(ConfirmationMessages.BillingFileSuccess.Body, confirmationModel.Body);
            CollectionAssert.AreEqual(ConfirmationMessages.BillingFileSuccess.NextText, confirmationModel.AdditionalParagraphs);
            Assert.AreEqual(CommonConstants.DashBoard, confirmationModel.RedirectController);
        }
    }
}