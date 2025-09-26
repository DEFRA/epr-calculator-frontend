using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class CalculatorRunClassificationDto
    {
        public int Id { get; set; }

        public string Status { get; set; } = null!;

        public string Description { get; set; } = null!;
    }
}
