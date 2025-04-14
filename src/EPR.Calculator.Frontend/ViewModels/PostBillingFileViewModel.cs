using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels
{
    public record PostBillingFileViewModel : ViewModelCommonData
    {
        public required string BillingFileSentDate { get; set; }

        public required string BillingFileRunBy { get; set; }

        public required string BillingFileSentBy { get; set; }

        public string? SelectedCalcRunOption { get; set; }

        public required CalculatorRunStatusUpdateDto CalculatorRunStatus { get; init; }
    }
}
