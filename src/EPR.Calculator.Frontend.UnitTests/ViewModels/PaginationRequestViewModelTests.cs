using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.UnitTests.ViewModels
{
    [TestClass]
    public class PaginationRequestViewModelTests
    {
        [TestMethod]
        public void DefaultConstructor_ShouldSetDefaultValues()
        {
            // Arrange & Act
            var model = new PaginationRequestViewModel();

            // Assert
            Assert.AreEqual(1, model.Page, "Default Page should be 1.");
            Assert.AreEqual(10, model.PageSize, "Default PageSize should be 10.");
        }

        [TestMethod]
        public void WithExpression_ShouldCreateModifiedCopy()
        {
            // Arrange
            var original = new PaginationRequestViewModel();

            // Act
            var modified = original with { Page = 2, PageSize = 25 };

            // Assert
            Assert.AreEqual(2, modified.Page, "Page should be updated to 2.");
            Assert.AreEqual(25, modified.PageSize, "PageSize should be updated to 25.");
            Assert.AreEqual(1, original.Page, "Original Page should remain 1.");
            Assert.AreEqual(10, original.PageSize, "Original PageSize should remain 10.");
        }
    }
}
