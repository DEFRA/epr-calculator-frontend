using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class CalculationRun
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CreatedBy {  get; set; }

        public string Status { get; set; }

        public string? tagStyle { get; set; }
    }
}
