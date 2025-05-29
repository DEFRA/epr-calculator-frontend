using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.ViewModels
{
    [ExcludeFromCodeCoverage]
    /// <summary>
    /// View model to hold the calculation runs.
    /// </summary>
    public class CalculationRunViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationRunViewModel"/> class.
        /// </summary>
        /// <param name="calculationRun">The calculation run.</param>
        public CalculationRunViewModel(CalculationRun calculationRun)
        {
            this.Id = calculationRun.Id;
            this.Name = calculationRun.Name;
            this.CreatedAt = calculationRun.CreatedAt;
            this.CreatedBy = calculationRun.CreatedBy;
            this.Status = calculationRun.CalculatorRunClassificationId;
            this.TagStyle = GetStatusTagStyle(calculationRun.CalculatorRunClassificationId);
            this.ShowRunDetailLink = ShouldShowRunDetailLink(calculationRun.CalculatorRunClassificationId);
            this.ShowErrorLink = ShouldShowErrorLink(calculationRun.CalculatorRunClassificationId);
            this.HasBillingFileGenerated = calculationRun.HasBillingFileGenerated;
        }

        /// <summary>
        /// Gets or sets the run id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the run name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the run created timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the run created by user.
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the run status.
        /// </summary>
        public RunClassification Status { get; set; }

        /// <summary>
        /// Gets or sets the tag style.
        /// </summary>
        public string? TagStyle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the show run detail link.
        /// </summary>
        public bool ShowRunDetailLink { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the show error link.
        /// </summary>
        public bool ShowErrorLink { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the has billing file generated.
        /// </summary>
        public bool HasBillingFileGenerated { get; set; }

        /// <summary>
        /// Gets a value indicating whether gets or sets the calculation run details link.
        /// </summary>
        public string TurnOffFeatureUrl => string.Format(ActionNames.ViewCalculationRunDetails, this.Id);

        /// <summary>
        /// Gets a value indicating whether the run detail link should be displayed.
        /// </summary>
        public string TurnOnFeatureUrl => GetTurnOnFeatureUrl(this.Status, this.Id, this.HasBillingFileGenerated);

        private static string GetStatusTagStyle(RunClassification status) => status switch
        {
            RunClassification.RUNNING => "govuk-tag govuk-tag--green",
            RunClassification.INITIAL_RUN => "govuk-tag govuk-tag--green",
            RunClassification.INITIAL_RUN_COMPLETED => "govuk-tag govuk-tag--purple",
            RunClassification.ERROR => "govuk-tag govuk-tag--red",
            _ => "govuk-tag",
        };

        private static bool ShouldShowRunDetailLink(RunClassification status) =>
             status != RunClassification.QUEUE && status != RunClassification.RUNNING;

        private static bool ShouldShowErrorLink(RunClassification status) =>
            status == RunClassification.ERROR;

        private static string GetTurnOnFeatureUrl(RunClassification status, int id, bool hasBillingFileGenerated)
        {
            return status switch
            {
                RunClassification.UNCLASSIFIED => string.Format(ActionNames.CalculationRunNewDetails, id),
                RunClassification.INITIAL_RUN when !hasBillingFileGenerated => string.Format(ActionNames.ClassifyRunConfirmation, id),
                RunClassification.INITIAL_RUN when hasBillingFileGenerated => string.Format(ActionNames.CalculationRunOverview, id),
                RunClassification.INITIAL_RUN_COMPLETED => string.Format(ActionNames.PostBillingFile, id),
                RunClassification.ERROR => string.Format(ActionNames.CalculationRunNewDetails, id),
                _ => ControllerNames.Dashboard,
            };
        }
    }
}