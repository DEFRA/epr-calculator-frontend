using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record PostBillingFileViewModel : ViewModelCommonData
{
    public CalculatorRunPostBillingFileDto CalculatorRunStatus { get; set; } = null!;
    public bool HideBackLink { get; set; }
}
