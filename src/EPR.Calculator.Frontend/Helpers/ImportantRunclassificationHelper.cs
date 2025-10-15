using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.Azure.Documents.SystemFunctions;
using System.Linq;

namespace EPR.Calculator.Frontend.Helpers
{
    public static class ImportantRunClassificationHelper
    {
        public static ImportantSectionViewModel CreateclassificationViewModel(List<ClassifiedCalculatorRunDto> classifiedCalculatorRuns, string financialYear)
        {
            ImportantSectionViewModel importantClassification = new ImportantSectionViewModel();

            if (classifiedCalculatorRuns.Exists(x => x.RunClassificationId == (int)RunClassification.INITIAL_RUN
            || x.RunClassificationId == (int)RunClassification.INTERIM_RECALCULATION_RUN
            || x.RunClassificationId == (int)RunClassification.FINAL_RECALCULATION_RUN
            || x.RunClassificationId == (int)RunClassification.FINAL_RUN))
            {
                importantClassification.IsAnyRunInProgress = true;
                importantClassification.HasAnyDesigRun = true;
                importantClassification.RunIdInProgress = classifiedCalculatorRuns.Where(x => x.RunClassificationId == (int)RunClassification.INITIAL_RUN
            || x.RunClassificationId == (int)RunClassification.INTERIM_RECALCULATION_RUN
            || x.RunClassificationId == (int)RunClassification.FINAL_RECALCULATION_RUN
            || x.RunClassificationId == (int)RunClassification.FINAL_RUN).OrderByDescending(x => x.UpdatedAt).First().RunId;
            }
            else
            {
                importantClassification.HasAnyDesigRun = false;

                if (classifiedCalculatorRuns.Exists(x => x.RunClassificationId == (int)RunClassification.INITIAL_RUN_COMPLETED))
                {
                    importantClassification.IsDisplayInitialRun = true;
                    importantClassification.HasAnyDesigRun = true;
                    ClassifiedCalculatorRunDto initialRunCompleted = classifiedCalculatorRuns.Where(x => x.RunClassificationId == (int)RunClassification.INITIAL_RUN_COMPLETED).OrderByDescending(x => x.UpdatedAt).First();
                    importantClassification.IsDisplayInitialRunMessage = "Already completed for financial year " + financialYear + " on " + initialRunCompleted.UpdatedAt?.ToString("dd MMM yyyy");
                }

                if (classifiedCalculatorRuns.Exists(x => x.RunClassificationId == (int)RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED))
                {
                    importantClassification.HasAnyDesigRun = true;
                    importantClassification.IsDisplayInterimRun = true;
                    ClassifiedCalculatorRunDto interimRunCompleted = classifiedCalculatorRuns.Where(x => x.RunClassificationId == (int)RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED).OrderByDescending(x => x.UpdatedAt).First();
                    importantClassification.IsDisplayInterimRunMessage = "Already completed for financial year " + financialYear + " on " + interimRunCompleted.UpdatedAt?.ToString("dd MMM yyyy");
                }

                if (classifiedCalculatorRuns.Exists(x => x.RunClassificationId == (int)RunClassification.FINAL_RECALCULATION_RUN_COMPLETED))
                {
                    importantClassification.HasAnyDesigRun = true;
                    importantClassification.IsDisplayFinalRecallRun = true;
                    ClassifiedCalculatorRunDto finalRecalRuncompleted = classifiedCalculatorRuns.Where(x => x.RunClassificationId == (int)RunClassification.FINAL_RECALCULATION_RUN_COMPLETED).OrderByDescending(x => x.UpdatedAt).First();
                    importantClassification.IsDisplayFinalRecallRunMessage = "Already completed for financial year " + financialYear + " on " + finalRecalRuncompleted.UpdatedAt?.ToString("dd MMM yyyy");
                }

                if (classifiedCalculatorRuns.Exists(x => x.RunClassificationId == (int)RunClassification.FINAL_RUN_COMPLETED))
                {
                    importantClassification.HasAnyDesigRun = true;
                    importantClassification.IsDisplayFinalRun = true;
                    ClassifiedCalculatorRunDto finalCompleted = classifiedCalculatorRuns.Where(x => x.RunClassificationId == (int)RunClassification.FINAL_RUN_COMPLETED).OrderByDescending(x => x.UpdatedAt).First();
                    importantClassification.IsDisplayFinalRunMessage = "Already completed for financial year " + financialYear + " on " + finalCompleted.UpdatedAt?.ToString("dd MMM yyyy");

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