using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.UnitTests.Helpers
{
    [TestClass]
    public class ImportantRunclassificationHelperTests
    {
        private readonly ImportantRunclassificationHelper _helper = new();

        [TestMethod]
        public void ShouldSetIsAnyRunInProgressWhenActiveRunExists()
        {
            // Arrange
            var runs = new List<ClassifiedCalculatorRunDto>
            {
                CreateRun((int)RunClassification.INITIAL_RUN, DateTime.UtcNow, 101)
            };

            // Act
            var result = _helper.CreateclassificationViewModel(runs, "2025");

            // Assert
            Assert.IsTrue(result.IsAnyRunInProgress);
            Assert.IsTrue(result.HasAnyDesigRun);
            Assert.AreEqual(101, result.RunIdInProgress);
        }

        [TestMethod]
        public void Should_Set_InitialRunCompletedMessage_When_InitialRunCompletedExists()
        {
            // Arrange
            var date = new DateTime(2025, 4, 1);
            var runs = new List<ClassifiedCalculatorRunDto>
            {
                CreateRun((int)RunClassification.INITIAL_RUN_COMPLETED, date)
            };

            // Act
            var result = _helper.CreateclassificationViewModel(runs, "2025");

            // Assert
            Assert.IsTrue(result.IsDisplayInitialRun);
            Assert.IsTrue(result.HasAnyDesigRun);
            StringAssert.Contains(result.IsDisplayInitialRunMessage, "Already completed for financial year 2025 on 01 Apr 2025");
        }

        [TestMethod]
        public void Should_Set_InterimRunCompletedMessage_When_InterimRunCompletedExists()
        {
            var date = new DateTime(2025, 5, 1);
            var runs = new List<ClassifiedCalculatorRunDto>
            {
                CreateRun((int)RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED, date)
            };

            var result = _helper.CreateclassificationViewModel(runs, "2025");

            Assert.IsTrue(result.IsDisplayInterimRun);
            StringAssert.Contains(result.IsDisplayInterimRunMessage, "01 May 2025");
        }

        [TestMethod]
        public void Should_Set_FinalRecalculationRunCompletedMessage_When_Exists()
        {
            var date = new DateTime(2025, 6, 1);
            var runs = new List<ClassifiedCalculatorRunDto>
            {
                CreateRun((int)RunClassification.FINAL_RECALCULATION_RUN_COMPLETED, date)
            };

            var result = _helper.CreateclassificationViewModel(runs, "2025");

            Assert.IsTrue(result.IsDisplayFinalRecallRun);
            StringAssert.Contains(result.IsDisplayFinalRecallRunMessage, "01 Jun 2025");
        }

        [TestMethod]
        public void Should_Set_FinalRunCompletedMessage_And_FallbackFinalRecallMessage_When_FinalRunExistsOnly()
        {
            var date = new DateTime(2025, 7, 1);
            var runs = new List<ClassifiedCalculatorRunDto>
            {
                CreateRun((int)RunClassification.FINAL_RUN_COMPLETED, date)
            };

            var result = _helper.CreateclassificationViewModel(runs, "2025");

            Assert.IsTrue(result.IsDisplayFinalRun);
            Assert.IsTrue(result.IsDisplayFinalRecallRun); // fallback
            StringAssert.Contains(result.IsDisplayFinalRunMessage, "01 Jul 2025");
            Assert.AreEqual("Not available after final run.", result.IsDisplayFinalRecallRunMessage);
        }

        private static ClassifiedCalculatorRunDto CreateRun(int classificationId, DateTime updatedAt, int runId = 1)
        {
            return new ClassifiedCalculatorRunDto
            {
                RunClassificationId = classificationId,
                UpdatedAt = updatedAt,
                RunId = runId,
                RunName = "Test Run",
                CreatedAt = updatedAt.AddDays(-1)
            };
        }
    }
}