using System.Globalization;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            var calculationRuns = GetCalulationRunsMockData();

            var dashboardRunData = new List<DashboardViewModel>();

            foreach(var calculationRun in calculationRuns)
            {
                dashboardRunData.Add(new DashboardViewModel(calculationRun));
            }

            ViewData["DashboardRunData"] = dashboardRunData;

            return View(ViewNames.DashboardIndex);
        }

        // TODO: This method should be deleted during GET API integration
        private List<CalculationRun> GetCalulationRunsMockData()
        {
            var calculationRuns = new List<CalculationRun>();

            calculationRuns.AddRange([
                new CalculationRun { Id = 1, Name = "Default settings check", CreatedAt = DateTime.Parse("28/06/2025 10:01:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.InTheQueue },
                new CalculationRun { Id = 2, Name = "Alteration check", CreatedAt = DateTime.Parse("28/06/2025 12:19:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Running },
                new CalculationRun { Id = 3, Name = "Test 10", CreatedAt = DateTime.Parse("21/06/2025 12:09:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Unclassified },
                new CalculationRun { Id = 4, Name = "June check", CreatedAt = DateTime.Parse("11/06/2025 09:14:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Play },
                new CalculationRun { Id = 5, Name = "Pre June check", CreatedAt = DateTime.Parse("13/06/2025 11:18:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Play },
                new CalculationRun { Id = 6, Name = "Local Authority data check 5", CreatedAt = DateTime.Parse("10/06/2025 08:13:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Play },
                new CalculationRun { Id = 7, Name = "Local Authority data check 4", CreatedAt = DateTime.Parse("10/06/2025 10:14:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Play },
                new CalculationRun { Id = 8, Name = "Local Authority data check 3", CreatedAt = DateTime.Parse("08/06/2025 10:00:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Play },
                new CalculationRun { Id = 9, Name = "Local Authority data check 2", CreatedAt = DateTime.Parse("06/06/2025 11:20:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Play },
                new CalculationRun { Id = 10, Name = "Local Authority data check", CreatedAt = DateTime.Parse("02/06/2025 12:02:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Play },
                new CalculationRun { Id = 11, Name = "Fee adjustment check", CreatedAt = DateTime.Parse("01/06/2025 09:12:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Error }
            ]);

            return calculationRuns;
        }
    }
}
