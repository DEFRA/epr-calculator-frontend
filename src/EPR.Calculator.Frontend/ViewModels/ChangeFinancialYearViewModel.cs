using System.ComponentModel.DataAnnotations;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record ChangeFinancialYearViewModel : ChangeFinancialYearFormModel
{
    public required List<RelativeYear> RelativeYears { get; init; }
}

public record ChangeFinancialYearFormModel
{
    [Required(ErrorMessage = ErrorMessages.FinancialYearNotSelected)]
    public int? RelativeYear { get; init; }
}
