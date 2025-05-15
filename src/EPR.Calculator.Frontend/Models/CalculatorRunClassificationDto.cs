namespace EPR.Calculator.Frontend.Models
{
    public record CalculatorRunClassificationDto
    {
        public int Id { get; init; }

        public string Status { get; set; }

        public string Description { get; set; }
    }
}
