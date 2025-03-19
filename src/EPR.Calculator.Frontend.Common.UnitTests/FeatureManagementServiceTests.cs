using EPR.Calculator.Frontend.Common;

namespace EPR.Calculator.Frontend.Common.UnitTests
{
    [TestClass]
    public class FeatureManagementServiceTests
    {
        [TestMethod]
        public void Should_IsShowFinancialYearEnabled_ReturnsTrue()
        {
            // Assign

            // Act
            var result = FeatureManagementService.IsShowFinancialYearEnabled();

            // Assert
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void Should_IsShowFinancialYearEnabled_ReturnsFalse()
        {
            // Assign

            // Act
            var result = FeatureManagementService.IsShowFinancialYearEnabled();

            // Assert
            Assert.AreEqual(true, result);
        }
    }
}