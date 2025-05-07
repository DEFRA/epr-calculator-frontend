using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class DownloadFileErrorControllerTest
    {
        public DownloadFileErrorControllerTest()
        {
            this.Fixture = new Fixture();
            this.MockHttpContext = new Mock<HttpContext>();
            this.MockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
        }

        private Fixture Fixture { get; init; }

        private Mock<HttpContext> MockHttpContext { get; init; }

        [TestMethod]
        public void DonwloadErrorController_View_Test()
        {
            var controller = new DownloadFileErrorController();
            controller.ControllerContext.HttpContext = this.MockHttpContext.Object;
            int runId = 1;
            string calcName = "TestCalc";
            string createdDate = "21 June 2024";
            string createdTime = "12:09";
            var result = controller.Index(runId, calcName, createdDate, createdTime) as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.DownloadFileErrorIndex, result.ViewName);
        }

        [TestMethod]
        public void IndexNew_ReturnsCorrectViewAndModel()
        {
            // Arrange
            var controller = new DownloadFileErrorController();
            var runId = 123;
            var calcName = "Test Calculation";
            var createdDate = "2023-10-01";
            var createdTime = "12:00 PM";

            // Mock HttpContext to simulate a user
            var httpContextMock = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
            httpContextMock.Setup(c => c.User.Identity.Name).Returns("TestUser");
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };

            // Act
            var result = controller.IndexNew(runId, calcName, createdDate, createdTime) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.AreEqual(ViewNames.DownloadFileErrorIndexNew, result.ViewName, "View name should match.");

            var model = result.Model as CalculatorRunStatusUpdateViewModel;
            Assert.IsNotNull(model, "Model should not be null.");
            Assert.AreEqual("TestUser", model.CurrentUser, "CurrentUser should match.");
            Assert.AreEqual(runId, model.Data.RunId, "RunId should match.");
            Assert.AreEqual((int)RunClassification.DELETED, model.Data.ClassificationId, "ClassificationId should match.");
            Assert.AreEqual(calcName, model.Data.CalcName, "CalcName should match.");
            Assert.AreEqual(createdDate, model.Data.CreatedDate, "CreatedDate should match.");
            Assert.AreEqual(createdTime, model.Data.CreatedTime, "CreatedTime should match.");
        }
    }
}
