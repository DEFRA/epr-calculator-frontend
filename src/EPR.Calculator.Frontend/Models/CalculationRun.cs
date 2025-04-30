using EPR.Calculator.Frontend.Enums;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class CalculationRun
    {
        public int Id { get; set; }

        public RunClassification CalculatorRunClassificationId { get; set; }

        public required string Name { get; set; }

        public required string Financial_Year { get; set; }

        public required string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}