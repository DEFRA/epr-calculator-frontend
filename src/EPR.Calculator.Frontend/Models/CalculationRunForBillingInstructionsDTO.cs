namespace EPR.Calculator.Frontend.Models
{
    public record CalculationRunForBillingInstructionsDto
    {
        public int Id { get; init; }

        public string Name { get; init; } = string.Empty;
    }
}
