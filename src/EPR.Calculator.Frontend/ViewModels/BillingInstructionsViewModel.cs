using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels
{
    public record BillingInstructionsViewModel : ViewModelCommonData
    {
        public CalculationRunForBillingInstructionsDTO CalculationRun { get; init; }

        public PaginationViewModel TablePaginationModel { get; init; }
    }
}