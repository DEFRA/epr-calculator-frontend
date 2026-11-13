using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record InitiateCalculatorRunViewModel : InitiateCalculatorRunFormModel
{
    public required RelativeYear RelativeYear { get; init; } 
    public ErrorViewModel? Errors { get; init; }
}

// Validated by FluentValidation
public record InitiateCalculatorRunFormModel
{
    public string? CalculationName { get; init; }
}
