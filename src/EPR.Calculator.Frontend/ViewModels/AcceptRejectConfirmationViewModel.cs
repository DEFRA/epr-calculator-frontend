using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;

namespace EPR.Calculator.Frontend.ViewModels;

public record AcceptRejectConfirmationViewModel : AcceptRejectConfirmationFormModel
{
    public required string RunName { get; init; }

    public string AcceptRejectConfirmationText => Status switch
    {
        BillingStatus.Accepted => CommonConstants.AcceptViewText,
        BillingStatus.Rejected => CommonConstants.RejectViewText,
        _ => string.Empty
    };
}

// Validated by FluentValidation
public record AcceptRejectConfirmationFormModel
{
    public required int RunId { get; init; }
    public BillingStatus? Status { get; init; }
    public bool? ApproveData { get; init; }
    public string? Reason { get; init; }
}

// Validated by FluentValidation
public record ReasonForRejectionFormModel
{
    public required int RunId { get; init; }
    public string? Reason { get; init; }
}
