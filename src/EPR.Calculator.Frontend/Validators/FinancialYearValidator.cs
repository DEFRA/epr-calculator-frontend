using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.ViewModels;
using FluentValidation;

namespace EPR.Calculator.Frontend.Validators
{
    /// <summary>
    /// Class to handle the financial year validation.
    /// </summary>
    public class FinancialYearValidator : AbstractValidator<InitiateCalculatorRunModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialYearValidator"/> class.
        /// </summary>
        public FinancialYearValidator()
        {
            this.RuleFor(x => x.FinancialYear)
                .NotEmpty().WithMessage(ErrorMessages.FinancialYearEmpty)
                .Matches(StaticHelpers.FinancialYear).WithMessage(ErrorMessages.FinancialYearIncorrectFormat);
        }
    }
}
