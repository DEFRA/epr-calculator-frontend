using System.Text.RegularExpressions;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.ViewModels;
using FluentValidation;

namespace EPR.Calculator.Frontend.Validators;

public partial class CalculatorRunNameValidator : AbstractValidator<InitiateCalculatorRunFormModel>
{
    public CalculatorRunNameValidator()
    {
        RuleFor(x => x.CalculationName)
            .NotEmpty()
            .WithMessage(ErrorMessages.CalculationRunNameEmpty)
            .MaximumLength(100)
            .WithMessage(ErrorMessages.CalculationRunNameMaxLengthExceeded)
            .Matches(AlphaNumericWithAtLeastOneLetter())
            .WithMessage(ErrorMessages.CalculationRunNameMustBeAlphaNumeric);
    }

    [GeneratedRegex("^[A-Za-z0-9 ]*[A-Za-z][A-Za-z0-9 ]*$")]
    private static partial Regex AlphaNumericWithAtLeastOneLetter();
}
