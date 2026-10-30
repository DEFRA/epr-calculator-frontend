using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

[Route("ChangeFinancialYear")]
public class ChangeFinancialYearController(
    IConfiguration configuration,
    IEprCalculatorApiService eprCalculatorApiService)
    : BaseController
{
    private readonly int relativeYearStartingMonth = CommonUtil.GetRelativeYearStartingMonth(configuration);

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        return View(ViewNames.ChangeFinancialYearIndex, await CreateViewModel());
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ChangeFinancialYearFormModel model)
    {
        var viewModel = await CreateViewModel();

        if (ModelState.IsValid && !viewModel.RelativeYears.Contains(new RelativeYear(model.RelativeYear!.Value)))
            ModelState.AddModelError(nameof(model.RelativeYear), ErrorMessages.FinancialYearNotSelected);

        if (!ModelState.IsValid)
            return View(ViewNames.ChangeFinancialYearIndex, viewModel);

        HttpContext.Session.SetInt32(SessionConstants.RelativeYear, model.RelativeYear!.Value);
        return RedirectToAction(ActionNames.Index, ControllerNames.Dashboard);
    }

    private async Task<ChangeFinancialYearViewModel> CreateViewModel()
    {
        var currentYear = CommonUtil.GetDefaultRelativeYear(DateTime.UtcNow, relativeYearStartingMonth);
        var relativeYears = await eprCalculatorApiService.FindRelativeYears();

        return new ChangeFinancialYearViewModel
        {
            RelativeYears = relativeYears
                .Append(currentYear)
                .Where(year => year.Value <= currentYear.Value)
                .Distinct()
                .OrderByDescending(year => year.Value)
                .ToList()
        };
    }
}
