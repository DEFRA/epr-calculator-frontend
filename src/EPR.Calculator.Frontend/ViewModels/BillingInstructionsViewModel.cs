using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// View model for displaying billing instructions, including calculation run details and paginated records.
    /// </summary>
    public record BillingInstructionsViewModel : ViewModelCommonData
    {
        /// <summary>
        /// Gets the calculation run details for the billing instructions.
        /// </summary>
        public CalculationRunForBillingInstructionsDto CalculationRun { get; init; } = new();

        /// <summary>
        /// Gets the pagination model containing the billing instruction records and pagination information.
        /// </summary>
        public PaginationViewModel TablePaginationModel { get; init; } = new();
    }
}