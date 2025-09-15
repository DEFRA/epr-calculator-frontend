using EPR.Calculator.Frontend.Enums;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.ViewModels
{
    public record CalculatorRunDetailsViewModel
    {
        [Required]
        public int RunId { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? RunName { get; set; }

        public string? CreatedBy { get; set; }

        public RunClassification RunClassificationId { get; set; }

        public string? RunClassificationStatus { get; set; }

        public string? FinancialYear { get; set; }

        /// <summary>
        /// gets or sets the billing file generating flag.
        /// </summary>
        public bool? IsBillingFileGenerating { get; set; }

        public bool IsBillingFileGeneratedLatest { get; set; }
    }
}