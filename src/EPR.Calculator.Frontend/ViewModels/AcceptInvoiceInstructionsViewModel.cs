using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Validators;

namespace EPR.Calculator.Frontend.ViewModels
{
    public record AcceptInvoiceInstructionsViewModel : ViewModelCommonData
    {
        public AcceptInvoiceInstructionsViewModel()
        {
            this.Errors = new List<ErrorViewModel>();
        }

        public int RunId { get; set; }

        public string CalculationRunTitle { get; set; } = string.Empty;

        [MustBeTrue(ErrorMessage = ErrorMessages.AcceptAllInstructionsNotChecked)]
        public bool AcceptAll { get; set; }
    }
}