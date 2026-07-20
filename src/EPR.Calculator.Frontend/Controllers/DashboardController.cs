using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SessionExtensions = EPR.Calculator.Frontend.Extensions.SessionExtensions;

namespace EPR.Calculator.Frontend.Controllers;

[Route("/")]
public class DashboardController(
    IConfiguration configuration,
    IEprCalculatorApiService eprCalculatorApiService)
    : BaseController
{
    private readonly int relativeYearStartingMonth = CommonUtil.GetRelativeYearStartingMonth(configuration);

    /// <summary>
    ///     Handles the Index action for the controller.
    /// </summary>
    /// <returns>
    ///     An <see cref="IActionResult" /> that renders the Dashboard Index view with the calculation runs data,
    ///     or redirects to the Standard Error page if an error occurs.
    /// </returns>
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        SessionExtensions.ClearAllSession(HttpContext.Session);
        var model = await GetDashboardViewModel(CommonUtil.GetRelativeYear(HttpContext.Session, relativeYearStartingMonth));
        return View(ViewNames.DashboardIndex, model);
    }

    /// <summary>
    ///     Returns the list of calculation runs for the given relative year.
    /// </summary>
    /// <param name="relativeYear">The relative year to filter the calculation runs.</param>
    /// <returns>The list of calculation runs.</returns>
    [Route("Dashboard/GetCalculations")]
    public async Task<IActionResult> GetCalculations(int relativeYear) // matches query param name
    {
        var model = await GetDashboardViewModel(new RelativeYear(relativeYear));
        return PartialView("_CalculationRunsPartial", model.Calculations);
    }

    private static List<RelativeYear> GetFilteredRelativeYears(List<RelativeYear> allYears, int relativeYearStartingMonth)
    {
        var today = DateTime.Today;
        var currentStartYear = today.Month >= relativeYearStartingMonth ? today.Year : today.Year - 1;

        var filteredYears = allYears
            .Where(y => y.Value <= currentStartYear)
            .OrderByDescending(y => y.Value)
            .ToList();

        return filteredYears;
    }

    private async Task<DashboardViewModel> GetDashboardViewModel(RelativeYear relativeYear)
    {
        HttpContext.Session.SetInt32(SessionConstants.RelativeYear, relativeYear.Value);

        var dashboardViewModel = new DashboardViewModel
        {
            RelativeYear = relativeYear,
            RelativeYearSelectList = await GetRelativeYearsAsync(relativeYear),
            Calculations = await FindCalculatorRunsAsync(relativeYear)
        };

        return dashboardViewModel;
    }

    private async Task<List<CalculationRunViewModel>> FindCalculatorRunsAsync(RelativeYear relativeYear)
    {
        var runs = await eprCalculatorApiService.FindCalculatorRuns(relativeYear);

        return runs
            .Where(x => x.RunClassification
                is not RunClassification.DELETED
                and not RunClassification.QUEUE)
            .Select(run => new CalculationRunViewModel
            {
                RunId = run.RunId,
                RunName = run.RunName,
                CreatedAt = run.CreatedAt,
                CreatedBy = run.CreatedBy,
                RunClassification = run.RunClassification,
                BillingRunStatus = run.BillingRunStatus
            })
            .ToList();
    }

    private async Task<List<SelectListItem>> GetRelativeYearsAsync(RelativeYear selectedRelativeYear)
    {
        var relativeYears = await eprCalculatorApiService.FindRelativeYears();
        relativeYears = GetFilteredRelativeYears(relativeYears, relativeYearStartingMonth);

        // Ensure current year is first
        var currentYear = CommonUtil.GetDefaultRelativeYear(DateTime.UtcNow, relativeYearStartingMonth);
        relativeYears.Remove(currentYear);
        relativeYears.Insert(0, currentYear);

        // Build SelectListItem list
        var selectList = relativeYears
            .Select(y => new SelectListItem
            {
                Value = y.Value.ToString(),
                Text = y.ToFinancialYear(),
                Selected = y == selectedRelativeYear
            })
            .ToList();

        return selectList;
    }
}
