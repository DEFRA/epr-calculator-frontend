using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.UnitTests.ViewModels
{
    [TestClass]
    public class PaginationRequestViewModelTests
    {
        [TestMethod]
        public void PaginationRequestViewModel_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var viewModel = new PaginationRequestViewModel();

            // Assert
            Assert.AreEqual(CommonConstants.DefaultPage, viewModel.Page);
            Assert.AreEqual(CommonConstants.DefaultPageSize, viewModel.PageSize);
            Assert.IsNull(viewModel.OrganisationId);
        }

        [TestMethod]
        public void PaginationRequestViewModel_CustomValues_ShouldBeSetCorrectly()
        {
            // Arrange
            var page = 2;
            var pageSize = 50;
            var organisationId = 123;

            // Act
            var viewModel = new PaginationRequestViewModel
            {
                Page = page,
                PageSize = pageSize,
                OrganisationId = organisationId
            };

            // Assert
            Assert.AreEqual(page, viewModel.Page);
            Assert.AreEqual(pageSize, viewModel.PageSize);
            Assert.AreEqual(organisationId, viewModel.OrganisationId);
        }
    }
}
