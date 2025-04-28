using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels
{
    public record PostBillingFileViewModel : ViewModelCommonData
    {
        public required string BillingFileSentDate { get; set; }

        public required string BillingFileRunBy { get; set; }

        public required string BillingFileSentBy { get; set; }

        public required string SelectedCalcRunOption { get; set; }

        public required CalculatorRunStatusUpdateDto CalculatorRunStatus { get; init; }

        /// <summary>
        /// Gets or sets download Timeout.
        /// </summary>
        public int? DownloadTimeout { get; set; }

        /// <summary>
        /// Gets or sets download result URL.
        /// </summary>
        public Uri? DownloadResultURL { get; set; }

        /// <summary>
        /// Gets or sets download error URL.
        /// </summary>
        public string? DownloadErrorURL { get; set; }

        /// <summary>
        /// Gets or sets download billing file name.
        /// </summary>
        public string? DownloadBillingFileName { get; set; }
    }
}
