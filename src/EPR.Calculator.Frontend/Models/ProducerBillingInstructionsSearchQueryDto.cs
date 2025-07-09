namespace EPR.Calculator.Frontend.Models;

public record ProducerBillingInstructionsSearchQueryDto
{
    public int? OrganisationId { get; init; }

    public IEnumerable<string>? Status { get; init; } = new List<string>();
}