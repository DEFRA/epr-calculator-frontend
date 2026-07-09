using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;

namespace EPR.Calculator.Frontend.ViewModels;

public record AcceptRejectConfirmationViewModel : ViewModelCommonData
{
    public int CalculationRunId { get; set; }
    public string? CalculationRunName { get; set; }
    public required BillingStatus Status { get; set; }
    public bool? ApproveData { get; set; }
    public string? Reason { get; set; }

    public string AcceptRejectConfirmationText => Status switch
    {
        BillingStatus.Accepted => CommonConstants.AcceptViewText,
        BillingStatus.Rejected => CommonConstants.RejectViewText,
        _ => string.Empty
    };
}
