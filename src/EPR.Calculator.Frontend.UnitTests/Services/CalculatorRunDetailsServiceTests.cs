namespace EPR.Calculator.Frontend.UnitTests.Services
{
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using AutoFixture;
    using EPR.Calculator.Frontend.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Text.Json;
    using Moq;
    using Newtonsoft.Json;

    [TestClass]
    public class CalculatorRunDetailsServiceTests
    {
        private readonly Fixture fixture;

        public CalculatorRunDetailsServiceTests()
        {
            this.fixture = new Fixture();
        }

        [TestMethod]
        public async Task CallGetCalculatorRundetails_Succeeds()
        {
            // Arrange
            var runId = this.fixture.Create<int>();

            var responseObject = new
            {
                RunId = runId
            };

            var apiResponses = new Dictionary<(HttpMethod, string, string), (HttpStatusCode, string)>
            {
                {
                    (HttpMethod.Get, $"v1/calculatorRuns/{runId}", string.Empty),
                    (HttpStatusCode.OK, JsonConvert.SerializeObject(responseObject))
                }
            };

            var mockApiService = TestMockUtils.BuildMockApiService(apiResponses).Object;
            var service = new CalculatorRunDetailsService(mockApiService);

            // Act
            var result = await service.GetCalculatorRundetailsAsync(
                new Mock<HttpContext>().Object,
                runId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(runId, result.RunId);
        }

        [TestMethod]
        public async Task CalculatorRundetailsAsync_ThrowsExceptionWhenApiReturnsNoData()
        {
            // Arrange
            var runId = this.fixture.Create<int>();

            var apiResponses = new Dictionary<(HttpMethod, string, string), (HttpStatusCode, string)>
            {
                {
                    (HttpMethod.Get, $"v1/calculatorRuns/{runId}", string.Empty),
                    (HttpStatusCode.BadRequest, "{}")
                }
            };

            var mockApiService = TestMockUtils.BuildMockApiService(apiResponses).Object;
            var service = new CalculatorRunDetailsService(mockApiService);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<HttpRequestException>(
                async () => await service.GetCalculatorRundetailsAsync(
                    new Mock<HttpContext>().Object,
                    runId));
        }
    }
}
