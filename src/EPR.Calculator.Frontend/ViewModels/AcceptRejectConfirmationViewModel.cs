using EPR.Calculator.Frontend.Enums;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// The view model for accept reject confirmation view model.
    /// </summary>
    public record AcceptRejectConfirmationViewModel : ViewModelCommonData
    {
        /// <summary>
        /// gets or sets Calculation Run Id.
        /// </summary>
        public int CalculationRunId { get; set; }

        /// <summary>
        /// gets or sets Calculation Run Name.
        /// </summary>
        public string? CalculationRunName { get; set; }

        /// <summary>
        /// gets or sets the Accepted or Rejected reason.
        /// </summary>
        public required BillingStatus Status { get; set; }

        /// <summary>
        /// gets or sets Reason for rejection.
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// Gets the text for the accept or reject confirmation button based on the status.
        /// </summary>
        public string AcceptRejectConfirmationText => this.Status switch
        {
            BillingStatus.Accepted => "accept",
            BillingStatus.Rejected => "reject",
            _ => string.Empty,
        };
    }
}