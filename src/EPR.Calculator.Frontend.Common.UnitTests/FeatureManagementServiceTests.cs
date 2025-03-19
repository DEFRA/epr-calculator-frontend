using Microsoft.Extensions.Configuration;
using Moq;

namespace EPR.Calculator.Frontend.Common.UnitTests
{
    [TestClass]
    public class FeatureManagementServiceTests
    {
        private readonly Mock<IConfiguration> mockConfiguration;
        private readonly Mock<IConfigurationSection> mockConfigurationSectionLevelOne;
        private readonly Mock<IConfigurationSection> mockConfigurationSectionLevelTwo;

        public FeatureManagementServiceTests()
        {
            mockConfiguration = new Mock<IConfiguration>();
            mockConfigurationSectionLevelOne = new Mock<IConfigurationSection>();
            mockConfigurationSectionLevelTwo = new Mock<IConfigurationSection>();
        }

        [TestMethod]
        public void Should_IsShowFinancialYearEnabled_ReturnsTrue()
        {
            // Assign
            mockConfigurationSectionLevelTwo.Setup(x => x.Value).Returns("true");
            mockConfigurationSectionLevelOne.Setup(x => x.GetSection("ShowFinancialYear")).Returns(mockConfigurationSectionLevelTwo.Object);
            mockConfiguration.Setup(x => x.GetSection("FeatureManagement")).Returns(mockConfigurationSectionLevelOne.Object);

            // Act
            var result = FeatureManagementService.IsShowFinancialYearEnabled(mockConfiguration.Object);

            // Assert
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void Should_IsShowFinancialYearEnabled_ReturnsFalse()
        {
            // Assign
            mockConfigurationSectionLevelTwo.Setup(x => x.Value).Returns("false");
            mockConfigurationSectionLevelOne.Setup(x => x.GetSection("ShowFinancialYear")).Returns(mockConfigurationSectionLevelTwo.Object);
            mockConfiguration.Setup(x => x.GetSection("FeatureManagement")).Returns(mockConfigurationSectionLevelOne.Object);

            // Act
            var result = FeatureManagementService.IsShowFinancialYearEnabled(mockConfiguration.Object);

            // Assert
            Assert.AreEqual(false, result);
        }
    }
}