namespace EPR.Calculator.Frontend.Constants
{
    public static class ClassifyCalculationRunStatusInformation
    {
        public const string RunStatusDescription = "Already classified for financial year {0} on Date Info.";
        public const string InterimReCalculationStatusDescription = "An optional run, only available after the initial run.";
        public const string FinalRecalculationStatusDescription = "An optional run, only available if no other final recalculation or later run has been classified this year.";
        public const string FinalRunStatusDescription = "A mandatory run, only available if no other final run or later run has been classified this year.";
    }
}
