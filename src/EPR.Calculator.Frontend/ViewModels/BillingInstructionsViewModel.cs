using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels
{
    public record BillingInstructionsViewModel : ViewModelCommonData
    {
        public CalculationRunForBillingInstructionsDto CalculationRun { get; init; } = new();

        public PaginationViewModel TablePaginationModel { get; init; } = new();
    }
}