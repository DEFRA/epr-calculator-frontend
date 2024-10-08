using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.ViewModels;
using FluentValidation;

namespace EPR.Calculator.Frontend.Validators
{
    public class CalculatorRunNameValidator : AbstractValidator<InitiateCalculatorRunModel>
    {
        public CalculatorRunNameValidator()
        {
            this.RuleFor(x => x.CalculationName)
            .NotEmpty().WithMessage(ErrorMessages.CalculationRunNameEmpty)
            .MaximumLength(100).WithMessage(ErrorMessages.CalculationRunNameMaxLengthExceeded)
            .Matches(StaticHelpers.AlphaNumeric).WithMessage(ErrorMessages.CalculationRunNameMustBeAlphaNumeric);
        }
    }
}
