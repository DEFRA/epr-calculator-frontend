using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record CalculationRunViewModel
{
    public required CalculatorRunDto Run { get; init; }
    private bool IsShared => Run.BillingFile?.IsShared ?? false;
    private bool IsErrored => Run.CalculationRunStatus is CalculationRunStatus.Errored;
    private bool IsRunning => Run.CalculationRunStatus is CalculationRunStatus.None or CalculationRunStatus.Started;
    
    public bool HasTag => !string.IsNullOrWhiteSpace(TagStyle);
    public bool HasLink => !string.IsNullOrWhiteSpace(RunDetailLink);

    public string TagStyle
    {
        get
        {
            if (IsErrored)
                return "govuk-tag govuk-tag--red";

            if (IsRunning)
                return "govuk-tag govuk-tag--green";

            if (IsShared)
                return "govuk-tag govuk-tag--purple";

            return Run.RunClassification switch
            {
                RunClassification.Initial or RunClassification.Recalculation => "govuk-tag govuk-tag--purple",
                RunClassification.Test => "govuk-tag govuk-tag--yellow",
                _ => ""
            };
        }
    }

    public string RunDetailLink
    {
        get
        {
            var actionName = GetActionName();
            return string.Format(actionName, Run.RunId);

            string GetActionName()
            {
                if (IsErrored)
                    return ActionNames.CalculationRunNewDetails;

                if (IsRunning)
                    return "";

                if (IsShared)
                    return ActionNames.CompletedRun;

                if (Run.RunClassification is not (RunClassification.Unknown or RunClassification.None))
                {
                    return Run.BillingRunStatus is not BillingRunStatus.None
                        ? ActionNames.DesignatedRunWithBillingFile
                        : ActionNames.DesignatedRun;
                }

                return ActionNames.CalculationRunNewDetails;
            }
        }
    }
}
