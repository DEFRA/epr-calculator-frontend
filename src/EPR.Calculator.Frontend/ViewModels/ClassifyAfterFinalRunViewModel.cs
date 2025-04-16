using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// classify calculation run scenerio4 view model.
    /// </summary>
    public record ClassifyAfterFinalRunViewModel : ViewModelCommonData
    {
        /// <summary>
        /// Gets or sets the errors.
        /// </summary>
        public ErrorViewModel? Errors { get; set; }

        /// <summary>
        /// Gets the data for the run status update.
        /// </summary>
        public required CalculatorRunStatusUpdateDto CalculatorRunStatus { get; init; }

        /// <summary>
        /// Gets or sets the selected calculation run option.
        /// </summary>
        public string? SelectedCalcRunOption { get; set; }
    }
}
