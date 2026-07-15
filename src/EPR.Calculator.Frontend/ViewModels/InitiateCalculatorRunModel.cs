using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record InitiateCalculatorRunModel
{
    public string? CalculationName { get; set; }
    public ErrorViewModel? Errors { get; set; }
}
