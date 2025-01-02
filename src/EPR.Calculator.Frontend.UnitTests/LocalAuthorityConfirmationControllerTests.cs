using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class LocalAuthorityConfirmationControllerTests
    {
        [TestMethod]
        public void Index_ReturnsViewResult_WithCorrectViewName()
        {
            // Arrange
            var controller = new LocalAuthorityConfirmationController();

            // Assert
            var viewResult = controller.Index() as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(ViewNames.LocalAuthorityConfirmationIndex, viewResult.ViewName);
        }
    }
}