using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class CalculationRunErrorControllerTests
    {
        private Mock<ITempDataDictionary> _tempDataMock;

        [TestMethod]
        public void CalculationRunErrorController_View_Test()
        {
            _tempDataMock = new Mock<ITempDataDictionary>();
            _tempDataMock.Setup(tempData => tempData["ErrorMessage"]).Returns("Test error message");
            var controller = new CalculationRunErrorController();
            controller.TempData = _tempDataMock.Object;
            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalcularionRunErrorIndex, result.ViewName);
        }
    }
}
