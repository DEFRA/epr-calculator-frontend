using System.ComponentModel.DataAnnotations;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record BillingInstructionsViewModel
{
    public required int RunId { get; init; }
    public required string RunName { get; init; }
    public required int? OrganisationId { get; init; }
    public required List<BillingInstruction> BillingInstructions { get; init; }
    public required List<BillingStatus> BillingStatuses { get; init; }
    public required Dictionary<BillingInstruction, int> InstructionCounts { get; init; }
    public required OrganisationSelectionsViewModel OrganisationSelections { get; init; } = new();
    public required ICollection<Organisation> SelectedRows { get; init; }
    public required PaginationViewModel TablePaginationModel { get; init; }
    public required IEnumerable<int>? ProducerIds { get; init; }
    public required Dictionary<BillingStatus, int> StatusCounts { get; init; }
    public required int TotalRecords { get; set; }
}

public record BillingInstructionsSelectModel : BillingInstructionsIndexModel
{
    [Range(1, int.MaxValue)] public required int RunId { get; init; }
    public OrganisationSelectionsViewModel OrganisationSelections { get; init; } = new();
}

public record BillingInstructionsClearModel : BasePaginationModel
{
    [Range(1, int.MaxValue)] public required int RunId { get; init; }
}


public record BillingInstructionsIndexModel : BasePaginationModel
{
    public int? OrganisationId { get; init; }
    public List<BillingStatus> BillingStatuses { get; init; } = [];
    public List<BillingInstruction> BillingInstructions { get; init; } = [];
}
