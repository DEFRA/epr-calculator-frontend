using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record PostBillingFileViewModel
{
    public CalculatorRunPostBillingFileDto CalculatorRunStatus { get; set; } = null!;
    public bool HideBackLink { get; set; }
}
