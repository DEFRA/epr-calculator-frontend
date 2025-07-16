using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.ViewModels;
using FluentValidation;

namespace EPR.Calculator.Frontend.Validators
{
    public class AcceptRejectConfirmationViewModelValidator : AbstractValidator<AcceptRejectConfirmationViewModel>
    {
        public AcceptRejectConfirmationViewModelValidator()
        {
            this.RuleFor(x => x.CalculationRunId)
                .GreaterThan(0).WithMessage(ErrorMessages.CalculatorRunIdGreaterThanZero);

            this.RuleFor(x => x.CalculationRunName)
                .NotEmpty().WithMessage(ErrorMessages.CalculationRunNameEmpty)
                .MaximumLength(100).WithMessage(ErrorMessages.CalculationRunNameMaxLengthExceeded);

            this.RuleFor(x => x.Status)
                .IsInEnum().WithMessage(ErrorMessages.StatusMustBeValid);

            this.RuleFor(x => x.ApproveData)
                .NotNull().WithMessage(ErrorMessages.AcceptRejectConfirmationApproveDataRequired);

            // Reason is only required if status is Rejected
            this.When(x => x.Status == BillingStatus.Rejected, () =>
            {
                this.RuleFor(x => x.Reason)
                    .NotEmpty().WithMessage(ErrorMessages.ReasonForRejectionRequired)
                    .MaximumLength(500).WithMessage(ErrorMessages.ReasonMustNotExceed500Characters);
            });
        }
    }
}