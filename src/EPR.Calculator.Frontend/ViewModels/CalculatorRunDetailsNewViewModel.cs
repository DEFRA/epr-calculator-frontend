using System.ComponentModel.DataAnnotations;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels.Enums;

namespace EPR.Calculator.Frontend.ViewModels;

public record CalculatorRunDetailsNewViewModel
{
    [Required(ErrorMessage = ErrorMessages.CalcRunOptionNotSelected)]
    public CalculationRunOption? SelectedCalcRunOption { get; set; }

    public required CalculatorRunDetailsViewModel CalculatorRunDetails { get; set; }
    public List<ErrorViewModel>? Errors { get; set; }
}
