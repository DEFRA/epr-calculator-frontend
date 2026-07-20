using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

[Route("[controller]")]
public class DesignatedRunWithBillingFileController(
    IEprCalculatorApiService eprCalculatorApiService,
    TelemetryClient telemetryClient)
    : BaseController
{
    [Route("{runId:int}")]
    public async Task<IActionResult> Index(int runId)
    {
        var viewModel = await CreateViewModel(runId);

        if (viewModel == null)
        {
            telemetryClient.TrackTrace($"No run details found for runId: {runId}");
            return RedirectToError();
        }

        return View(ViewNames.CalculationRunOverviewIndex, viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Submit(int runId)
    {
        if (!ModelState.IsValid)
            return RedirectToAction("Index", new { runId });

        return RedirectToAction("Index", "SendBillingFile", new { runId });
    }

    [HttpGet]
    public async Task<IActionResult> GenerateDraftBillingFile(int runId)
    {
        var result = await TryGenerateDraftBillingFile(runId);

        if (result)
            return RedirectToRoute(new { controller = ControllerNames.CalculationRunOverview, action = "Index",   runId });

        throw new InvalidOperationException($"Failed to generate draft billing file for calculation run {runId}.");
    }

    private async Task<bool> TryGenerateDraftBillingFile(int runId)
    {
        var responseDto = await eprCalculatorApiService.CallApi(
            HttpMethod.Put,
            $"v1/producerBillingInstructionsAccept/{runId}");

        if (!responseDto.IsSuccessStatusCode)
        {
            telemetryClient.TrackTrace($"Billing instructions acceptance failed for RunId {runId}. StatusCode: {responseDto.StatusCode}, Reason: {responseDto.ReasonPhrase}");
            return false;
        }

        return true;
    }

    private async Task<CalculatorRunOverviewViewModel?> CreateViewModel(int runId)
    {
        var runDto = await eprCalculatorApiService.GetCalculatorRun(runId);

        if (runDto == null)
            return null;

        return new CalculatorRunOverviewViewModel
        {
            Run = runDto
        };
    }
}
