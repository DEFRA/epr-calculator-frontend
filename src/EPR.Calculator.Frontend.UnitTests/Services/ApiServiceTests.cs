namespace EPR.Calculator.Frontend.UnitTests.Services
{
    using AutoFixture;
    using EPR.Calculator.Frontend.Services;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Http;
    using Microsoft.DotNet.Scaffolding.Shared.Messaging;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Identity.Web;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Threading.Tasks;

    [TestClass]
    public class ApiServiceTests
    {
        public ApiServiceTests()
        {
            this.Fixture = new Fixture();

            this.MockHttpContext = new Mock<HttpContext>();
            this.MockSession = TestMockUtils.BuildMockSession(this.Fixture);
            this.MockHttpContext.Setup(c => c.Session).Returns(this.MockSession.Object);
            this.Token = this.Fixture.Create<string>();
            this.MockTokenAcquisition = new Mock<ITokenAcquisition>();
            this.MockTokenAcquisition.Setup(t => t.GetAccessTokenForUserAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<TokenAcquisitionOptions>()))
                .ReturnsAsync(this.Token);

            this.Configuration = TestMockUtils.BuildConfiguration();
            var messageHandler = TestMockUtils.BuildMockMessageHandler();

            this.TestClass = new ApiService(
                this.Configuration,
                Fixture.Create<TelemetryClient>(),
                TestMockUtils.BuildMockHttpClientFactory(messageHandler.Object).Object,
                this.MockTokenAcquisition.Object);
        }

        private Fixture Fixture { get; init; }

        private ApiService TestClass { get; init; }

        private Mock<HttpContext> MockHttpContext { get; init; }

        private IConfiguration Configuration { get; init; }

        private Mock<ISession> MockSession { get; init; }

        private Mock<ITokenAcquisition> MockTokenAcquisition { get; init; }

        private string Token { get; init; }

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
        public async Task CallApi_NoTokenInSession()
        {
            // Arrange
            var httpMethod = HttpMethod.Get;
            var apiUrl = Fixture.Create<Uri>();
            var argument = Fixture.Create<string>();
            var body = Fixture.Create<object>();
            byte[]? a = null;
            this.MockSession.Setup(s => s.TryGetValue("accessToken", out a))
                .Returns(false);

            // Act
            var result = await this.TestClass.CallApi(
                this.MockHttpContext.Object,
                httpMethod,
                apiUrl,
                argument,
                body).Result.Content.ReadAsStringAsync();

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task CallApi_CantRetrieveToken()
        {
            // Arrange
            var httpMethod = HttpMethod.Get;
            var apiUrl = Fixture.Create<Uri>();
            var argument = Fixture.Create<string>();
            var body = Fixture.Create<object>();
            byte[]? a = null;
            this.MockSession.Setup(s => s.TryGetValue("accessToken", out a))
                .Returns(false);
            this.Configuration.GetSection("DownstreamApi:Scopes").Value = null;

            var testClass = new ApiService(
                null,
                Fixture.Create<TelemetryClient>(),
                TestMockUtils.BuildMockHttpClientFactory(TestMockUtils.BuildMockMessageHandler().Object).Object,
                this.MockTokenAcquisition.Object);

            // Act
            Exception result = null;
            try
            {
                await testClass.CallApi(
                    this.MockHttpContext.Object,
                    httpMethod,
                    apiUrl,
                    argument,
                    body).Result.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                result = ex;
            }

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