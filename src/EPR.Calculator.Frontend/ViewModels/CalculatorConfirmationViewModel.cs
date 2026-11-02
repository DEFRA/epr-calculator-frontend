using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record CalculatorConfirmationViewModel
{
    public required string CalculationName { get; init; }
    public required RelativeYear RelativeYear { get; init; }
}
