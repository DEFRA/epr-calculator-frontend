namespace EPR.Calculator.Frontend.Common.UnitTests
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.FeatureManagement;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

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
        public async Task Should_FeatureManager_ReturnsTrue()
        {
            // Assign
            var featureManagerMock = new Mock<IFeatureManager>();
            featureManagerMock.Setup(fm => fm.IsEnabledAsync("some_feature")).Returns(Task.FromResult(true));
            var featureManager = featureManagerMock.Object;

            // Act
            var result = await featureManager.IsEnabledAsync("some_feature");

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task Should_FeatureManager_ReturnsFalse()
        {
            // Assign
            var featureManagerMock = new Mock<IFeatureManager>();
            featureManagerMock.Setup(fm => fm.IsEnabledAsync("some_feature")).Returns(Task.FromResult(false));
            var featureManager = featureManagerMock.Object;

            // Act
            var result = await featureManager.IsEnabledAsync("some_feature");

            // Assert
            Assert.IsFalse(result);
        }
    }
}