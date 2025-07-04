using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.UnitTests.ViewModels
{
    [TestClass]
    public class BillingInstructionsViewModelTests
    {
        [TestMethod]
        public void BillingInstructionsViewModel_ShouldInitializeWithDefaultValues()
        {
            // Act
            var viewModel = new BillingInstructionsViewModel();

            // Assert
            Assert.IsNotNull(viewModel.CalculationRun);
            Assert.IsInstanceOfType(viewModel.CalculationRun, typeof(CalculationRunForBillingInstructionsDTO));

            Assert.IsNotNull(viewModel.TablePaginationModel);
            Assert.IsInstanceOfType(viewModel.TablePaginationModel, typeof(PaginationViewModel));
        }
    }
}