using System.ComponentModel.DataAnnotations;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Validators;

namespace EPR.Calculator.Frontend.ViewModels;

public record SendBillingFileViewModel : SendBillingFileFormModel
{
    public required string CalcRunName { get; init; }
    public string SendBillFileHeading { get; init; } = CommonConstants.SendBillingFile;
    public string ConfirmationContent { get; init; } = CommonConstants.ConfirmationContent;
    public string WarningContent { get; init; } = CommonConstants.WarningContent;
    public bool IsBillingFileLatest { get; init; }
}

public record SendBillingFileFormModel
{
    [Range(1, int.MaxValue)] public required int RunId { get; init; }

    [MustBeTrue(ErrorMessage = "You must confirm that you’ve checked the billing file before sending it.")]
    [Display(Name = "ConfirmSend")]
    public bool? ConfirmSend { get; init; }
}
