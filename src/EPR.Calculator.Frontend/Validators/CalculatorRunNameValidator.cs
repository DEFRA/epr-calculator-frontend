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
            .Matches(AlphaNumericWithSpaces())
            .WithMessage(ErrorMessages.CalculationRunNameMustBeAlphaNumeric);
    }

    [GeneratedRegex("^[A-Za-z0-9 ]+$")] // Restricted because run names are used in generated filenames and '%'/'_' are reserved for API partial-search wildcards.
    private static partial Regex AlphaNumericWithSpaces();
}
