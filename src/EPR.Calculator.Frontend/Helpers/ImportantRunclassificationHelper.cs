using System.Collections.Immutable;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.Helpers
{
    public static class ImportantRunClassificationHelper
    {
        public static ImportantSectionViewModel CreateNotificationViewModel(
            ImmutableList<RunClassification> availableClassifications,
            ImmutableList<CalculatorRunDto> classifiedRuns,
            RelativeYear relativeYear)
        {
            var officialRuns = classifiedRuns
                .Where(run => run.RunClassification is RunClassification.Initial or RunClassification.Recalculation)
                .ToList();

            var completedRuns = officialRuns.Where(run => run.BillingFile?.IsShared == true).ToList();
            var incompleteRuns = officialRuns.Except(completedRuns).ToList();

            var viewModel = new ImportantSectionViewModel
            {
                RelativeYear = relativeYear,
                IncompleteOfficialRunId = incompleteRuns.OrderByDescending(run => run.UpdatedAt).FirstOrDefault()?.RunId,
                InitialRunCompletedAt = GetLatestShared(RunClassification.Initial),
                RecalculationCompletedAt = GetLatestShared(RunClassification.Recalculation)
            };

            if (availableClassifications is [RunClassification.Test])
            {
                if(viewModel.IncompleteOfficialRunId != null)
                    return viewModel with { Notification = ImportantSectionViewModel.NotificationType.TestOnlyIncompleted };

                return viewModel with { Notification = ImportantSectionViewModel.NotificationType.TestOnlyOutdated };
            }

            if (viewModel.InitialRunCompletedAt != null || viewModel.RecalculationCompletedAt != null)
            {
                return viewModel with { Notification = ImportantSectionViewModel.NotificationType.AlreadyCompleted };
            }

            return viewModel;

            DateTime? GetLatestShared(RunClassification classification)
            {
                return completedRuns
                    .Where(x => x.RunClassification == classification)
                    .OrderByDescending(x => x.BillingFile!.SharedAt)
                    .FirstOrDefault()
                    ?.BillingFile
                    ?.SharedAt;
            }
        }
    }
}
