using Microsoft.AspNetCore.Mvc;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class StandardErrorControllerTest
    {
        [TestMethod]
        public void StandardErrorController_View_Test()
        {
            var controller = new StandardErrorController();
            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.StandardErrorIndex, result.ViewName);
        }
    }
}
