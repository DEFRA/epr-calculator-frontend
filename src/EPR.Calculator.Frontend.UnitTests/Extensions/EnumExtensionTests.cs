using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using EPR.Calculator.Frontend.Extensions;

namespace EPR.Calculator.Frontend.Tests.Extensions
{
    [TestClass]
    public class EnumExtensionTests
    {
        private enum TestEnum
        {
            [Display(Name = "Display Name")]
            WithDisplayName,

            [System.ComponentModel.Description("Description Name")]
            WithDescription,

            [EnumMember(Value = "Enum Member Value")]
            WithEnumMember,

            WithoutAttributes
        }

        [TestMethod]
        public void GetDisplayName_ShouldReturnDisplayName_WhenDisplayAttributeIsPresent()
        {
            // Arrange
            var enumValue = TestEnum.WithDisplayName;

            // Act
            var result = enumValue.GetDisplayName();

            // Assert
            Assert.AreEqual("Display Name", result);
        }

        [TestMethod]
        public void GetDisplayName_ShouldReturnDescription_WhenDescriptionAttributeIsPresent()
        {
            // Arrange
            var enumValue = TestEnum.WithDescription;

            // Act
            var result = enumValue.GetDisplayName();

            // Assert
            Assert.AreEqual("Description Name", result);
        }

        [TestMethod]
        public void GetDisplayName_ShouldReturnEnumMemberValue_WhenEnumMemberAttributeIsPresent()
        {
            // Arrange
            var enumValue = TestEnum.WithEnumMember;

            // Act
            var result = enumValue.GetDisplayName();

            // Assert
            Assert.AreEqual("Enum Member Value", result);
        }

        [TestMethod]
        public void GetDisplayName_ShouldReturnEnumName_WhenNoAttributesArePresent()
        {
            // Arrange
            var enumValue = TestEnum.WithoutAttributes;

            // Act
            var result = enumValue.GetDisplayName();

            // Assert
            Assert.AreEqual("WithoutAttributes", result);
        }
    }
}
