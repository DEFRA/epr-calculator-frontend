using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels.Enums;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// The view model for the calculator run Details page.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record CalculatorRunDetailsNewViewModel : ViewModelCommonData
    {
        /// <summary>
        /// Gets or sets the option for the calculation run.
        /// </summary>
        [Required(ErrorMessage = ErrorMessages.CalcRunOptionNotSelected)]
        public CalculationRunOption? SelectedCalcRunOption { get; set; }

        /// <summary>
        /// gets or sets calculator run details.
        /// </summary>
        public CalculatorRunDetailsViewModel CalculatorRunDetails { get; set; }

        /// <summary>
        /// Gets or sets download result URL.
        /// </summary>
        public Uri? DownloadResultURL { get; set; }

        /// <summary>
        /// Gets or sets download error URL.
        /// </summary>
        public string? DownloadErrorURL { get; set; }

        /// <summary>
        /// Gets or sets download Timeout.
        /// </summary>
        public int? DownloadTimeout { get; set; }

        /// <summary>
        /// Gets or sets the errors.
        /// </summary>
        public List<ErrorViewModel>? Errors { get; set; }
    }
}
