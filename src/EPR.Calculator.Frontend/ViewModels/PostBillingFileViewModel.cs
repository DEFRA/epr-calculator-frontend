using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels
{
    public record PostBillingFileViewModel : ViewModelCommonData
    {
        public CalculatorRunPostBillingFileDto CalculatorRunStatus { get; set; } = null!;

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
        /// Gets or sets download billing URL.
        /// </summary>
        public Uri? DownloadCsvBillingURL { get; set; }
    }
}
