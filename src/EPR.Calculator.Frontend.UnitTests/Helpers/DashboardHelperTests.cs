namespace EPR.Calculator.Frontend.UnitTests.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using EPR.Calculator.Frontend.Constants;
    using EPR.Calculator.Frontend.Enums;
    using EPR.Calculator.Frontend.Helpers;
    using EPR.Calculator.Frontend.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DashboardHelperTests
    {
        [TestMethod]
        public void CanGetCalulationRunsData()
        {
            // Arrange
            var calculationRuns = new List<CalculationRun>
            {
                new CalculationRun { Id = 1, CalculatorRunClassificationId = RunClassification.RUNNING, Name = "Alteration check", CreatedAt = DateTime.Parse("28/06/2025 12:19:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Financial_Year = "2024-25" },
                new CalculationRun { Id = 2, CalculatorRunClassificationId = RunClassification.UNCLASSIFIED, Name = "Test 10", CreatedAt = DateTime.Parse("21/06/2025 12:09:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Financial_Year = "2024-25" },
                new CalculationRun { Id = 3, CalculatorRunClassificationId = RunClassification.ERROR, Name = "Test 5", CreatedAt = DateTime.Parse("21/06/2025 12:09:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Financial_Year = "2024-25" },
            };

            // Act
            var result = DashboardHelper.GetCalulationRunsData(calculationRuns);

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(RunClassification.RUNNING, result[0].Status);
            Assert.AreEqual(RunClassification.UNCLASSIFIED, result[1].Status);
            Assert.AreEqual(RunClassification.ERROR, result[2].Status);
        }

        [TestMethod]
        public void CannotGetCalulationRunsDataWithNoCalculationRuns()
        {
            // Arrange
            var calculationRuns = new List<CalculationRun>();

            // Act
            var result = DashboardHelper.GetCalulationRunsData(calculationRuns);

            // Assert
            Assert.AreEqual(0, result.Count);
        }
    }
}