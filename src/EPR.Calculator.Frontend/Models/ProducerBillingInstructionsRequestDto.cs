namespace EPR.Calculator.Frontend.Models;

public record ProducerBillingInstructionsRequestDto
{
    public ProducerBillingInstructionsSearchQueryDto? SearchQuery { get; init; }

    public int? PageNumber { get; init; }

    public int? PageSize { get; init; }

    public bool FetchAll { get; set; }
}