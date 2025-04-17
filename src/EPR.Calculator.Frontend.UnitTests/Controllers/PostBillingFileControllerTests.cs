namespace EPR.Calculator.Frontend.UnitTests.Controllers
{
    using AutoFixture;
    using EPR.Calculator.Frontend.Controllers;
    using EPR.Calculator.Frontend.ViewModels;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class PostBillingFileControllerTests
    {
        public PostBillingFileControllerTests()
        {
            this.Fixture = new Fixture();
            this.MockHttpContext = new Mock<HttpContext>();
            this.MockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
        }

        private Fixture Fixture { get; }

        private Mock<HttpContext> MockHttpContext { get; }

        [TestMethod]
        public void CanCallIndex()
        {
            var controller = new PostBillingFileController();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = this.MockHttpContext.Object
            };

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);

            var model = result.Model as PostBillingFileViewModel;
            Assert.IsNotNull(model);
        }
    }
}