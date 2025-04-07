using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// classify calculation run scenerio1 view model.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record ClassifyCalculationRunScenerio1ViewModel : ViewModelCommonData
    {
        /// <summary>
        /// Gets or sets the back link.
        /// </summary>
        public string BackLink { get; set; } = CommonConstants.DashBoard;

        /// <summary>
        /// Gets or sets the errors.
        /// </summary>
        public ErrorViewModel? Errors { get; set; }

        /// <summary>
        /// Gets the data for the run status update.
        /// </summary>
        public required CalculatorRunStatusUpdateDto CalculatorRunStatus { get; init; }
    }
}