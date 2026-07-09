using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record BillingFileSuccessViewModel : ViewModelCommonData
{
    public required ConfirmationViewModel ConfirmationViewModel { get; init; }
}
