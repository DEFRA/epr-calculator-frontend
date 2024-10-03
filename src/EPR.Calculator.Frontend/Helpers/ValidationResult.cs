namespace EPR.Calculator.Frontend.Helpers
{
    public class ValidationResult
    {
        private ValidationResult(bool isValid, string errorMessage = null)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }

        public bool IsValid { get; }

        public string ErrorMessage { get; }

        public static ValidationResult Success() => new(true);

        public static ValidationResult Fail(string errorMessage) => new(false, errorMessage);
    }
}
