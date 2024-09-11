using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.UnitTests
{
    public class LocalAuthorityConfirmationControllerTests
    {
        [TestMethod]
        public void Index_ReturnsViewResult_WithCorrectViewName()
        {
            // Arrange
            var controller = new LocalAuthorityConfirmationController();

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = controller.Index() as ViewResult;
            Assert.AreEqual(ViewNames.LocalAuthorityConfirmationIndex, viewResult.ViewName);
        }
    }
}