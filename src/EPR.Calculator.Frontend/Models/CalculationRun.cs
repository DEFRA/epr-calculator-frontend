using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class CalculationRun
    {
        public int Id { get; set; }

        public int CalculatorRunClassificationId { get; set; }

        public required string Name { get; set; }

        public required string Financial_Year { get; set; }

        public required string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public required string Status { get; set; }
    }
}
