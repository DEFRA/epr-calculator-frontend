using Microsoft.Extensions.Configuration;
using Moq;

namespace EPR.Calculator.Frontend.Common.UnitTests
{
    [TestClass]
    public class FeatureManagementServiceTests
    {
        private readonly Mock<IConfiguration> mockConfiguration;
        private readonly Mock<IConfigurationSection> mockConfigurationSection;
        private readonly Mock<IConfigurationSection> mockConfigurationSectionLevelOne;
        private readonly Mock<IConfigurationSection> mockConfigurationSectionLevelTwo;

        public FeatureManagementServiceTests()
        {
            mockConfiguration = new Mock<IConfiguration>();
            mockConfigurationSection = new Mock<IConfigurationSection>();
            mockConfigurationSectionLevelOne = new Mock<IConfigurationSection>();
            mockConfigurationSectionLevelTwo = new Mock<IConfigurationSection>();
        }

        [TestMethod]
        public void Should_IsShowFinancialYearEnabled_ReturnsTrue()
        {
            // Assign
            mockConfigurationSection.Setup(x => x.Path).Returns("FeatureManagement");
            mockConfigurationSection.Setup(x => x.Key).Returns("ShowFinancialYear");
            mockConfigurationSection.Setup(x => x.Value).Returns("true");
            mockConfiguration.Setup(x => x.GetSection("FeatureManagement")).Returns(mockConfigurationSection.Object);
            // var configValue = mockConfiguration.Object.GetSection("FeatureManagement:ShowFinancialYear").Value;

            // mockConfigurationSectionLevelOne.Setup(x => x.Path).Returns("FeatureManagement");
            // mockConfigurationSectionLevelTwo.Setup(x => x.Key).Returns("ShowFinancialYear");

            // Act
            var result = FeatureManagementService.IsShowFinancialYearEnabled(mockConfiguration.Object);

            // Assert
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void Should_IsShowFinancialYearEnabled_ReturnsFalse()
        {
            // Assign
            mockConfigurationSection.Setup(x => x.Value).Returns("FeatureManagement");
            mockConfiguration.Setup(x => x.GetSection("ShowFinancialYear")).Returns(mockConfigurationSection.Object);

            // Act
            var result = FeatureManagementService.IsShowFinancialYearEnabled(mockConfiguration.Object);

            // Assert
            Assert.AreEqual(false, result);
        }
    }
}