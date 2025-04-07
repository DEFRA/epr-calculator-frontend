namespace EPR.Calculator.Frontend.ViewModels
{
    using EPR.Calculator.Frontend.Models;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The view model for the create calculation run confirmation page.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record CalculationRunConfirmationViewModel
    {
        /// <summary>
        /// Gets or sets the calculation run name.
        /// </summary>
        public required string CalculationName { get; set; }
    }
}
