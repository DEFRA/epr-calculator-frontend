using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class CalculatorRunStatusUpdateDtoTests
    {
        [TestMethod]
        public void CalculatorRunStatusUpdateDto_Properties_ShouldSetAndGetCorrectly()
        {
            // Arrange
            var dto = new CalculatorRunStatusUpdateDto
            {
                RunId = 1,
                ClassificationId = 2,
                CalcName = "Test Calculation"
            };

            // Act & Assert
            Assert.AreEqual(1, dto.RunId);
            Assert.AreEqual(2, dto.ClassificationId);
            Assert.AreEqual("Test Calculation", dto.CalcName);
        }

        [TestMethod]
        public void CalculatorRunStatusUpdateDto_CalcName_ShouldAllowNull()
        {
            // Arrange
            var dto = new CalculatorRunStatusUpdateDto
            {
                RunId = 1,
                ClassificationId = 2,
                CalcName = null
            };

            // Act & Assert
            Assert.AreEqual(1, dto.RunId);
            Assert.AreEqual(2, dto.ClassificationId);
            Assert.IsNull(dto.CalcName);
        }
    }
}