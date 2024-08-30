using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class DashboardViewModel
    {
        public DashboardViewModel(CalculationRun calculationRun)
        {
            Id = calculationRun.Id;
            Name = calculationRun.Name;
            CreatedAt = GetFormattedCreatedAt(calculationRun.CreatedAt);
            CreatedBy = calculationRun.CreatedBy;
            Status = calculationRun.Status;
            TagStyle = GetCalculationRunStatusStyles(calculationRun.Status);
            ShowRunDetailLink = GetShowRunDetailLink(calculationRun.Status);
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        public string Status { get; set; }

        public string? TagStyle { get; set; }

        public bool ShowRunDetailLink { get; set; }

        private string GetFormattedCreatedAt(DateTime createdAt)
        {
            return createdAt.ToString("dd MMM yyyy ' at 'H:mm", new System.Globalization.CultureInfo("en-GB"));
        }

        private string GetCalculationRunStatusStyles(string calculationRunStatus)
        {
            switch (calculationRunStatus)
            {
                case CalculationRunStatus.InTheQueue:
                case CalculationRunStatus.Running:
                    return "govuk-tag govuk-tag--grey";
                case CalculationRunStatus.Play:
                    return "govuk-tag govuk-tag--green";
                case CalculationRunStatus.Unclassified:
                    return "govuk-tag govuk-tag--blue";
                case CalculationRunStatus.Error:
                    return "govuk-tag govuk-tag--yellow";
                default:
                    return "govuk-tag";
            }
        }

        private bool GetShowRunDetailLink(string calculationRunStatus)
        {
            return !(calculationRunStatus == CalculationRunStatus.InTheQueue || calculationRunStatus == CalculationRunStatus.Running);
        }
    }
}
