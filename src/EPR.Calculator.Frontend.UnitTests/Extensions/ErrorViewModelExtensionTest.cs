using EPR.Calculator.Frontend.Extensions;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.UnitTests.Extensions
{
    [TestClass]
    public class ErrorViewModelExtensionTest
    {
        [TestMethod]
        public void HasError_ReturnsTrue_WhenErrorExists()
        {
            // Arrange
            var errors = new List<ErrorViewModel>
            {
                new ErrorViewModel { DOMElementId = "AcceptAll-Error", ErrorMessage = "Error message" }
            };

            // Act
            var result = errors.HasError("AcceptAll");

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void HasError_ReturnsFalse_WhenErrorDoesNotExist()
        {
            // Arrange
            var errors = new List<ErrorViewModel>
            {
                new ErrorViewModel { DOMElementId = "SomeOtherField", ErrorMessage = "Error message" }
            };

            // Act
            var result = errors.HasError("AcceptAll");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void HasError_ReturnsFalse_WhenErrorsListIsEmpty()
        {
            // Arrange
            var errors = new List<ErrorViewModel>();

            // Act
            var result = errors.HasError("AcceptAll");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void HasError_ReturnsFalse_WhenErrorsListIsNull()
        {
            // Arrange
            List<ErrorViewModel> errors = null;

            // Act
            var result = errors.HasError("AcceptAll");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void HasError_ReturnsFalse_WhenDomElementIdIsNull()
        {
            // Arrange
            var errors = new List<ErrorViewModel>
            {
                new ErrorViewModel { DOMElementId = "AcceptAll", ErrorMessage = "Error message" }
            };

            // Act
            var result = errors.HasError(null);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void HasError_ReturnsFalse_WhenDomElementIdIsEmpty()
        {
            // Arrange
            var errors = new List<ErrorViewModel>
            {
                new ErrorViewModel { DOMElementId = "AcceptAll", ErrorMessage = "Error message" }
            };

            // Act
            var result = errors.HasError(string.Empty);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void HasError_ReturnsFalse_WhenDomElementIdIsWhitespace()
        {
            // Arrange
            var errors = new List<ErrorViewModel>
            {
                new ErrorViewModel { DOMElementId = "AcceptAll", ErrorMessage = "Error message" }
            };

            // Act
            var result = errors.HasError("   ");

            // Assert
            Assert.IsFalse(result);
        }
    }
}