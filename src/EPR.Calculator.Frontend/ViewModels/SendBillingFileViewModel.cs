using System.ComponentModel.DataAnnotations;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Validators;

namespace EPR.Calculator.Frontend.ViewModels;

public record ShareBillingFileViewModel : ShareBillingFileFormModel
{
    public required CalculatorRunDto Run { get; init; }
    public bool IsBillingFileLatest => Run.BillingFile?.IsLatest == true;
}

public record ShareBillingFileFormModel
{
    [Range(1, int.MaxValue)] public required int RunId { get; init; }

    [MustBeTrue(ErrorMessage = "Select the checkbox to confirm that you want to share the billing file")]
    public bool? ConfirmShare { get; init; }
}
