using EPR.Calculator.Frontend.HealthCheck;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EPR.Calculator.Frontend.UnitTests.HealthCheck
{
    [TestClass]
    public class HealthCheckOptionsBuilderTests
    {
        [TestMethod]
        public void Build_ShouldReturnHealthCheckOptions_WithExpectedProperties()
        {
            // Act
            var options = HealthCheckOptionsBuilder.Build();

            // Assert
            Assert.IsNotNull(options);
            Assert.IsFalse(options.AllowCachingResponses);
            Assert.AreEqual(StatusCodes.Status200OK, options.ResultStatusCodes[HealthStatus.Healthy]);
        }
    }
}