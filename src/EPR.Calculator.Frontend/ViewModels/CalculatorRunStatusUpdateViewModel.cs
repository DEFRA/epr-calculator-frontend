using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// The view model for the calculator run status update page.
    /// </summary>
    public record CalculatorRunStatusUpdateViewModel : ViewModelCommonData
    {
        /// <summary>
        /// Gets the data for the run status update.
        /// </summary>
        public required CalculatorRunStatusUpdateDto Data { get; init; }
    }
}
