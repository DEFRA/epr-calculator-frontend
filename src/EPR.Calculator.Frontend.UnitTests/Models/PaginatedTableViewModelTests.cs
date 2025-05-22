using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.UnitTests.Models
{
    [TestClass]
    public class PaginatedTableViewModelTests
    {
        [TestMethod]
        public void Properties_AreSetCorrectly()
        {
            // Arrange
            var records = new List<object> { "Record1", "Record2" };
            var pageSizeOptions = new[] { 5, 10, 15 };

            // Act
            var model = new PaginatedTableViewModel
            {
                Records = records,
                Caption = "Test Caption",
                CurrentPage = 2,
                PageSize = 10,
                TotalRecords = 45,
                PageSizeOptions = pageSizeOptions
            };

            // Assert
            Assert.AreEqual(records, model.Records);
            Assert.AreEqual("Test Caption", model.Caption);
            Assert.AreEqual(2, model.CurrentPage);
            Assert.AreEqual(10, model.PageSize);
            Assert.AreEqual(45, model.TotalRecords);
            CollectionAssert.AreEqual(pageSizeOptions.ToList(), model.PageSizeOptions.ToList());
        }

        [TestMethod]
        public void PageSizeOptions_HasDefaultValues()
        {
            // Act
            var model = new PaginatedTableViewModel
            {
                Records = new List<object>(),
                Caption = string.Empty,
                CurrentPage = 1,
                PageSize = 10,
                TotalRecords = 0,
                // PageSizeOptions is not set explicitly
            };

            // Assert
            CollectionAssert.AreEqual(new[] { 10, 25, 50 }, model.PageSizeOptions.ToList());
        }

        [DataTestMethod]
        [DataRow(0, 10, 0)]
        [DataRow(5, 10, 1)]
        [DataRow(10, 10, 1)]
        [DataRow(11, 10, 2)]
        [DataRow(45, 10, 5)]
        [DataRow(50, 25, 2)]
        public void TotalPages_ComputedCorrectly(int totalRecords, int pageSize, int expectedTotalPages)
        {
            // Arrange
            var model = new PaginatedTableViewModel
            {
                TotalRecords = totalRecords,
                PageSize = pageSize,
                Records = new List<object>(),
                Caption = string.Empty,
                CurrentPage = 1,
            };

            // Act
            var totalPages = model.TotalPages;

            // Assert
            Assert.AreEqual(expectedTotalPages, totalPages);
        }
    }
}
