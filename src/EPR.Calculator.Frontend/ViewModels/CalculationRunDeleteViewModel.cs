using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record CalculationRunDeleteViewModel
{
    public required CalculatorRunStatusUpdateDto CalculatorRunStatusData { get; init; }
}
