namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// The view model for accept reject confirmation view model.
    /// </summary>
    public record AcceptRejectConfirmationViewModel : ViewModelCommonData
    {
        /// <summary>
        /// Gets the run id.
        public int RunId { get; init; }

        /// <summary>
        /// Gets the calculation run title.
        /// </summary>
        public string CalculationRunTitle { get; init; } = string.Empty;
    }
}