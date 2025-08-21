namespace EPR.Calculator.Frontend.Models;

/// <summary>
/// Represents search criteria for filtering producer billing instructions.
/// </summary>
public record ProducerBillingInstructionsSearchQueryDto
{
    /// <summary>
    /// Gets the optional organisation ID to filter results by a specific organisation.
    /// </summary>
    public int? OrganisationId { get; init; }

    /// <summary>
    /// Gets the collection of status values to filter the billing instructions.
    /// </summary>
    public IEnumerable<string>? Status { get; init; } = new List<string>();
}