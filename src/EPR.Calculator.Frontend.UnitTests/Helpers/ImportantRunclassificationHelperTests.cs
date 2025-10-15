using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.UnitTests.Helpers
{
    [TestClass]
    public class ImportantRunClassificationHelperTests
    {
        [DataTestMethod]
        [DataRow(RunClassification.INITIAL_RUN, 101)]
        [DataRow(RunClassification.INTERIM_RECALCULATION_RUN, 102)]
        [DataRow(RunClassification.FINAL_RECALCULATION_RUN, 103)]
        [DataRow(RunClassification.FINAL_RUN, 104)]
        public void ShouldSetIsAnyRunInProgressWhenActiveRunExists(RunClassification classification, int runId)
        {
            var runs = new List<ClassifiedCalculatorRunDto>
    {
        CreateRun(classification, DateTime.UtcNow, runId)
    };

            var result = ImportantRunClassificationHelper.CreateclassificationViewModel(runs, "2025");

            Assert.IsTrue(result.IsAnyRunInProgress);
            Assert.IsTrue(result.HasAnyDesigRun);
            Assert.AreEqual(runId, result.RunIdInProgress);
        }

        [TestMethod]
        public void Should_Set_InterimRunCompletedMessage_When_InterimRunCompletedExists()
        {
            var date = new DateTime(2025, 5, 1);
            var runs = new List<ClassifiedCalculatorRunDto>
    {
        CreateRun(RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED, date)
    };

            var result = ImportantRunClassificationHelper.CreateclassificationViewModel(runs, "2025");

            Assert.IsTrue(result.IsDisplayInterimRun);
            StringAssert.Contains(result.IsDisplayInterimRunMessage, "01 May 2025");
        }

        [TestMethod]
        public void Should_Set_FinalRecalculationRunCompletedMessage_When_Exists()
        {
            var date = new DateTime(2025, 6, 1);
            var runs = new List<ClassifiedCalculatorRunDto>
    {
        CreateRun(RunClassification.FINAL_RECALCULATION_RUN_COMPLETED, date)
    };

            var result = ImportantRunClassificationHelper.CreateclassificationViewModel(runs, "2025");

            Assert.IsTrue(result.IsDisplayFinalRecallRun);
            StringAssert.Contains(result.IsDisplayFinalRecallRunMessage, "01 Jun 2025");
        }

        [TestMethod]
        public void Should_Set_FinalRunCompletedMessage_And_FallbackFinalRecallMessage_When_FinalRunExistsOnly()
        {
            var date = new DateTime(2025, 7, 1);
            var runs = new List<ClassifiedCalculatorRunDto>
    {
        CreateRun(RunClassification.FINAL_RUN_COMPLETED, date)
    };

            var result = ImportantRunClassificationHelper.CreateclassificationViewModel(runs, "2025");

            Assert.IsTrue(result.IsDisplayFinalRun);
            Assert.IsTrue(result.IsDisplayFinalRecallRun); // fallback
            StringAssert.Contains(result.IsDisplayFinalRunMessage, "01 Jul 2025");
            Assert.AreEqual("Not available after final run.", result.IsDisplayFinalRecallRunMessage);
        }

        [TestMethod]
        public void Should_Set_InitialRunCompletedMessage_When_InitialRunCompletedExists()
        {
            // Arrange
            var financialYear = "2025";
            var updatedAt = new DateTime(2025, 4, 1);
            var runs = new List<ClassifiedCalculatorRunDto>
    {
        new ClassifiedCalculatorRunDto
        {
            RunClassificationId = (int)RunClassification.INITIAL_RUN_COMPLETED,
            UpdatedAt = updatedAt,
            RunId = 1,
            RunName = "Initial Run Completed",
            CreatedAt = updatedAt.AddDays(-1)
        }
    };

            // Act
            var result = ImportantRunClassificationHelper.CreateclassificationViewModel(runs, financialYear);

            // Assert
            Assert.IsTrue(result.IsDisplayInitialRun);
            StringAssert.Contains(result.IsDisplayInitialRunMessage, "Already completed for financial year 2025 on 01 Apr 2025");
        }

        private static ClassifiedCalculatorRunDto CreateRun(RunClassification classification, DateTime updatedAt, int runId = 1)
        {
            return new ClassifiedCalculatorRunDto
            {
                RunClassificationId = (int)classification,
                UpdatedAt = updatedAt,
                RunId = runId,
                RunName = "Test Run",
                CreatedAt = updatedAt.AddDays(-1)
            };
        }
    }
}