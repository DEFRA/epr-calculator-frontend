namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// The view model for the calculation run error page.
    /// </summary>
    public record CalculationRunErrorViewModel : ViewModelCommonData
    {
        /// <summary>
        /// Gets the error message to be displayed on the page.
        /// </summary>
        public required string ErrorMessage { get; init; }
    }
}
