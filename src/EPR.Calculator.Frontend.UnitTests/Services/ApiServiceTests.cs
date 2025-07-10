namespace EPR.Calculator.Frontend.UnitTests.Services
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AutoFixture;
    using EPR.Calculator.Frontend.Services;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Identity.Web;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ApiServiceTests
    {
        public ApiServiceTests()
        {
            this.Fixture = new Fixture();

            this.MockHttpContext = new Mock<HttpContext>();
            this.MockHttpContext.Setup(c => c.Session).Returns(TestMockUtils.BuildMockSession(this.Fixture).Object);
            this.Configuration = TestMockUtils.BuildConfiguration();
            var messageHandler = TestMockUtils.BuildMockMessageHandler();

            this.TestClass = new ApiService(
                this.Configuration,
                Fixture.Create<TelemetryClient>(),
                TestMockUtils.BuildMockHttpClientFactory(messageHandler.Object).Object,
                new Mock<ITokenAcquisition>().Object);
        }

        private Fixture Fixture { get; init; }

        private ApiService TestClass { get; init; }

        private Mock<HttpContext> MockHttpContext { get; init; }

        private IConfiguration Configuration { get; init; }

        [TestMethod]
        public async Task CanCallCallApi()
        {
            // Arrange
            var httpMethod = HttpMethod.Get;
            var apiUrl = Fixture.Create<Uri>();
            var argument = Fixture.Create<string>();
            var body = Fixture.Create<object>();

            // Act
            var result = await this.TestClass.CallApi(
                this.MockHttpContext.Object,
                httpMethod,
                apiUrl,
                argument,
                body);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CanCallGetApiUrl()
        {
            // Arrange
            var configSection = this.Configuration;
            var configKey = Fixture.Create<string>();

            // Act
            var result = this.TestClass.GetApiUrl("ParameterSettings", "DefaultParameterSettingsApi");

            // Assert
            Assert.AreEqual(
                Configuration.GetSection("ParameterSettings").GetValue<string>("DefaultParameterSettingsApi"),
                result.ToString());
        }
    }
}