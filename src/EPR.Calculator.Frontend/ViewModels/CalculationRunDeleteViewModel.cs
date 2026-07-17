using System.ComponentModel.DataAnnotations;

namespace EPR.Calculator.Frontend.ViewModels;

public record CalculationRunDeleteViewModel : CalculationRunDeleteFormModel
{
    public required string RunName { get; init; }
}

public record CalculationRunDeleteFormModel
{
    [Range(1, int.MaxValue)] public required int RunId { get; init; }
}
