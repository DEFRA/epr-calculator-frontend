using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.UnitTests.Models
{
    [TestClass]
    public class CalculationRunForBillingInstructionsDtoTests
    {
        [TestMethod]
        public void CalculationRunForBillingInstructionsDTO_ShouldInitializeProperties()
        {
            // Arrange
            int expectedId = 1;
            string expectedName = "Test Name";

            // Act
            var dto = new CalculationRunForBillingInstructionsDto
            {
                Id = expectedId,
                Name = expectedName
            };

            // Assert
            Assert.AreEqual(expectedId, dto.Id);
            Assert.AreEqual(expectedName, dto.Name);

            Assert.AreEqual(dto.GetType(), typeof(CalculationRunForBillingInstructionsDto));
        }
    }
}
