using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class CalculationRunNameControllerTests
    {
        [TestMethod]
        public void CalculationRunNameController_View_Test()
        {
            var controller = new CalculationRunNameController();
            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunNameIndex, result.ViewName);
        }

        [TestMethod]
        public void CalculationRunNameController_View_WithCalculationName()
        {
            // Arrange
            var calculationName = "Test Calculation";
            var sessionMock = new Mock<ISession>();
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.Session).Returns(sessionMock.Object);

            var controller = new CalculationRunNameController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContextMock.Object
                }
            };

            // Act
            var result = controller.CalculateRun(calculationName) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunConfirmationIndex, result.ViewName);
            Assert.AreEqual(calculationName, controller.ViewBag.CalculationName);
            sessionMock.Verify(s => s.SetString("CalculationName", calculationName), Times.Once);
        }
        
        [TestMethod]
        public void CalculationRunNameController_View_WithoutCalculationName()
        {
            // Arrange
            var existingCalculationName = "Existing Calculation";
            var sessionMock = new Mock<ISession>();
            sessionMock.Setup(s => s.GetString("CalculationName")).Returns(existingCalculationName);
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.Session).Returns(sessionMock.Object);

            var controller = new CalculationRunNameController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContextMock.Object
                }
            };

            var mockHttpSession = new MockHttpSession();

            mockHttpSession.GetString("CalculationName");
            controller.ControllerContext.HttpContext.Session = mockHttpSession;

            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.LocalAuthorityUploadFileErrorIndex, result.ViewName);






            // Act
            var result = controller.CalculateRun(null) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunConfirmationIndex, result.ViewName);
            Assert.AreEqual(existingCalculationName, controller.ViewBag.CalculationName);

        }
    }
}