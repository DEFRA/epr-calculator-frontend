using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

[Route("[controller]")]
public class CalculationRunDeleteController(
    IEprCalculatorApiService eprCalculatorApiService,
    TelemetryClient telemetryClient)
    : BaseController
{
    /// <summary>
    ///     Displays the calculate run delete confirmation screen.
    /// </summary>
    /// <param name="runId">The ID of the calculation run.</param>
    /// <returns>The delete confirmation view.</returns>
    [Route("{runId:int}")]
    public async Task<IActionResult> Index(int runId)
    {
        var viewModel = await CreateViewModel(runId);

        if (viewModel == null)
            return RedirectToAction(nameof(DashboardController.Index), "Dashboard");

        return View(ViewNames.CalculationRunDeleteIndex, viewModel);
    }

    /// <summary>
    ///     Displays the calculate run delete confirmation screen.
    /// </summary>
    /// <returns>The delete confirmation success view.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmationSuccess(CalculationRunDeleteFormModel model)
    {
        if (!ModelState.IsValid)
            return RedirectToError();

        try
        {
            var viewModel = await CreateViewModel(model.RunId);

            if (viewModel == null)
                return RedirectToError();

            await eprCalculatorApiService.DeleteCalculatorRun(model.RunId);

            return View(ViewNames.CalculationRunDeleteConfirmationSuccess, viewModel);
        }
        catch
        {
            telemetryClient.TrackTrace($"API was not able to delete the run {model.RunId}.");
            return RedirectToError();
        }
    }

    private async Task<CalculationRunDeleteViewModel?> CreateViewModel(int runId)
    {
        var run = await eprCalculatorApiService.GetCalculatorRun(runId);

        if (run == null)
            return null;

        return new CalculationRunDeleteViewModel
        {
            RunId = run.RunId,
            RunName = run.RunName
        };
    }
}
