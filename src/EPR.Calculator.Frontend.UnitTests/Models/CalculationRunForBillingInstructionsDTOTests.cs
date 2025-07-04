using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.UnitTests.Models
{
    [TestClass]
    public class CalculationRunForBillingInstructionsDTOTests
    {
        [TestMethod]
        public void CalculationRunForBillingInstructionsDTO_ShouldInitializeProperties()
        {
            // Arrange
            var id = 1;
            var name = "Test Name";

            // Act
            var dto = new CalculationRunForBillingInstructionsDTO
            {
                Id = id,
                Name = name
            };

            // Assert
            Assert.AreEqual(id, dto.Id);
            Assert.AreEqual(name, dto.Name);
        }
    }
}
