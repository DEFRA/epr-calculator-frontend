using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class CalculationRun
    {
        public int Id { get; set; }

        public int CalculatorRunClassificationId { get; set; }

        public string Name { get; set; }

        public string Financial_Year { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Status { get; set; }
    }
}
