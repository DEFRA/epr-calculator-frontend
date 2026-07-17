using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.ViewModels;
using FluentValidation;

namespace EPR.Calculator.Frontend.Validators;

public class ReasonForRejectionFormModelValidator : AbstractValidator<ReasonForRejectionFormModel>
{
    public ReasonForRejectionFormModelValidator()
    {
        RuleFor(x => x.RunId)
            .GreaterThan(0)
            .WithMessage(ErrorMessages.CalculatorRunIdGreaterThanZero);

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage(ErrorMessages.ReasonForRejectionRequired)
            .MaximumLength(500)
            .WithMessage(ErrorMessages.ReasonMustNotExceed500Characters);
    }
}
