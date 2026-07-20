using System.ComponentModel.DataAnnotations;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record SetRunClassificationViewModel : SetRunClassificationFormModel
{
    public required RunClassification RunClassification { get; init; }
    public required string RunName { get; init; }
    public required RelativeYear RelativeYear { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required string CreatedBy { get; init; }
    public required RelativeYearClassificationResponseDto RelativeYearClassifications { get; init; }
    public required ImportantSectionViewModel ImportantViewModel { get; init; }
}

public record SetRunClassificationFormModel
{
    [Range(1, int.MaxValue)] public required int RunId { get; init; }

    [Required(ErrorMessage = ErrorMessages.ClassifyRunTypeNotSelected)]
    public int? ClassifyRunType { get; init; }
}
