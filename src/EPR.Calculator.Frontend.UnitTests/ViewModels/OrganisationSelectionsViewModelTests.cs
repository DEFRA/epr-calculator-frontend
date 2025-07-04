using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.UnitTests.ViewModels
{
    [TestClass]
    public class OrganisationSelectionsViewModelTests
    {
        [TestMethod]
        public void OrganisationSelectionsViewModel_ShouldInitializeSelectedOrganisationIds()
        {
            // Arrange & Act
            var viewModel = new OrganisationSelectionsViewModel();

            // Assert
            Assert.AreEqual(viewModel.GetType(), typeof(OrganisationSelectionsViewModel));
            Assert.IsNotNull(viewModel.SelectedOrganisationIds, "SelectedOrganisationIds should not be null.");
            Assert.AreEqual(0, viewModel.SelectedOrganisationIds.Count, "SelectedOrganisationIds should be initialized as an empty list.");
        }

        [TestMethod]
        public void OrganisationSelectionsViewModel_ShouldAllowImmutableSelectedOrganisationIds()
        {
            // Arrange
            var initialIds = new List<int> { 1, 2, 3 };
            var viewModel = new OrganisationSelectionsViewModel
            {
                SelectedOrganisationIds = initialIds
            };

            // Act
            var retrievedIds = viewModel.SelectedOrganisationIds;

            // Assert
            CollectionAssert.AreEqual(initialIds, retrievedIds, "SelectedOrganisationIds should match the initialized values.");
        }
    }
}
