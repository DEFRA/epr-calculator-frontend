using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using Microsoft.AspNetCore.Mvc;

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
