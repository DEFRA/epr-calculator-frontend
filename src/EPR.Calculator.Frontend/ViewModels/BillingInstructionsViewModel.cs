using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels
{
    public record BillingInstructionsViewModel : ViewModelCommonData
    {
        public CalculationRunForBillingInstructionsDTO CalculationRun { get; init; }

        public PaginationViewModel TablePaginationModel { get; init; }
    }

    public record OrganisationSelectionsViewModel
    {
        public List<int> SelectedOrganisationIds { get; init; } = new();
    }
}