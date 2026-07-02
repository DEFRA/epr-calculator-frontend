using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// View model for displaying billing instructions, including calculation run details and paginated records.
    /// </summary>
    public record BillingInstructionsViewModel : ViewModelCommonData
    {
        public CalculationRunForBillingInstructionsDto CalculationRun { get; init; } = new();

        public int? OrganisationId { get; init; }

        public List<BillingInstruction> BillingInstructions { get; init; } = [];

        public List<BillingStatus> BillingStatuses { get; init; } = [];

        public Dictionary<BillingInstruction, int> InstructionCounts { get; init; } = [];

        public ICollection<Organisation> SelectedRows { get; init; } = [];

        public PaginationViewModel TablePaginationModel { get; init; } = new();

        public OrganisationSelectionsViewModel OrganisationSelections { get; set; } = new();

        public IEnumerable<int>? ProducerIds { get; init; }

        public Dictionary<BillingStatus, int> StatusCounts { get; init; } = [];

        public int TotalRecords { get; set; }
    }
}
