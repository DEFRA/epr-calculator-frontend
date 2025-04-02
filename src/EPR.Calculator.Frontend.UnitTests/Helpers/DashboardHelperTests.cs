namespace EPR.Calculator.Frontend.UnitTests.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using EPR.Calculator.Frontend.Constants;
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
                new CalculationRun { Id = 1, CalculatorRunClassificationId = 2, Name = "Alteration check", CreatedAt = DateTime.Parse("28/06/2025 12:19:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Running, Financial_Year = "2024-25" },
                new CalculationRun { Id = 2, CalculatorRunClassificationId = 3, Name = "Test 10", CreatedAt = DateTime.Parse("21/06/2025 12:09:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Unclassified, Financial_Year = "2024-25" },
                new CalculationRun { Id = 3, CalculatorRunClassificationId = 5, Name = "Test 5", CreatedAt = DateTime.Parse("21/06/2025 12:09:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Error, Financial_Year = "2024-25" },
            };

            // Act
            var result = DashboardHelper.GetCalulationRunsData(calculationRuns);

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(CalculationRunStatus.Running, result[0].Status);
            Assert.AreEqual(CalculationRunStatus.Unclassified, result[1].Status);
            Assert.AreEqual(CalculationRunStatus.Error, result[2].Status);
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