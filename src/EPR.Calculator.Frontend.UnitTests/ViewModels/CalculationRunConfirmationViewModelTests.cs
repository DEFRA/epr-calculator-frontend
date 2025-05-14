using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.UnitTests.ViewModels
{
    [TestClass]
    public class CalculationRunConfirmationViewModelTests
    {
        [TestMethod]
        public void CalculationName_WhenSet_ReturnsExpectedValue()
        {
            // Arrange
            var viewModel = new CalculationRunConfirmationViewModel
            {
                CalculationName = "Sample Calculation"
            };

            // Act
            var result = viewModel.CalculationName;

            // Assert
            Assert.AreEqual("Sample Calculation", result);
        }
    }
}
