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
                CalcName = "Test Calculation",
                CreatedDate = "21 June 2024",
                CreatedTime = "12:09",
            };

            // Act & Assert
            Assert.AreEqual(1, dto.RunId);
            Assert.AreEqual(2, dto.ClassificationId);
            Assert.AreEqual("Test Calculation", dto.CalcName);
            Assert.AreEqual("21 June 2024", dto.CreatedDate);
            Assert.AreEqual("12:09", dto.CreatedTime);
        }

        [TestMethod]
        public void CalculatorRunStatusUpdateDto_CalcName_ShouldAllowNull()
        {
            // Arrange
            var dto = new CalculatorRunStatusUpdateDto
            {
                RunId = 1,
                ClassificationId = 2,
                CalcName = null,
                CreatedDate = null,
                CreatedTime = null,
            };

            // Act & Assert
            Assert.AreEqual(1, dto.RunId);
            Assert.AreEqual(2, dto.ClassificationId);
            Assert.IsNull(dto.CalcName);
        }
    }
}