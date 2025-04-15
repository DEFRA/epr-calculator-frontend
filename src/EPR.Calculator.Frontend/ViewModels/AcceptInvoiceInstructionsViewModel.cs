using System.ComponentModel.DataAnnotations;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels
{
    public record AcceptInvoiceInstructionsViewModel : ViewModelCommonData
    {
        public int RunId { get; set; }

        public string CalculationRunTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = ErrorMessages.AcceptAllInstructionsNotChecked)]
        public bool AcceptAll { get; set; }

        public string ReturnUrl { get; set; } = string.Empty;

        public List<ErrorViewModel> Errors { get; set; } = [];
    }
}