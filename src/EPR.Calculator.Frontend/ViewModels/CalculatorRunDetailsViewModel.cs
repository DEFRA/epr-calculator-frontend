using System.ComponentModel.DataAnnotations;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record CalculatorRunDetailsViewModel
{
    [Required] public int RunId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? RunName { get; set; }
    public string? CreatedBy { get; set; }
    public RunClassification RunClassificationId { get; set; }
    public string? RunClassificationStatus { get; set; }
    public RelativeYear RelativeYear { get; set; } = new (0);
    public bool? IsBillingFileGenerating { get; set; }
    public bool? IsBillingFileGeneratedLatest { get; set; }
}
