namespace EPR.Calculator.Frontend.UnitTests.Services
{
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using EPR.Calculator.Frontend.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CalculatorRunDetailsServiceTests
    {
        public CalculatorRunDetailsServiceTests()
        {
            this.Fixture = new Fixture(); // .Customize(new AutoMoqCustomization());
            this.ApiService = new Mock<IApiService>();
            this.ApiService.Setup(s => s.GetApiUrl(
                It.IsAny<string>(),
                It.IsAny<string>())).Returns(Fixture.Create<Uri>());
            this.ApiService.Setup(s => s.CallApi(
                new Mock<HttpContext>().Object,
                HttpMethod.Get,
                It.IsAny<Uri>(),
                It.IsAny<string>(),
                It.IsAny<object?>())).Callback(
                (HttpMethod a, Uri b, string c, object d) => new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent($"{{\"RunId\" : \"{c}\"}}"),
                });
            this.TestClass = new CalculatorRunDetailsService(this.ApiService.Object);
        }

        private CalculatorRunDetailsService TestClass { get; init; }

        private IFixture Fixture { get; init; }

        private Mock<IApiService> ApiService { get; init; }

        [TestMethod]
        public async Task CallGetCalculatorRundetails_Succeeds()
        {
            // Arrange
            var runId = Fixture.Create<int>();

            // Act
            var result = await this.TestClass.GetCalculatorRundetailsAsync(
                new Mock<HttpContext>().Object,
                runId);

            // Assert
            Assert.AreEqual(runId, result.RunId);
        }

        [TestMethod]
        public async Task CalculatorRundetailsAsync_ThrowsExceptionWhenApiReturnsNoData()
        {
            // Arrange
            var runId = Fixture.Create<int>();
            this.ApiService.Setup(s => s.CallApi(
                new Mock<HttpContext>().Object,
                HttpMethod.Get,
                It.IsAny<Uri>(),
                It.IsAny<string>(),
                It.IsAny<object?>())).ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(null),
                });

            // Act
            Exception result = null;
            try
            {
                await this.TestClass.GetCalculatorRundetailsAsync(
                    new Mock<HttpContext>().Object,
                    runId);
            }
            catch (Exception ex)
            {
                result = ex;
            }

            // Assert
            Assert.IsInstanceOfType<InvalidOperationException>(result);
        }
    }
}