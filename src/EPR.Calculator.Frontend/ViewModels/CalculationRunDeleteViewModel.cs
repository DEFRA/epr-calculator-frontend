using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record CalculationRunDeleteViewModel : ViewModelCommonData
{
    public required CalculatorRunStatusUpdateDto CalculatorRunStatusData { get; init; }
}
