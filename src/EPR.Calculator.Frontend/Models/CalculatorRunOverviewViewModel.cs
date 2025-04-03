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
        /// Gets or sets the title of the page.
        /// </summary>
        public string Title { get; init; } = "Index";

        /// <summary>
        /// Gets or sets the title of the Back Link.
        /// </summary>
        public string BackLink { get; init; } = string.Empty; // ToDo: will be update at the time of implementing the back link functionality.

        /// <summary>
        /// Gets the data for the run status update.
        /// </summary>
        public required CalculatorRunDto Data { get; init; }

        /// <summary>
        /// Gets or sets download result URL.
        /// </summary>
        public Uri? DownloadResultURL { get; set; }

        /// <summary>
        /// Gets or sets download Drafted Billing File URL.
        /// </summary>
        public Uri? DownloadDraftBillingURL { get; set; }

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