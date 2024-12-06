using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class DeleteConfirmationControllerTests
    {
        [TestMethod]
        public void StandardErrorController_View_Test()
        {
            var controller = new DeleteConfirmationController();
            int runId = 1;
            string calcName = "TestCalc";
            var result = controller.Index(runId, calcName) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.DeleteConfirmation, result.ViewName);
        }
    }
}
