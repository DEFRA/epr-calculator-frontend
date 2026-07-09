using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record InitiateCalculatorRunModel : ViewModelCommonData
{
    public string? CalculationName { get; set; }
    public ErrorViewModel? Errors { get; set; }
}
