using EPR.Calculator.Frontend.Models;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// The view model for the calculation run delete page.
    /// </summary>
    public record CalculationRunDeleteViewModel : ViewModelCommonData
    {
        /// <summary>
        /// Gets the calculator run status data for the run status update.
        /// </summary>
        public required CalculatorRunStatusUpdateDto CalculatorRunStatusData { get; init; }
    }
}
