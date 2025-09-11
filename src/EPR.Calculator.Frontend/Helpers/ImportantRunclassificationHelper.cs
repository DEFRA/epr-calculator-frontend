using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using Microsoft.Azure.Documents.SystemFunctions;
using System.Linq;

namespace EPR.Calculator.Frontend.Helpers
{
    public class ImportantRunclassificationHelper
    {
        public ImportantSectionViewModel CreateclassificationViewModel(List<ClassifiedCalculatorRunDto> classifiedCalculatorRuns, string financialYear)
        {
            ImportantSectionViewModel importantClassification = new ImportantSectionViewModel();

            if (classifiedCalculatorRuns.Any(x => x.RunClassificationId == (int)RunClassification.INITIAL_RUN
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
                importantClassification.HasAnyDesigRun = true;

                if (classifiedCalculatorRuns.Any(x => x.RunClassificationId == (int)RunClassification.INITIAL_RUN_COMPLETED))
                {
                    importantClassification.IsDisplayInitialRun = true;
                    ClassifiedCalculatorRunDto initialRunCompleted = classifiedCalculatorRuns.Where(x => x.RunClassificationId == (int)RunClassification.INITIAL_RUN_COMPLETED).OrderByDescending(x => x.UpdatedAt).First();
                    importantClassification.IsDisplayInitialRunMessage = "Already completed for financial year " + financialYear + " on " + initialRunCompleted.UpdatedAt?.ToString("dd MMM yyyy");
                }

                if (classifiedCalculatorRuns.Any(x => x.RunClassificationId == (int)RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED))
                {
                    importantClassification.IsDisplayInterimRun = true;
                    ClassifiedCalculatorRunDto interimRunCompleted = classifiedCalculatorRuns.Where(x => x.RunClassificationId == (int)RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED).OrderByDescending(x => x.UpdatedAt).First();
                    importantClassification.IsDisplayInterimRunMessage = "Already completed for financial year " + financialYear + " on " + interimRunCompleted.UpdatedAt?.ToString("dd MMM yyyy");
                }

                if (classifiedCalculatorRuns.Any(x => x.RunClassificationId == (int)RunClassification.FINAL_RECALCULATION_RUN_COMPLETED))
                {
                    importantClassification.IsDisplayFinalRecallRun = true;
                    ClassifiedCalculatorRunDto finalRecalRuncompleted = classifiedCalculatorRuns.Where(x => x.RunClassificationId == (int)RunClassification.FINAL_RECALCULATION_RUN_COMPLETED).OrderByDescending(x => x.UpdatedAt).First();
                    importantClassification.IsDisplayFinalRecallRunMessage = "Already completed for financial year " + financialYear + " on " + finalRecalRuncompleted.UpdatedAt?.ToString("dd MMM yyyy");
                }

                if (classifiedCalculatorRuns.Any(x => x.RunClassificationId == (int)RunClassification.FINAL_RUN_COMPLETED))
                {
                    importantClassification.IsDisplayFinalRun = true;
                    ClassifiedCalculatorRunDto finalCompleted = classifiedCalculatorRuns.Where(x => x.RunClassificationId == (int)RunClassification.FINAL_RUN_COMPLETED).OrderByDescending(x => x.UpdatedAt).First();
                    importantClassification.IsDisplayFinalRunMessage = "Already completed for financial year " + financialYear + " on " + finalCompleted.UpdatedAt?.ToString("dd MMM yyyy");

                    if (importantClassification.IsDisplayFinalRecallRun == false)
                    {
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