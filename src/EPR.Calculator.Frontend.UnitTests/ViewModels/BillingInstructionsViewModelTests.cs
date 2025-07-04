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
            Assert.IsInstanceOfType(viewModel.CalculationRun, typeof(CalculationRunForBillingInstructionsDto));

            Assert.IsNotNull(viewModel.TablePaginationModel);
            Assert.IsInstanceOfType(viewModel.TablePaginationModel, typeof(PaginationViewModel));

            Assert.AreEqual(viewModel.GetType(), typeof(BillingInstructionsViewModel));
        }

        [TestMethod]
        public void BillingInstructionsViewModel_ShouldInherit_ViewModelCommonData()
        {
            // Arrange & Act
            var viewModel = new BillingInstructionsViewModel();

            // Assert
            Assert.IsInstanceOfType(viewModel, typeof(ViewModelCommonData));
        }
    }
}