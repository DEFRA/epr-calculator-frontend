namespace EPR.Calculator.Frontend.ViewModels;

public record CalculationRunErrorViewModel : ViewModelCommonData
{
    public required string ErrorMessage { get; init; }
}
