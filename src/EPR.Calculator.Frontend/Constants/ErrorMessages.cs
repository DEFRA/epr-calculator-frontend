namespace EPR.Calculator.Frontend.Constants
{
    /// <summary>
    /// List of error messages.
    /// </summary>
    public static class ErrorMessages
    {
        /// <summary>
        /// Error Title.
        /// </summary>
        public const string ErrorSummaryTitle = "There is a problem";

        /// <summary>
        /// Error message for calculation run name not provided.
        /// </summary>
        public const string CalculationRunNameEmpty = "Enter a name for this calculation";

        /// <summary>
        /// Error message for calculation run name exists.
        /// </summary>
        public const string CalculationRunNameExists = "There is already a calculation with this name";

        /// <summary>
        /// Error message for calculation run name maximum length exceeded.
        /// </summary>
        public const string CalculationRunNameMaxLengthExceeded = "Calculation name must contain no more than 100 characters";

        /// <summary>
        /// Error message for calculation run name must be alphanumeric.
        /// </summary>
        public const string CalculationRunNameMustBeAlphaNumeric = "Calculation name must only contain numbers and letters";

        /// <summary>
        /// Error message for file cannot be deleted.
        /// </summary>
        public const string DeleteCalculationError = "This file cannot be deleted";

        /// <summary>
        /// Error message for file must be CSV.
        /// </summary>
        public const string FileMustBeCSV = "Your file must have the extension .csv";

        /// <summary>
        /// Error message for file must not exceed 50KB.
        /// </summary>
        public const string FileNotExceed50KB = "The CSV must be smaller than 50KB";

        /// <summary>
        /// Error message for file not selected.
        /// </summary>
        public const string FileNotSelected = "Please select a file";

        /// <summary>
        /// Error message for delete calculation run option not selected.
        /// </summary>
        public const string SelectDeleteCalculation = "Select delete to continue or return to the dashboard";

        /// <summary>
        /// Error message for unknown user.
        /// </summary>
        public const string UnknownUser = "Unknown User";

        /// <summary>
        /// Error message for when user doesn't accept all invoice instructions.
        /// </summary>
        public const string AcceptAllInstructionsNotChecked = "Confirm that you have accepted all billing instructions before continuing.";

        /// <summary>
        /// Error message for classify run type option not selected.
        /// </summary>
        public const string ClassifyRunTypeNotSelected = "Select a classification before confirming.";
    }
}
