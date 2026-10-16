using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;
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

    private async Task<DashboardViewModel> GetDashboardViewModel(RelativeYear relativeYear)
    {
        HttpContext.Session.SetInt32(SessionConstants.RelativeYear, relativeYear.Value);

        var dashboardViewModel = new DashboardViewModel
        {
            RelativeYear = relativeYear,
            Calculations = await FindCalculatorRunsAsync(relativeYear)
        };

        return dashboardViewModel;
    }

    private async Task<List<CalculationRunViewModel>> FindCalculatorRunsAsync(RelativeYear relativeYear)
    {
        var runs = await eprCalculatorApiService.FindCalculatorRuns(relativeYear);

        return runs
            .Where(x => x.RunClassification is not RunClassification.Deleted)
            .Select(run => new CalculationRunViewModel
            {
                Run = run
            })
            .ToList();
    }
}
