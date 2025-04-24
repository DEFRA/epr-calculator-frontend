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
            var runClassifications = Enum.GetValues(typeof(RunClassification)).Cast<RunClassification>().ToList();
            var dashboardRunData = new List<CalculationRunViewModel>();

            if (calculationRuns.Count > 0)
            {
                var displayRuns = calculationRuns.Where(x =>
                    x.CalculatorRunClassificationId != (int)RunClassification.DELETED &&
                    x.CalculatorRunClassificationId != (int)RunClassification.PLAY &&
                    x.CalculatorRunClassificationId != (int)RunClassification.QUEUE);
                foreach (var calculationRun in displayRuns)
                {
                    var classificationVal = runClassifications.Find(c => (int)c == calculationRun.CalculatorRunClassificationId);
                    var member = typeof(RunClassification).GetTypeInfo().DeclaredMembers.SingleOrDefault(x => x.Name == classificationVal.ToString());

                    var attribute = member?.GetCustomAttribute<EnumMemberAttribute>(false);

                    calculationRun.Status = attribute?.Value ?? string.Empty; // Use a default value if attribute or value is null

                    dashboardRunData.Add(new CalculationRunViewModel(calculationRun));
                }
            }

            return dashboardRunData;
        }

        /// <summary>
        /// Turn on feature urls based on classification status.
        /// </summary>
        /// <param name="status">classification status.</param>
        /// <param name="id">run id.</param>
        /// <returns>calculation run url.</returns>
        public static string GetTurnOnFeatureUrl(string status, int id)
        {
            return status switch
            {
                CalculationRunStatus.Unclassified => string.Format(ActionNames.ViewCalculationRunNewDetails, id),
                _ => ControllerNames.Dashboard,
            };
        }
    }
}
