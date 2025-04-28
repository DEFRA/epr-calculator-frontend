using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using System.Reflection;
using System.Runtime.Serialization;

namespace EPR.Calculator.Frontend.Helpers
{
    public static class DashboardHelper
    {
        /// <summary>
        /// Processes a list of calculation runs and assigns status values based on their classifications.
        /// </summary>
        /// <param name="calculationRuns">The list of calculation runs to be processed.</param>
        /// <returns>A list of <see cref="CalculationRunViewModel"/> objects containing the processed data.</returns>
        public static List<CalculationRunViewModel> GetCalulationRunsData(List<CalculationRun> calculationRuns)
        {
            return calculationRuns
         .Where(x => x.CalculatorRunClassificationId != RunClassification.DELETED &&
                     x.CalculatorRunClassificationId != RunClassification.TEST_RUN &&
                     x.CalculatorRunClassificationId != RunClassification.INTHEQUEUE)
         .Select(calculationRun => new CalculationRunViewModel(calculationRun))
         .ToList();
        }
    }
}
