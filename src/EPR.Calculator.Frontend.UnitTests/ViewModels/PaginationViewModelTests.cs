using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.UnitTests.ViewModels
{
    [TestClass]
    public class PaginationViewModelTests
    {
        private static readonly int[] ExpectedPageSizes = { 10, 25, 50 };

        [TestMethod]
        public void Caption_ShouldBeInitializedCorrectly()
        {
            // Arrange
            var expectedCaption = "Test Caption";
            var viewModel = new PaginationViewModel
            {
                Caption = expectedCaption
            };

            // Act
            var actualCaption = viewModel.Caption;

            // Assert
            Assert.AreEqual(expectedCaption, actualCaption);
        }

        [TestMethod]
        public void TotalPages_ShouldCalculateCorrectly()
        {
            // Arrange
            var viewModel = new PaginationViewModel
            {
                TotalRecords = 55,
                PageSize = 10
            };

            // Act
            var totalPages = viewModel.TotalPages;

            // Assert
            Assert.AreEqual(6, totalPages);
        }

        [TestMethod]
        public void StartRecord_ShouldCalculateCorrectly()
        {
            // Arrange
            var viewModel = new PaginationViewModel
            {
                TotalRecords = 55,
                PageSize = 10,
                CurrentPage = 2
            };

            // Act
            var startRecord = viewModel.StartRecord;

            // Assert
            Assert.AreEqual(11, startRecord);
        }

        [TestMethod]
        public void EndRecord_ShouldCalculateCorrectly()
        {
            // Arrange
            var viewModel = new PaginationViewModel
            {
                TotalRecords = 55,
                PageSize = 10,
                CurrentPage = 2
            };

            // Act
            var endRecord = viewModel.EndRecord;

            // Assert
            Assert.AreEqual(20, endRecord);
        }

        [TestMethod]
        public void EndRecord_ShouldNotExceedTotalRecords()
        {
            // Arrange
            var viewModel = new PaginationViewModel
            {
                TotalRecords = 55,
                PageSize = 10,
                CurrentPage = 6
            };

            // Act
            var endRecord = viewModel.EndRecord;

            // Assert
            Assert.AreEqual(55, endRecord);
        }

        [TestMethod]
        public void PageSizeOptions_ShouldHaveDefaultValues()
        {
            // Arrange
            var viewModel = new PaginationViewModel();

            // Act
            var pageSizeOptions = viewModel.PageSizeOptions;

            // Assert
            CollectionAssert.AreEqual(ExpectedPageSizes, pageSizeOptions.ToList());
        }

        [TestMethod]
        public void StartRecord_ShouldReturnZero_WhenTotalRecordsIsZero()
        {
            // Arrange
            var viewModel = new PaginationViewModel
            {
                TotalRecords = 0,
                CurrentPage = 1,
                PageSize = 10
            };

            // Act
            var startRecord = viewModel.StartRecord;

            // Assert
            Assert.AreEqual(0, startRecord);
        }

        [TestMethod]
        public void StartRecord_ShouldReturnCorrectValue_WhenTotalRecordsIsGreaterThanZero()
        {
            // Arrange
            var viewModel = new PaginationViewModel
            {
                TotalRecords = 100,
                CurrentPage = 2,
                PageSize = 10
            };

            // Act
            var startRecord = viewModel.StartRecord;

            // Assert
            Assert.AreEqual(11, startRecord);
        }

        [TestMethod]
        public void DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange
            var viewModel = new PaginationViewModel();

            // Assert
            Assert.IsNotNull(viewModel.Records);
            Assert.AreEqual(Enumerable.Empty<object>(), viewModel.Records);
            Assert.AreEqual("index", viewModel.RouteName);
            Assert.IsNotNull(viewModel.RouteValues);
            Assert.AreEqual(0, viewModel.TotalRecords);
            Assert.AreEqual(0, viewModel.CurrentPage);
            Assert.AreEqual(0, viewModel.PageSize);
            CollectionAssert.AreEqual(ExpectedPageSizes, viewModel.PageSizeOptions.ToList());
        }
    }
}
