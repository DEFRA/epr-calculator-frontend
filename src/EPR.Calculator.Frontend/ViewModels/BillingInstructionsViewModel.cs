using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels
{
    public record BillingInstructionsViewModel : ViewModelCommonData
    {
        public CalculationRunForBillingInstructionsDto CalculationRun { get; init; } = new();

        public ICollection<Organisation> OrganisationBillingInstructions { get; init; } = [];

        public PaginationViewModel TablePaginationModel { get; init; } = new();

        public bool SelectAll { get; set; }
    }
}