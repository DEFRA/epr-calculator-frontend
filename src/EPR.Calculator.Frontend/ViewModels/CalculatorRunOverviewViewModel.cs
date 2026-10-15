using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record CalculatorRunOverviewViewModel
{
    public required CalculatorRunDto Run { get; set; }

    public bool IsBillingFileRunning =>
        Run.BillingRunStatus == BillingRunStatus.Started;

    public bool IsBillingFileErrored =>
        Run.BillingRunStatus == BillingRunStatus.Errored
        || (Run.BillingRunStatus == BillingRunStatus.Started && Run.BillingRunStartedAt < DateTime.UtcNow.AddMinutes(-60));

    public bool IsOutdatedBillingFile =>
        Run.BillingRunStatus == BillingRunStatus.Completed
        && !Run.BillingFile!.IsLatest;

    public bool IsLatestBillingFile =>
        Run.BillingRunStatus == BillingRunStatus.Completed
        && Run.BillingFile!.IsLatest;

    public bool IsNewBillingRunAllowed =>
        Run.BillingRunStatus != BillingRunStatus.Started
        && !IsLatestBillingFile;

    public bool IsRemoveClassificationAllowed =>
        Run.BillingRunStatus == BillingRunStatus.Completed;

    public bool IsSubmitAllowed => IsLatestBillingFile;
}
