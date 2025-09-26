using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.ViewModels
{
    [ExcludeFromCodeCoverage]

    /// <summary>
    /// The view model for the calculator run status update page.
    /// </summary>
    public record CalculatorRunStatusUpdateViewModel : ViewModelCommonData
    {
        /// <summary>
        /// Gets the data for the run status update.
        /// </summary>
        public required CalculatorRunStatusUpdateDto Data { get; init; }

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

        public ErrorViewModel Errors { get; set; } = null!;
    }
}
