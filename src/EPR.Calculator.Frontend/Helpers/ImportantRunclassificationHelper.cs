using System.Collections.Immutable;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.Helpers
{
    public static class ImportantRunClassificationHelper
    {
        public static ImportantSectionViewModel CreateNotificationViewModel( ImmutableList<RunClassification> availableClassifications, ImmutableList<CalculatorRunDto> classifiedRuns, RelativeYear relativeYear)
        {
            var officialRuns = classifiedRuns.Where(x => x.RunClassification.IsOfficial).ToList();

            var viewModel = new ImportantSectionViewModel
            {
                RelativeYear = relativeYear,
                IncompleteOfficialRunId = officialRuns.OrderByDescending(x => x.UpdatedAt).FirstOrDefault()?.RunId,
                InitialRunCompletedAt = GetLatestSentAt(RunClassification.InitialCompleted),
                RecalculationCompletedAt = GetLatestSentAt(RunClassification.RecalculationCompleted)
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

            DateTime? GetLatestSentAt(RunClassification classification)
            {
                return officialRuns
                    .Where(x => x.RunClassification == classification)
                    .OrderByDescending(x => x.BillingFile!.SentAt)
                    .FirstOrDefault()
                    ?.BillingFile
                    ?.SentAt;
            }
        }
    }
}
