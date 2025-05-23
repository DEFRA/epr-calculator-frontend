using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.UnitTests.Models
{
    [TestClass]
    public class PaginatedTableViewModelTests
    {
        [TestMethod]
        public void Default_PageSizeOptions_ShouldContain_10_25_50()
        {
            // Arrange
            var model = new PaginatedTableViewModel();

            // Act
            var options = model.PageSizeOptions;

            // Assert
            CollectionAssert.AreEqual(new[] { 10, 25, 50 }, options.ToArray());
        }

        [DataTestMethod]
        [DataRow(0, 10, 0)]
        [DataRow(1, 10, 1)]
        [DataRow(9, 10, 1)]
        [DataRow(10, 10, 1)]
        [DataRow(11, 10, 2)]
        [DataRow(100, 25, 4)]
        public void TotalPages_ShouldCalculateCorrectly(int totalRecords, int pageSize, int expectedTotalPages)
        {
            // Arrange
            var model = new PaginatedTableViewModel
            {
                TotalRecords = totalRecords,
                PageSize = pageSize
            };

            // Act & Assert
            Assert.AreEqual(expectedTotalPages, model.TotalPages);
        }

        [TestMethod]
        public void ShouldPreserveAllValues_WhenUsingWithExpression()
        {
            // Arrange
            var original = new PaginatedTableViewModel
            {
                Records = new List<object> { "a", "b" },
                Caption = "Test Caption",
                CurrentPage = 2,
                PageSize = 10,
                TotalRecords = 50,
                StartRecord = 11,
                EndRecord = 20
            };

            // Act
            var modified = original with { CurrentPage = 3, PageSize = 25 };

            // Assert
            Assert.AreEqual(3, modified.CurrentPage);
            Assert.AreEqual(25, modified.PageSize);
            Assert.AreEqual("Test Caption", modified.Caption);
            Assert.AreEqual(2, original.CurrentPage); // Original remains unchanged
            Assert.AreEqual(10, original.PageSize);
        }
    }
}
