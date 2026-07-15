using System.Net;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

[Route("[controller]")]
public class CalculationRunDeleteController(
    IEprCalculatorApiService eprCalculatorApiService,
    TelemetryClient telemetryClient,
    ICalculatorRunDetailsService calculatorRunDetailsService)
    : BaseController
{
    /// <summary>
    ///     Displays the calculate run delete confirmation screen.
    /// </summary>
    /// <param name="runId">The ID of the calculation run.</param>
    /// <returns>The delete confirmation view.</returns>
    [Route("{runId}")]
    public async Task<IActionResult> Index(int runId)
    {
        var runDetails = await calculatorRunDetailsService.GetCalculatorRundetailsAsync(
            HttpContext,
            runId);
        var calculatorRunStatusUpdate = new CalculatorRunStatusUpdateDto
        {
            RunId = runId,
            CalcName = runDetails?.RunName,
            ClassificationId = (int)RunClassification.DELETED
        };

        var calculationRunDeleteViewModel = new CalculationRunDeleteViewModel
        {
            CalculatorRunStatusData = calculatorRunStatusUpdate
        };
        return View(ViewNames.CalculationRunDeleteIndex, calculationRunDeleteViewModel);
    }

    /// <summary>
    ///     Displays the calculate run delete confirmation screen.
    /// </summary>
    /// <returns>The delete confirmation success view.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmationSuccess(CalculatorRunDetailsViewModel model)
    {
        var viewModel = new CalculatorRunDetailsNewViewModel
        {
            CalculatorRunDetails = model
        };

        var result = await eprCalculatorApiService.CallApi(
            HttpContext,
            HttpMethod.Put,
            "v2/calculatorRuns",
            body: new ClassificationDto
            {
                RunId = model.RunId,
                ClassificationId = (int)RunClassification.DELETED
            });

        if (result.StatusCode == HttpStatusCode.Created)
            return View(ViewNames.CalculationRunDeleteConfirmationSuccess, viewModel);

        telemetryClient.TrackTrace($"API did not return successful ({result.StatusCode}).");
        return RedirectToError();
    }
}
