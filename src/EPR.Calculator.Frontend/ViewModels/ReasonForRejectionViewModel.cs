using Pipelines.Sockets.Unofficial.Arenas;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace EPR.Calculator.Frontend.ViewModels
{
    public record ReasonForRejectionViewModel : ViewModelCommonData
    {
        public int CalculationRunId { get; set; }

        public string? CalculationRunName { get; set; }

        [Required(ErrorMessage = "Provide a reason that applies to all the billing instructions you selected for rejection.")]
        public required string Reason { get; set; }
    }
}
