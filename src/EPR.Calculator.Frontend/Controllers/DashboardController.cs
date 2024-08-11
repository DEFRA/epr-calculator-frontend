using System.Globalization;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.CalculationRuns = GetCalulationRunsMockData();

            return View(ViewNames.DashboardIndex);
        }

        private List<CalculationRun> GetCalulationRunsMockData()
        {
            var calculationRuns = new List<CalculationRun>();

            calculationRuns.AddRange([
                new CalculationRun { Id = 1, Name = "Default cettings check", CreatedAt = DateTime.Parse("28/06/2025 10:01:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.InTheQueue, tagStyle = ApplyCalculationRunStatusStyles(CalculationRunStatus.InTheQueue) },
                new CalculationRun { Id = 2, Name = "Alteration check", CreatedAt = DateTime.Parse("28/06/2025 12:19:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Running, tagStyle = ApplyCalculationRunStatusStyles(CalculationRunStatus.Running) },
                new CalculationRun { Id = 3, Name = "Test 10", CreatedAt = DateTime.Parse("21/06/2025 12:09:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Unclassified, tagStyle = ApplyCalculationRunStatusStyles(CalculationRunStatus.Unclassified) },
                new CalculationRun { Id = 4, Name = "June check", CreatedAt = DateTime.Parse("11/06/2025 09:14:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Play, tagStyle = ApplyCalculationRunStatusStyles(CalculationRunStatus.Play) },
                new CalculationRun { Id = 5, Name = "Pre June check", CreatedAt = DateTime.Parse("13/06/2025 11:18:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Play, tagStyle = ApplyCalculationRunStatusStyles(CalculationRunStatus.Play) },
                new CalculationRun { Id = 6, Name = "Local Authority data check 5", CreatedAt = DateTime.Parse("10/06/2025 08:13:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Play, tagStyle = ApplyCalculationRunStatusStyles(CalculationRunStatus.Play) },
                new CalculationRun { Id = 7, Name = "Local Authority data check 4", CreatedAt = DateTime.Parse("10/06/2025 10:14:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Play, tagStyle = ApplyCalculationRunStatusStyles(CalculationRunStatus.Play) },
                new CalculationRun { Id = 8, Name = "Local Authority data check 3", CreatedAt = DateTime.Parse("08/06/2025 10:00:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Play, tagStyle = ApplyCalculationRunStatusStyles(CalculationRunStatus.Play) },
                new CalculationRun { Id = 9, Name = "Local Authority data check 2", CreatedAt = DateTime.Parse("06/06/2025 11:20:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Play, tagStyle = ApplyCalculationRunStatusStyles(CalculationRunStatus.Play) },
                new CalculationRun { Id = 10, Name = "Local Authority data check", CreatedAt = DateTime.Parse("02/06/2025 12:02:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Play, tagStyle = ApplyCalculationRunStatusStyles(CalculationRunStatus.Play) },
                new CalculationRun { Id = 11, Name = "Fee adjustment check", CreatedAt = DateTime.Parse("01/06/2025 09:12:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Error, tagStyle = ApplyCalculationRunStatusStyles(CalculationRunStatus.Error) }
            ]);

            return calculationRuns;
        }

        private string ApplyCalculationRunStatusStyles(string calculationRunStatus)
        {
            switch (calculationRunStatus)
            {
                case CalculationRunStatus.InTheQueue:
                case CalculationRunStatus.Running:
                    return "govuk-tag govuk-tag--grey";
                case CalculationRunStatus.Play:
                    return "govuk-tag govuk-tag--green";
                case CalculationRunStatus.Unclassified:
                    return "govuk-tag";
                case CalculationRunStatus.Error:
                    return "govuk-tag govuk-tag--yellow";
                default:
                    return "govuk-tag";
            }
        }
    }
}
