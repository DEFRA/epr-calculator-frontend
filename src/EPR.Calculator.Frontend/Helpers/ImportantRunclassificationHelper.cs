using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.Helpers
{
    public static class ImportantRunClassificationHelper
    {
        public static ImportantSectionViewModel CreateclassificationViewModel(List<CalculatorRunDto> classifiedCalculatorRuns, RelativeYear relativeYear)
        {
            ImportantSectionViewModel importantClassification = new ImportantSectionViewModel();

            if (classifiedCalculatorRuns.Exists(x => x.RunClassification
                    is RunClassification.INITIAL_RUN
                    or RunClassification.INTERIM_RECALCULATION_RUN
                    or RunClassification.FINAL_RECALCULATION_RUN
                    or RunClassification.FINAL_RUN))
            {
                importantClassification.IsAnyRunInProgress = true;
                importantClassification.HasAnyDesigRun = true;
                importantClassification.RunIdInProgress = classifiedCalculatorRuns.Where(x => x.RunClassification
                    is RunClassification.INITIAL_RUN
                    or RunClassification.INTERIM_RECALCULATION_RUN
                    or RunClassification.FINAL_RECALCULATION_RUN
                    or RunClassification.FINAL_RUN)
                    .OrderByDescending(x => x.UpdatedAt)
                    .First()
                    .RunId;
            }
            else
            {
                importantClassification.HasAnyDesigRun = false;

                if (classifiedCalculatorRuns.Exists(x => x.RunClassification == RunClassification.INITIAL_RUN_COMPLETED))
                {
                    importantClassification.IsDisplayInitialRun = true;
                    importantClassification.HasAnyDesigRun = true;
                    CalculatorRunDto initialRunCompleted = classifiedCalculatorRuns.Where(x => x.RunClassification == RunClassification.INITIAL_RUN_COMPLETED).OrderByDescending(x => x.UpdatedAt).First();
                    importantClassification.IsDisplayInitialRunMessage = "Already completed for financial year " + relativeYear + " on " + initialRunCompleted.UpdatedAt?.ToString("dd MMM yyyy");
                }

                if (classifiedCalculatorRuns.Exists(x => x.RunClassification == RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED))
                {
                    importantClassification.HasAnyDesigRun = true;
                    importantClassification.IsDisplayInterimRun = true;
                    CalculatorRunDto interimRunCompleted = classifiedCalculatorRuns.Where(x => x.RunClassification == RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED).OrderByDescending(x => x.UpdatedAt).First();
                    importantClassification.IsDisplayInterimRunMessage = "Already completed for financial year " + relativeYear + " on " + interimRunCompleted.UpdatedAt?.ToString("dd MMM yyyy");
                }

                if (classifiedCalculatorRuns.Exists(x => x.RunClassification == RunClassification.FINAL_RECALCULATION_RUN_COMPLETED))
                {
                    importantClassification.HasAnyDesigRun = true;
                    importantClassification.IsDisplayFinalRecallRun = true;
                    CalculatorRunDto finalRecalRuncompleted = classifiedCalculatorRuns.Where(x => x.RunClassification == RunClassification.FINAL_RECALCULATION_RUN_COMPLETED).OrderByDescending(x => x.UpdatedAt).First();
                    importantClassification.IsDisplayFinalRecallRunMessage = "Already completed for financial year " + relativeYear + " on " + finalRecalRuncompleted.UpdatedAt?.ToString("dd MMM yyyy");
                }

                if (classifiedCalculatorRuns.Exists(x => x.RunClassification == RunClassification.FINAL_RUN_COMPLETED))
                {
                    importantClassification.HasAnyDesigRun = true;
                    importantClassification.IsDisplayFinalRun = true;
                    CalculatorRunDto finalCompleted = classifiedCalculatorRuns.Where(x => x.RunClassification == RunClassification.FINAL_RUN_COMPLETED).OrderByDescending(x => x.UpdatedAt).First();
                    importantClassification.IsDisplayFinalRunMessage = "Already completed for financial year " + relativeYear + " on " + finalCompleted.UpdatedAt?.ToString("dd MMM yyyy");

                    if (!importantClassification.IsDisplayFinalRecallRun)
                    {
                        importantClassification.HasAnyDesigRun = true;

                        // They have skipped the final re-callculation-run and got to the final run
                        importantClassification.IsDisplayFinalRecallRun = true;
                        importantClassification.IsDisplayFinalRecallRunMessage = "Not available after final run.";
                    }
                }
            }

            return importantClassification;
        }
    }
}
