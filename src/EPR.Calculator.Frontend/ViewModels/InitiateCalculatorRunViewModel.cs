namespace EPR.Calculator.Frontend.ViewModels;

public record InitiateCalculatorRunViewModel : InitiateCalculatorRunFormModel
{
    public ErrorViewModel? Errors { get; init; }
}

// Validated by FluentValidation
public record InitiateCalculatorRunFormModel
{
    public string? CalculationName { get; init; }
}
