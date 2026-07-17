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
            RunClassification.RUNNING => " govuk-tag--green",
            RunClassification.UNCLASSIFIED => " govuk-tag--blue",
            RunClassification.TEST_RUN => " govuk-tag--yellow",
            RunClassification.ERROR => " govuk-tag--red",
            RunClassification.INITIAL_RUN => " govuk-tag--purple",
            RunClassification.INITIAL_RUN_COMPLETED => " govuk-tag--purple",
            RunClassification.INTERIM_RECALCULATION_RUN => " govuk-tag--purple",
            RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED => " govuk-tag--purple",
            RunClassification.FINAL_RECALCULATION_RUN => " govuk-tag--purple",
            RunClassification.FINAL_RECALCULATION_RUN_COMPLETED => " govuk-tag--purple",
            RunClassification.FINAL_RUN => " govuk-tag--purple",
            RunClassification.FINAL_RUN_COMPLETED => " govuk-tag--purple",
            _ => ""
        };

    public string RunDetailLink =>
        RunClassification switch
        {
            RunClassification.UNCLASSIFIED =>
                string.Format(ActionNames.CalculationRunNewDetails, RunId),

            RunClassification.TEST_RUN =>
                string.Format(ActionNames.DesignatedRun, RunId),

            RunClassification.INITIAL_RUN
                or RunClassification.INTERIM_RECALCULATION_RUN
                or RunClassification.FINAL_RECALCULATION_RUN
                or RunClassification.FINAL_RUN
                when BillingRunStatus is BillingRunStatus.None =>
                string.Format(ActionNames.DesignatedRun, RunId),

            RunClassification.INITIAL_RUN
                or RunClassification.INTERIM_RECALCULATION_RUN
                or RunClassification.FINAL_RECALCULATION_RUN
                or RunClassification.FINAL_RUN
                when BillingRunStatus is not BillingRunStatus.None =>
                string.Format(ActionNames.DesignatedRunWithBillingFile, RunId),

            RunClassification.INITIAL_RUN_COMPLETED
                or RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED
                or RunClassification.FINAL_RECALCULATION_RUN_COMPLETED
                or RunClassification.FINAL_RUN_COMPLETED =>
                string.Format(ActionNames.CompletedRun, RunId),

            RunClassification.ERROR =>
                string.Format(ActionNames.CalculationRunNewDetails, RunId),

            _ => ControllerNames.Dashboard
        };

    public bool ShowRunDetailLink =>
        RunClassification
            is not RunClassification.QUEUE
            and not RunClassification.RUNNING;

    public bool ShowErrorLink =>
        RunClassification
            is RunClassification.ERROR;

    public bool ShowStatus =>
        Enum.IsDefined(typeof(RunClassification), (int)RunClassification)
        && RunClassification
            is not RunClassification.UNCLASSIFIED
            and not RunClassification.None;
}
