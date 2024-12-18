using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
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
    }
}
