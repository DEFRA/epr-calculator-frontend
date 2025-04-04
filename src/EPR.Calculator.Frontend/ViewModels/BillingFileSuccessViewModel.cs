using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// View model used to hold the data for the billing file success page.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record BillingFileSuccessViewModel : ViewModelCommonData
    {
        /// <summary>
        /// Gets or sets the confirmation view model for use in the partial.
        /// </summary>
        public ConfirmationViewModel ConfirmationViewModel { get; set; }
    }
}
