using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels
{
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
            this.CreatedAt = FormatCreatedAt(calculationRun.CreatedAt);
            this.CreatedBy = calculationRun.CreatedBy;
            this.Status = calculationRun.CalculatorRunClassificationId;
            this.TagStyle = GetStatusTagStyle(calculationRun.CalculatorRunClassificationId);
            this.ShowRunDetailLink = ShouldShowRunDetailLink(calculationRun.CalculatorRunClassificationId);
            this.ShowErrorLink = ShouldShowErrorLink(calculationRun.CalculatorRunClassificationId);
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
        public string CreatedAt { get; set; }

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
        /// Gets a value indicating whether gets or sets the calculation run details link.
        /// </summary>
        public string TurnOffFeatureUrl => string.Format(ActionNames.ViewCalculationRunDetails, this.Id);

        /// <summary>
        /// Gets a value indicating whether the run detail link should be displayed.
        /// </summary>
        public string TurnOnFeatureUrl => GetTurnOnFeatureUrl(this.Status, this.Id);

        private static string FormatCreatedAt(DateTime createdAt)
        {
            return CommonUtil.GetDateTime(createdAt).ToString($"{CommonConstants.DateFormat} ' at '{CommonConstants.TimeFormat}", new System.Globalization.CultureInfo("en-GB"));
        }

        private static string GetStatusTagStyle(RunClassification status) => status switch
        {
            RunClassification.RUNNING => "govuk-tag govuk-tag--green",
            RunClassification.ERROR => "govuk-tag govuk-tag--red",
            RunClassification.UNCLASSIFIED => "govuk-tag govuk-tag--grey",
            _ => "govuk-tag",
        };

        private static bool ShouldShowRunDetailLink(RunClassification status) =>
             status != RunClassification.INTHEQUEUE && status != RunClassification.RUNNING;

        private static bool ShouldShowErrorLink(RunClassification status) =>
            status == RunClassification.ERROR;

        private static string GetTurnOnFeatureUrl(RunClassification status, int id)
        {
            return status switch
            {
                RunClassification.UNCLASSIFIED => string.Format(ActionNames.ViewCalculationRunNewDetails, id),
                _ => ControllerNames.Dashboard,
            };
        }
    }
}