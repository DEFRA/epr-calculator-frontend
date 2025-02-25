namespace EPR.Calculator.Frontend.Constants
{
    public static class ErrorMessages
    {
        public const string FileNotSelected = "Please select a file";
        public const string FileMustBeCSV = "Your file must have the extension .csv";
        public const string FileNotExceed50KB = "The CSV must be smaller than 50KB";
        public const string CalculationRunNameEmpty = "Enter a name for this calculation";
        public const string CalculationRunNameMaxLengthExceeded = "Calculation name must contain no more than 100 characters";
        public const string CalculationRunNameMustBeAlphaNumeric = "Calculation name must only contain numbers and letters";
        public const string CalculationRunNameExists = "There is already a calculation with this name";
        public const string SelectDeleteCalculation = "Select delete to continue or return to the dashboard";
        public const string DeleteCalculationError = "This file cannot be deleted";
        public const string UnknownUser = "Unknown User";
    }
}
