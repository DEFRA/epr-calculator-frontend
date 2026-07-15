using System.ComponentModel.DataAnnotations;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record SetRunClassificationViewModel
{
    public required CalculatorRunDetailsViewModel CalculatorRunDetails { get; set; }

    [Required(ErrorMessage = ErrorMessages.ClassifyRunTypeNotSelected)]
    public int? ClassifyRunType { get; set; }

    public string? SelectedCalcRunOption { get; set; }
    public RelativeYearClassificationResponseDto? RelativeYearClassifications { get; set; }
    public ClassificationStatusInformationViewModel? ClassificationStatusInformation { get; set; }
    public ImportantSectionViewModel ImportantViewModel { get; set; } = new ();
}
