using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// View model used to hold the data for the billing file success page.
    /// </summary>
    public record BillingFileSuccessViewModel : ViewModelCommonData
    {
        /// <summary>
        /// Gets the confirmation view model for use in the partial.
        /// </summary>
        public required ConfirmationViewModel ConfirmationViewModel { get; init; }
    }
}
