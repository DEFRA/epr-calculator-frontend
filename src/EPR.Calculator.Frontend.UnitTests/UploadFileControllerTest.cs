using EPR.Calculator.Frontend.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class UploadFileControllerTest
    {
        [TestMethod]
        public void UploadFileController_View_Test()
        {
            var controller = new UploadFileController();
            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ViewName);
        }
    }
}
