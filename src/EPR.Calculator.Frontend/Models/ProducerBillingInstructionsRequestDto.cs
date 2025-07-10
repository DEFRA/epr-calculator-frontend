namespace EPR.Calculator.Frontend.Models;

/// <summary>
/// Represents a request for paginated producer billing instructions, including optional search criteria.
/// </summary>
public record ProducerBillingInstructionsRequestDto
{
    /// <summary>
    /// Gets the search query criteria for filtering producer billing instructions.
    /// </summary>
    public ProducerBillingInstructionsSearchQueryDto? SearchQuery { get; init; }

    /// <summary>
    /// Gets the page number for pagination.
    /// </summary>
    public int? PageNumber { get; init; }

    /// <summary>
    /// Gets the page size for pagination.
    /// </summary>
    public int? PageSize { get; init; }
}