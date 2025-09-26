using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class CalculatorRunDto
    {
        public int RunId { get; set; }

        public string CreatedBy { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public string RunName { get; set; } = null!;

        public string FileExtension { get; set; } = null!;

        public string UpdatedBy { get; set; } = null!;

        public DateTime? UpdatedAt { get; set; }

        public int RunClassificationId { get; set; }

        public string RunClassificationStatus { get; set; } = null!;

        public string? Classification { get; set; }

        public string FinancialYear { get; set; } = null!;
    }
}
