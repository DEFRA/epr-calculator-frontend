using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using EPR.Calculator.Frontend.Extensions;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace EPR.Calculator.Frontend.Tests.Extensions
{
    [TestClass]
    public class EnumExtensionTests
    {
        private enum TestEnum
        {
            [Display(Name = "Display Name")]
            WithDisplayName,

            [Description("Description Name")]
            WithDescription,

            [EnumMember(Value = "Enum Member Value")]
            WithEnumMember,

            WithoutAttributes,

            [Display(Name = "Display Name")]
            [Description("Description Name")]
            [EnumMember(Value = "Enum Member Value")]
            WithAllAttributes
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

        [DataTestMethod]
        [DataRow(false, false, false)]
        [DataRow(true, false, false)]
        [DataRow(true, true, false)]
        [DataRow(true, true, true)]
        public void GetDisplayName_ShouldReturnExpectedValue_WhenAllAttributesArePresent(
            bool skipDisplayAttribute,
            bool skipDescriptionAttribute,
            bool skipEnumMemberAttribute)
        {
            // Arrange
            var enumValue = TestEnum.WithAllAttributes;

            // Act
            var result = enumValue.GetDisplayName(skipDisplayAttribute, skipDescriptionAttribute, skipEnumMemberAttribute);

            // Assert
            if (!skipDisplayAttribute && !skipDescriptionAttribute && !skipEnumMemberAttribute)
            {
                Assert.AreEqual("Display Name", result);
            }
            else if (skipDisplayAttribute && !skipDescriptionAttribute && !skipEnumMemberAttribute)
            {
                Assert.AreEqual("Description Name", result);
            }
            else if (skipDisplayAttribute && skipDescriptionAttribute && !skipEnumMemberAttribute)
            {
                Assert.AreEqual("Enum Member Value", result);
            }
            else if (skipDisplayAttribute && skipDescriptionAttribute && skipEnumMemberAttribute)
            {
                Assert.AreEqual(enumValue.ToString(), result);
            }
        }
    }
}
