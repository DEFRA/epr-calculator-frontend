using EPR.Calculator.Frontend.Constants;
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
            this.CreatedAt = GetFormattedCreatedAt(calculationRun.CreatedAt);
            this.CreatedBy = calculationRun.CreatedBy;
            this.Status = calculationRun.Status;
            this.TagStyle = GetCalculationRunStatusStyles(calculationRun.Status);
            this.ShowRunDetailLink = GetShowRunDetailLink(calculationRun.Status);
            this.ShowErrorLink = GetShowErrorLink(calculationRun.Status);
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
        public string Status { get; set; }

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
        public string TurnOffFeatureUrl =>
        string.Format(ActionNames.ViewCalculationRunDetails, this.Id);

        /// <summary>
        /// Gets a value indicating whether gets or sets the calculation run new details link.
        /// </summary>
        public string TurnOnFeatureUrl => DashboardHelper.GetTurnOnFeatureUrl(this.Status, this.Id);

        private static string GetFormattedCreatedAt(DateTime createdAt)
        {
            return CommonUtil.GetDateTime(createdAt).ToString($"{CommonConstants.DateFormat} ' at '{CommonConstants.TimeFormat}", new System.Globalization.CultureInfo("en-GB"));
        }

        private static string GetCalculationRunStatusStyles(string calculationRunStatus)
        {
            switch (calculationRunStatus)
            {
                case CalculationRunStatus.Running:
                    return "govuk-tag govuk-tag--green";
                case CalculationRunStatus.Error:
                    return "govuk-tag govuk-tag--red";
                default:
                    return "govuk-tag";
            }
        }

        private static bool GetShowRunDetailLink(string calculationRunStatus)
        {
            return !(calculationRunStatus == CalculationRunStatus.InTheQueue || calculationRunStatus == CalculationRunStatus.Running);
        }

        private static bool GetShowErrorLink(string calculationRunStatus)
        {
            return calculationRunStatus == CalculationRunStatus.Error;
        }
    }
}
