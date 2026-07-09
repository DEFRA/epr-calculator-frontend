using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record CalculationRunViewModel(CalculationRun calculationRun)
{
    public int Id { get; set; } = calculationRun.Id;
    public string Name { get; set; } = calculationRun.Name;
    public DateTime CreatedAt { get; set; } = calculationRun.CreatedAt;
    public string CreatedBy { get; set; } = calculationRun.CreatedBy;
    public RunClassification Status { get; set; } = calculationRun.CalculatorRunClassificationId;
    public string? TagStyle { get; set; } = GetStatusTagStyle(calculationRun.CalculatorRunClassificationId);
    public bool ShowRunDetailLink { get; set; } = ShouldShowRunDetailLink(calculationRun.CalculatorRunClassificationId);
    public bool ShowErrorLink { get; set; } = ShouldShowErrorLink(calculationRun.CalculatorRunClassificationId);
    public bool HasBillingFileGenerated { get; set; } = calculationRun.HasBillingFileGenerated;
    public bool IsBillingFileGenerating { get; set; } = calculationRun.IsBillingFileGenerating ?? false;
    public string RunDetailLink  => GetRunDetailLink(Status, Id, HasBillingFileGenerated, IsBillingFileGenerating);

    private static string GetStatusTagStyle(RunClassification status)
    {
        return status switch
        {
            RunClassification.RUNNING => "govuk-tag govuk-tag--green",
            RunClassification.UNCLASSIFIED => "govuk-tag govuk-tag--blue",
            RunClassification.TEST_RUN => "govuk-tag govuk-tag--yellow",
            RunClassification.ERROR => "govuk-tag govuk-tag--red",
            RunClassification.INITIAL_RUN => "govuk-tag govuk-tag--purple",
            RunClassification.INITIAL_RUN_COMPLETED => "govuk-tag govuk-tag--purple",
            RunClassification.INTERIM_RECALCULATION_RUN => "govuk-tag govuk-tag--purple",
            RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED => "govuk-tag govuk-tag--purple",
            RunClassification.FINAL_RECALCULATION_RUN => "govuk-tag govuk-tag--purple",
            RunClassification.FINAL_RECALCULATION_RUN_COMPLETED => "govuk-tag govuk-tag--purple",
            RunClassification.FINAL_RUN => "govuk-tag govuk-tag--purple",
            RunClassification.FINAL_RUN_COMPLETED => "govuk-tag govuk-tag--purple",
            _ => "govuk-tag"
        };
    }

    private static bool ShouldShowRunDetailLink(RunClassification status)
    {
        return status != RunClassification.QUEUE && status != RunClassification.RUNNING;
    }

    private static bool ShouldShowErrorLink(RunClassification status)
    {
        return status == RunClassification.ERROR;
    }

    private static string GetRunDetailLink(RunClassification status, int id, bool hasBillingFileGenerated, bool isBillingFileGenerating)
    {
        return status switch
        {
            RunClassification.UNCLASSIFIED =>
                string.Format(ActionNames.CalculationRunNewDetails, id),

            RunClassification.TEST_RUN =>
                string.Format(ActionNames.DesignatedRun, id),

            RunClassification.INITIAL_RUN or RunClassification.INTERIM_RECALCULATION_RUN or RunClassification.FINAL_RECALCULATION_RUN or RunClassification.FINAL_RUN when isBillingFileGenerating || hasBillingFileGenerated =>
                string.Format(ActionNames.DesignatedRunWithBillingFile, id),

            RunClassification.INITIAL_RUN or RunClassification.INTERIM_RECALCULATION_RUN or RunClassification.FINAL_RECALCULATION_RUN or RunClassification.FINAL_RUN when !hasBillingFileGenerated =>
                string.Format(ActionNames.DesignatedRun, id),

            RunClassification.INITIAL_RUN_COMPLETED or RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED or RunClassification.FINAL_RECALCULATION_RUN_COMPLETED or RunClassification.FINAL_RUN_COMPLETED =>
                string.Format(ActionNames.CompletedRun, id),

            RunClassification.ERROR =>
                string.Format(ActionNames.CalculationRunNewDetails, id),

            _ => ControllerNames.Dashboard
        };
    }
}
