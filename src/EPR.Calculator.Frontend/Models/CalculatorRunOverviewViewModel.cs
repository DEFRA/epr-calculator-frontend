using EPR.Calculator.Frontend.Models;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// The view model for the calculator run status update page.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record CalculatorRunOverviewViewModel : ViewModelCommonData
    {
        /// <summary>
        /// gets or sets calculator run details.
        /// </summary>
        public required CalculatorRunDetailsViewModel CalculatorRunDetails { get; set; }

        /// <summary>
        /// Gets or sets download result URL.
        /// </summary>
        public Uri? DownloadResultURL { get; set; }

        /// <summary>
        /// Gets or sets download Drafted Billing File URL.
        /// </summary>
        public Uri? DownloadCsvBillingURL { get; set; }

        /// <summary>
        /// Gets or sets download error URL.
        /// </summary>
        public string? DownloadErrorURL { get; set; }

        /// <summary>
        /// Gets or sets download Timeout.
        /// </summary>
        public int? DownloadTimeout { get; set; }
    }
}