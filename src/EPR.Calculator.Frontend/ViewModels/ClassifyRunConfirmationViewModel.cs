using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record ClassifyRunConfirmationViewModel
{
    public required CalculatorRunDto CalculatorRunDetails { get; init; }
}
