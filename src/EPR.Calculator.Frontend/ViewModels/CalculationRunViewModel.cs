using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;

namespace EPR.Calculator.Frontend.ViewModels;

public record CalculationRunViewModel
{
    public required int RunId { get; init; }
    public required string RunName { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required string CreatedBy { get; init; }
    public required RunClassification RunClassification { get; init; }
    public required CalculationRunStatus CalculationRunStatus { get; init; }
    public required BillingRunStatus BillingRunStatus { get; init; }
    public required bool IsShared { get; init; }

    public string OverallStatus
    {
        get
        {
            if (IsShared)
                return $"{RunClassification} run completed";

            if (CalculationRunStatus is CalculationRunStatus.Errored)
                return "Calculation run errored";

            if (BillingRunStatus is BillingRunStatus.Errored)
                return "Billing run errored";

            if(CalculationRunStatus is CalculationRunStatus.Started)
                return "Calculation run in progress";

            if(BillingRunStatus is BillingRunStatus.Started)
                return "Billing run in progress";

            if(RunClassification is not RunClassification.None)
                return $"{RunClassification} run";

            if(RunClassification is RunClassification.None && CalculationRunStatus is CalculationRunStatus.Completed)
                return "Ready for classification";

            return "";
        }
    }

    public string TagStyle
    {
        get
        {
            return "govuk-tag" + GetTagColour();

            string GetTagColour()
            {
                if(IsShared)
                    return " govuk-tag--purple";

                if(CalculationRunStatus is CalculationRunStatus.Errored || BillingRunStatus is BillingRunStatus.Errored)
                    return " govuk-tag--red";

                if(CalculationRunStatus is CalculationRunStatus.Started || BillingRunStatus is BillingRunStatus.Started)
                    return " govuk-tag--green";

                return RunClassification switch
                {
                    RunClassification.None => " govuk-tag--blue",
                    RunClassification.Test => " govuk-tag--yellow",
                    RunClassification.Initial or RunClassification.Recalculation => " govuk-tag--purple",
                    _ => ""
                };
            }
        }
    }

    public string RunDetailLink
    {
        get
        {
            var actionName = GetActionName();
            return string.Format(actionName, RunId);

            string GetActionName()
            {
                if (IsShared)
                    return ActionNames.CompletedRun;

                if(CalculationRunStatus is CalculationRunStatus.Errored || BillingRunStatus is BillingRunStatus.Errored)
                    return ActionNames.CalculationRunNewDetails;

                if (RunClassification is not (RunClassification.Unknown or RunClassification.None))
                {
                    return BillingRunStatus is not BillingRunStatus.None
                        ? ActionNames.DesignatedRunWithBillingFile
                        : ActionNames.DesignatedRun;
                }

                return ActionNames.CalculationRunNewDetails;
            }
        }
    }

    public bool ShowRunDetailLink =>
        CalculationRunStatus is not CalculationRunStatus.Started
        && BillingRunStatus is not BillingRunStatus.Started;

    public bool ShowStatus => !string.IsNullOrWhiteSpace(OverallStatus);
}
