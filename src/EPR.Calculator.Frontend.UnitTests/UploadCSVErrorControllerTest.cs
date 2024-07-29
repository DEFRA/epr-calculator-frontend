using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class UploadCSVErrorControllerTest
    {
        [TestMethod]
        public void UploadCSVErrorController_View_Test()
        {
            var controller = new UploadCSVErrorController();
            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.Index, result.ViewName);
        }
    }
}
