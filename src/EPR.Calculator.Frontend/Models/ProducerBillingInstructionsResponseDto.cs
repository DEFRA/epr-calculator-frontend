using System.Text.Json.Serialization;

namespace EPR.Calculator.Frontend.Models;

/// <summary>
/// Represents a paginated response containing producer billing instructions for a calculation run.
/// </summary>
public record ProducerBillingInstructionsResponseDto
{
    /// <summary>
    /// Gets the list of producer billing instruction records.
    /// </summary>
    [JsonPropertyName("records")]
    public List<ProducerBillingInstructionsDto> Records { get; init; } = new();

    /// <summary>
    /// Gets the total number of records available for the query.
    /// </summary>
    [JsonPropertyName("totalRecords")]
    public int TotalRecords { get; init; }

    /// <summary>
    /// Gets the unique identifier for the calculation run.
    /// </summary>
    [JsonPropertyName("calculatorRunId")]
    public int CalculatorRunId { get; init; }

    /// <summary>
    /// Gets the name of the calculation run.
    /// </summary>
    [JsonPropertyName("runName")]
    public string? RunName { get; init; }

    /// <summary>
    /// Gets the current page number in the paginated response.
    /// </summary>
    [JsonPropertyName("pageNumber")]
    public int? PageNumber { get; init; }

    /// <summary>
    /// Gets the number of records per page in the paginated response.
    /// </summary>
    [JsonPropertyName("pageSize")]
    public int? PageSize { get; init; }
}