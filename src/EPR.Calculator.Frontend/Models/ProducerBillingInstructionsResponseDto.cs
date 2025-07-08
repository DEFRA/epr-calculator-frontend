using System.Text.Json.Serialization;

namespace EPR.Calculator.Frontend.Models;

public record ProducerBillingInstructionsResponseDto
{
    [JsonPropertyName("records")]
    public List<ProducerBillingInstructionsDto> Records { get; init; } = new();

    [JsonPropertyName("totalRecords")]
    public int TotalRecords { get; init; }

    [JsonPropertyName("calculatorRunId")]
    public int CalculatorRunId { get; init; }

    [JsonPropertyName("runName")]
    public string? RunName { get; init; }

    [JsonPropertyName("pageNumber")]
    public int? PageNumber { get; init; }

    [JsonPropertyName("pageSize")]
    public int? PageSize { get; init; }
}