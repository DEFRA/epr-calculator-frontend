using System.ComponentModel.DataAnnotations;

namespace EPR.Calculator.Frontend.ViewModels
{
    public record CalculatorRunDetailsViewModel
    {
        [Required]
        public int RunId { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? RunName { get; set; }

        public string? CreatedBy { get; set; }

        public int? RunClassificationId { get; set; }

        public string? RunClassificationStatus { get; set; }

        public string? FinancialYear { get; set; }
    }
}
