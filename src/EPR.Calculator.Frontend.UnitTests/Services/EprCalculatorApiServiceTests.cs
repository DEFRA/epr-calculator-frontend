﻿namespace EPR.Calculator.Frontend.UnitTests.Services
{
    using System;
    using System.Configuration;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using AutoFixture;
    using EPR.Calculator.Frontend.Services;
    using EPR.Calculator.Frontend.UnitTests.HelpersTest;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Identity.Web;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class EprCalculatorApiServiceTests
    {
        private readonly IConfiguration configuration = ConfigurationItems.GetConfigurationValues();

        public EprCalculatorApiServiceTests()
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
            (this.Configuration as IConfigurationRoot)!.GetSection("DownstreamApi")["Scopes"] = "scope1";
            var messageHandler = TestMockUtils.BuildMockMessageHandler();

            this.TestClass = new EprCalculatorApiService(
                this.Configuration,
                Fixture.Create<TelemetryClient>(),
                TestMockUtils.BuildMockHttpClientFactory(messageHandler.Object).Object,
                this.MockTokenAcquisition.Object);
        }

        private Fixture Fixture { get; init; }

        private EprCalculatorApiService TestClass { get; init; }

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
            var relativePath = Fixture.Create<string>();
            var queryParams = Fixture.Create<IDictionary<string, string?>>();
            var body = Fixture.Create<object>();

            // Act
            var result = await this.TestClass.CallApi(
                this.MockHttpContext.Object,
                httpMethod,
                relativePath,
                queryParams,
                body);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task CallApi_NoTokenInSession()
        {
            // Arrange
            var httpMethod = HttpMethod.Get;
            var relativePath = Fixture.Create<string>();
            var queryParams = Fixture.Create<IDictionary<string, string?>>();
            var body = Fixture.Create<object>();
            byte[]? a = null;
            this.MockSession.Setup(s => s.TryGetValue("accessToken", out a))
                .Returns(false);

            // Act
            var result = await this.TestClass.CallApi(
                this.MockHttpContext.Object,
                httpMethod,
                relativePath,
                queryParams,
                body).Result.Content.ReadAsStringAsync();

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task CallApi_CantRetrieveToken()
        {
            // Arrange
            var httpMethod = HttpMethod.Get;
            var relativePath = Fixture.Create<string>();
            var queryParams = Fixture.Create<IDictionary<string, string?>>();
            var body = Fixture.Create<object>();
            byte[]? a = null;
            this.MockSession.Setup(s => s.TryGetValue("accessToken", out a))
                .Returns(false);

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Test.json")
                .Build();

            var testClass = new EprCalculatorApiService(
                configuration,
                Fixture.Create<TelemetryClient>(),
                TestMockUtils.BuildMockHttpClientFactory(TestMockUtils.BuildMockMessageHandler().Object).Object,
                this.MockTokenAcquisition.Object);

            // Act
            Exception? result = null;
            try
            {
                await testClass.CallApi(
                    this.MockHttpContext.Object,
                    httpMethod,
                    relativePath,
                    queryParams,
                    body).Result.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                result = ex;
            }

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
