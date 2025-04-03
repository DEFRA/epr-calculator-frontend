using System.Drawing.Printing;

namespace EPR.Calculator.Frontend.Models
{
    public class CalculatorRunDto
    {
        public int RunId { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public required string RunName { get; set; }

        public required string FileExtension { get; set; }

        public DateTime? UpdatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int RunClassificationId { get; set; }

        public required string RunClassificationStatus { get; set; }

        public string CreatedBy { get; set; }

        public string FinancialYear { get; set; }
    }
}
