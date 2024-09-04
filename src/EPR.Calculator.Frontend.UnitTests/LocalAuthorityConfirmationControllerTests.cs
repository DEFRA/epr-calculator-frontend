using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.UnitTests
{
    public class LocalAuthorityConfirmationControllerTests
    {
        [TestMethod]
        public void LocalAuthorityConfirmationController_View_Test()
        {
            var controller = new LocalAuthorityConfirmationController();
            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.LocalAuthorityConfirmationIndex, result.ViewName);
        }
    }
}