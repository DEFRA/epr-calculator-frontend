using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.UnitTests.ViewModels
{
    [TestClass]
    public class BillingInstructionsViewModelTests
    {
        [TestMethod]
        public void BillingInstructionsViewModel_ShouldInitializeProperties()
        {
            // Arrange
            var calculationRun = new CalculationRunForBillingInstructionsDTO();
            var paginationModel = new PaginationViewModel();

            // Act
            var viewModel = new BillingInstructionsViewModel
            {
                CalculationRun = calculationRun,
                TablePaginationModel = paginationModel
            };

            // Assert
            Assert.AreEqual(calculationRun, viewModel.CalculationRun);
            Assert.AreEqual(paginationModel, viewModel.TablePaginationModel);
        }
    }
}