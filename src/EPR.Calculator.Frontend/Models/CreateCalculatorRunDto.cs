using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    public record CreateCalculatorRunDto
    {
        public required string CalculatorRunName { get; init; }

        public required RelativeYear RelativeYear { get; init; }

        public required string CreatedBy { get; init; }
    }
}