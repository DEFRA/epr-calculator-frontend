using System.ComponentModel.DataAnnotations;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// Reason for rejection view model.
    /// </summary>
    public record ReasonForRejectionViewModel : ViewModelCommonData
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
        /// gets or sets Reason for rejection.
        /// </summary>
        [Required(ErrorMessage = "Provide a reason that applies to all the billing instructions you selected for rejection.")]
        public required string Reason { get; set; }
    }
}
