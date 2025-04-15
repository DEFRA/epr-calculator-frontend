using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels
{
    public record AcceptInvoiceInstructionsViewModel : ViewModelCommonData
    {
        public int RunId { get; set; }

        public string CalculationRunTitle { get; set; } = string.Empty;

        public bool AcceptAll { get; set; }

        public string ReturnUrl { get; set; } = string.Empty;
    }
}