using System.ComponentModel.DataAnnotations;
using EPR.Calculator.Frontend.Validators;

namespace EPR.Calculator.Frontend.UnitTests.Validators
{
    [TestClass]
    public class MustBeTrueAttributeTests
    {
        [TestMethod]
        public void IsValid_ReturnsFalse_WhenValuePassedIsNull()
        {
            // Arrange
            var attribute = new MustBeTrueAttribute();

            // Act
            var result = attribute.IsValid(null);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsValid_ReturnsFalse_WhenValueIsFalse()
        {
            // Arrange
            var attribute = new MustBeTrueAttribute();

            // Act
            var result = attribute.IsValid(false);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsValid_ReturnsTrue_WhenValueIsTrue()
        {
            // Arrange
            var attribute = new MustBeTrueAttribute();

            // Act
            var result = attribute.IsValid(true);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }
    }
}