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
            int expectedId = 1;
            string expectedName = "Test Name";

            // Act
            var dto = new CalculationRunForBillingInstructionsDTO
            {
                Id = expectedId,
                Name = expectedName
            };

            // Assert
            Assert.AreEqual(expectedId, dto.Id);
            Assert.AreEqual(expectedName, dto.Name);
        }
    }
}
