using EPR.Calculator.Frontend.Models;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// classify calculation run viewmodel.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record ClassifyCalculationRunViewModel : ViewModelCommonData
    {
        /// <summary>
        /// Gets or sets the errors.
        /// </summary>
        public ErrorViewModel? Errors { get; set; }

        /// <summary>
        /// Gets the data for the run status update.
        /// </summary>
        public required CalculatorRunStatusUpdateDto Data { get; init; }
    }
}