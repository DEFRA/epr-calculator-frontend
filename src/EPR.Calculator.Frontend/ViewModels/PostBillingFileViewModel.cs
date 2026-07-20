using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record PostBillingFileViewModel
{
    public required CalculatorRunDto CalculatorRunStatus { get; init; }
}
