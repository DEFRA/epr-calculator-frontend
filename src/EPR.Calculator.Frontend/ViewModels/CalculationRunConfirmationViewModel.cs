namespace EPR.Calculator.Frontend.ViewModels
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The view model for the create calculation run confirmation page.
    /// </summary>
    public record CalculationRunConfirmationViewModel
    {
        /// <summary>
        /// Gets or sets the calculation run name.
        /// </summary>
        public required string CalculationName { get; set; }
    }
}
