using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.ViewModels;
using FluentValidation;

namespace EPR.Calculator.Frontend.Validators;

public class AcceptRejectConfirmationFormModelValidator : AbstractValidator<AcceptRejectConfirmationFormModel>
{
    public AcceptRejectConfirmationFormModelValidator()
    {
        RuleFor(x => x.RunId)
            .GreaterThan(0)
            .WithMessage(ErrorMessages.CalculatorRunIdGreaterThanZero);

        RuleFor(x => x.Status)
            .NotNull()
            .IsInEnum()
            .WithMessage(ErrorMessages.StatusMustBeValid);

        RuleFor(x => x.ApproveData)
            .NotNull()
            .WithMessage(model => string.Format(ErrorMessages.AcceptRejectConfirmationApproveDataRequired,
                GetAcceptRejectText(model.Status)));

        // Reason is only required if status is Rejected
        When(x => x.Status == BillingStatus.Rejected, () =>
        {
            RuleFor(x => x.Reason)
                .NotEmpty()
                .WithMessage(ErrorMessages.ReasonForRejectionRequired)
                .MaximumLength(500)
                .WithMessage(ErrorMessages.ReasonMustNotExceed500Characters);
        });
    }

    private static string GetAcceptRejectText(BillingStatus? status)
    {
        return status switch
        {
            BillingStatus.Accepted => CommonConstants.AcceptViewText,
            BillingStatus.Rejected => CommonConstants.RejectViewText,
            _ => string.Empty
        };
    }
}
