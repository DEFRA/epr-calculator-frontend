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
            Assert.IsNotNull(viewModel.SelectedOrganisationIds);
            Assert.AreEqual(0, viewModel.SelectedOrganisationIds.Count);
        }
    }
}
