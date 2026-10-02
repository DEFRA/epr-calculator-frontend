using System.ComponentModel.DataAnnotations;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record RemoveRunClassificationViewModel : RemoveRunClassificationFormModel
{
    public required CalculatorRunDto Run { get; init; }
}

public record RemoveRunClassificationFormModel
{
    [Range(1, int.MaxValue)] public required int RunId { get; init; }

    [Required(ErrorMessage = ErrorMessages.ClassifyRunTypeNotSelected)]
    public RunClassification? ClassifyRunType { get; set; }
}
