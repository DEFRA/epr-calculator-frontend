using System.ComponentModel.DataAnnotations;
using System.Globalization;
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

    public AcceptSuccessViewModel? AcceptSuccess { get; set; }
    public bool HasNoSelectionError { get; set; }
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

public record AcceptSuccessViewModel
{
    private string FormattedCount => Count.ToString("N0", CultureInfo.GetCultureInfo("en-GB"));
    private string OrganisationNameFormatted => string.IsNullOrWhiteSpace(OrganisationName) ? string.Empty : OrganisationName + " ";

    public int Count { get; init; }
    public int? OrganisationId { get; init; }
    public string? OrganisationName { get; init; }

    public string GetHeading() => Count switch
    {
        1 => $"{FormattedCount} billing instruction accepted",
        _ => $"{FormattedCount} billing instructions accepted"
    };

    public string GetBody(int pendingCount) => (pendingCount, FormattedCount) switch
    {
        (0, _) => "All billing decisions for this run have been saved.",
        (_, "1") => $"Your decision for {OrganisationNameFormatted}({OrganisationId}) has been saved.",
        _ => $"Your decisions for the {FormattedCount} selected organisations have been saved."
    };
}
