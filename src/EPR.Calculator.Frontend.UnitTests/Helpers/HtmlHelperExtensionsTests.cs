using EPR.Calculator.Frontend.Helpers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace EPR.Calculator.Frontend.Tests.Helpers
{
    [TestClass]
    public class HtmlHelperExtensionsTests
    {
        [TestMethod]
        public void HasErrorFor_ReturnsTrue_WhenModelStateHasErrors()
        {
            // Arrange
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
            var propertyName = "TestProperty";
            viewData.ModelState.AddModelError(propertyName, "Error message");

            // Act
            var result = viewData.HasErrorFor(propertyName);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void HasErrorFor_ReturnsFalse_WhenModelStateHasNoErrors()
        {
            // Arrange
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
            var propertyName = "TestProperty";

            // Act
            var result = viewData.HasErrorFor(propertyName);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void HasErrorFor_ReturnsFalse_WhenPropertyNameDoesNotExist()
        {
            // Arrange
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
            var propertyName = "NonExistentProperty";

            // Act
            var result = viewData.HasErrorFor(propertyName);

            // Assert
            Assert.IsFalse(result);
        }
    }
}
