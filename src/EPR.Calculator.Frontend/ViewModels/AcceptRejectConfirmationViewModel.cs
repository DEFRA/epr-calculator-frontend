namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// The view model for accept reject confirmation view model.
    /// </summary>
    public record AcceptRejectConfirmationViewModel : ViewModelCommonData
    {
        /// <summary>
        /// Gets or sets the run id.
        public int RunId { get; set; }

        /// <summary>
        /// Gets or sets the calculation run title.
        /// </summary>
        public string CalculationRunTitle { get; set; } = string.Empty;
    }
}