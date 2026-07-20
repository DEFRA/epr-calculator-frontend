using System.ComponentModel.DataAnnotations;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels.Enums;

namespace EPR.Calculator.Frontend.ViewModels;

public record CalculatorRunDetailsNewViewModel : CalculatorRunDetailsNewFormModel
{
    public required RunClassification RunClassification { get; init; }
    public required string RunName { get; init; }
    public required RelativeYear RelativeYear { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required string CreatedBy { get; init; }
}

public record CalculatorRunDetailsNewFormModel
{
    [Range(1, int.MaxValue)] public required int RunId { get; init; }

    [Required(ErrorMessage = ErrorMessages.CalcRunOptionNotSelected)]
    public CalculationRunOption? SelectedCalcRunOption { get; init; }
}
