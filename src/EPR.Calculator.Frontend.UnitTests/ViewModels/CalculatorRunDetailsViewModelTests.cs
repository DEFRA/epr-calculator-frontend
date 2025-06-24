using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.UnitTests.ViewModels
{
    [TestClass]
    public class CalculatorRunDetailsViewModelTests
    {
        [TestMethod]
        public void Can_Create_CalculatorRunDetailsViewModel_With_Valid_Properties()
        {
            // Arrange
            var runId = 1;
            var createdAt = DateTime.UtcNow;
            var runName = "Test Run";
            var createdBy = "User1";
            var runClassificationId = RunClassification.TEST_RUN;
            var runClassificationStatus = "Completed";
            var financialYear = "2023/24";

            // Act
            var model = new CalculatorRunDetailsViewModel
            {
                RunId = runId,
                CreatedAt = createdAt,
                RunName = runName,
                CreatedBy = createdBy,
                RunClassificationId = runClassificationId,
                RunClassificationStatus = runClassificationStatus,
                FinancialYear = financialYear,
                IsBillingFileGenerating = true,
            };

            // Assert
            Assert.AreEqual(runId, model.RunId);
            Assert.AreEqual(createdAt, model.CreatedAt);
            Assert.AreEqual(runName, model.RunName);
            Assert.AreEqual(createdBy, model.CreatedBy);
            Assert.AreEqual(runClassificationId, model.RunClassificationId);
            Assert.AreEqual(runClassificationStatus, model.RunClassificationStatus);
            Assert.AreEqual(financialYear, model.FinancialYear);
            Assert.IsTrue(model.IsBillingFileGenerating);
        }

        [TestMethod]
        public void Default_Values_Are_Correct()
        {
            // Act
            var model = new CalculatorRunDetailsViewModel();

            // Assert
            Assert.AreEqual(0, model.RunId);
            Assert.AreEqual(default(DateTime), model.CreatedAt);
            Assert.IsNull(model.RunName);
            Assert.IsNull(model.CreatedBy);
            Assert.AreEqual(RunClassification.None, model.RunClassificationId);
            Assert.IsNull(model.RunClassificationStatus);
            Assert.IsNull(model.FinancialYear);
            Assert.IsNull (model.IsBillingFileGenerating);
        }
    }
}