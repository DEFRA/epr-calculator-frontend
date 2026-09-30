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
    public required BillingRunStatus BillingRunStatus { get; init; }

    public string TagStyle =>
        "govuk-tag" + RunClassification switch
        {
            RunClassification.Running => " govuk-tag--green",
            RunClassification.Unclassified => " govuk-tag--blue",
            RunClassification.Test => " govuk-tag--yellow",
            RunClassification.Errored => " govuk-tag--red",
            RunClassification.Initial => " govuk-tag--purple",
            RunClassification.InitialCompleted => " govuk-tag--purple",
            RunClassification.Recalculation => " govuk-tag--purple",
            RunClassification.RecalculationCompleted => " govuk-tag--purple",
            _ => ""
        };

    public string RunDetailLink =>
        RunClassification switch
        {
            RunClassification.Unclassified =>
                string.Format(ActionNames.CalculationRunNewDetails, RunId),

            RunClassification.Test =>
                string.Format(ActionNames.DesignatedRun, RunId),

            RunClassification.Initial
                or RunClassification.Recalculation
                when BillingRunStatus is BillingRunStatus.None
                => string.Format(ActionNames.DesignatedRun, RunId),

            RunClassification.Initial
                or RunClassification.Recalculation
                when BillingRunStatus is not BillingRunStatus.None
                => string.Format(ActionNames.DesignatedRunWithBillingFile, RunId),

            RunClassification.InitialCompleted
                or RunClassification.RecalculationCompleted
                => string.Format(ActionNames.CompletedRun, RunId),

            RunClassification.Errored =>
                string.Format(ActionNames.CalculationRunNewDetails, RunId),

            _ => ControllerNames.Dashboard
        };

    public bool ShowRunDetailLink =>
        RunClassification
            is not RunClassification.Running;

    public bool ShowErrorLink =>
        RunClassification
            is RunClassification.Errored;

    public bool ShowStatus =>
        Enum.IsDefined(typeof(RunClassification), RunClassification)
        && RunClassification
            is not RunClassification.Unclassified
            and not RunClassification.None;
}
