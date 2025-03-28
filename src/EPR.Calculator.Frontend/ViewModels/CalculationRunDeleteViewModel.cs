using EPR.Calculator.Frontend.Models;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// The view model for the calculation run delete page.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record CalculationRunDeleteViewModel : ViewModelCommonData
    {
        /// <summary>
        /// Gets the data for the run status update.
        /// </summary>
        public required CalculatorRunStatusUpdateDto Data { get; init; }
    }
}
