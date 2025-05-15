using System.Drawing.Printing;

namespace EPR.Calculator.Frontend.Models
{
    public class CalculatorRunDto
    {
        public int RunId { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public string RunName { get; set; }

        public string FileExtension { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int RunClassificationId { get; set; }

        public string RunClassificationStatus { get; set; }

        public string? Classification { get; set; }

        public string FinancialYear { get; set; }
    }
}
