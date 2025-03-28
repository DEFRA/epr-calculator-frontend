namespace EPR.Calculator.Frontend.UnitTests.Extensions
{
    using EPR.Calculator.Frontend.Constants;
    using EPR.Calculator.Frontend.Extensions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EnumExtensionTests
    {
        [TestMethod]
        public void CanCallGetDisplayName()
        {
            // Arrange
            var enumValue = ParameterType.CommunicationCostsByMaterial;

            // Act
            var result = enumValue.GetDisplayName();

            // Assert
            Assert.AreEqual("Communication costs by material", result);
        }
    }
}