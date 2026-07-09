using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

/// <summary>
///     Controller for the calculation run overview page.
/// </summary>
[Route("[controller]")]
public class DesignatedRunWithBillingFileController(
    IEprCalculatorApiService eprCalculatorApiService,
    TelemetryClient telemetryClient,
    ICalculatorRunDetailsService calculatorRunDetailsService)
    : BaseController
{
    [Route("{runId}")]
    public async Task<IActionResult> Index(int runId)
    {
        if (runId <= 0)
            return RedirectToError();

        var viewModel = await CreateViewModel(runId);
        if (viewModel.CalculatorRunDetails.RunId <= 0)
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
            return RedirectToAction(ActionNames.Index, new { runId });

        return RedirectToAction(ActionNames.Index, ControllerNames.SendBillingFile, new {   runId });
    }

    [HttpGet]
    public async Task<IActionResult> GenerateDraftBillingFile(int id)
    {
        var result = await TryGenerateDraftBillingFile(id);
        if (result)
        {
            return RedirectToRoute(new
            {
                controller = ControllerNames.CalculationRunOverview,
                action = "Index",
                runId = id
            });
        }

        throw new InvalidOperationException($"Failed to generate draft billing file for calculation run {id}.");
    }

    private async Task<bool> TryGenerateDraftBillingFile(int id)
    {
        var responseDto = await eprCalculatorApiService.CallApi(
            HttpContext,
            HttpMethod.Put,
            $"v1/producerBillingInstructionsAccept/{id}");

        if (!responseDto.IsSuccessStatusCode)
        {
            telemetryClient.TrackTrace($"Billing instructions acceptance failed for RunId {id}. StatusCode: {responseDto.StatusCode}, Reason: {responseDto.ReasonPhrase}");
            return false;
        }

        return true;
    }

    private async Task<CalculatorRunOverviewViewModel> CreateViewModel(int runId)
    {
        var currentUser = CommonUtil.GetUserName(HttpContext);
        var viewModel = new CalculatorRunOverviewViewModel
        {
            CurrentUser = currentUser,
            CalculatorRunDetails = new CalculatorRunDetailsViewModel(),
            BackLinkViewModel = new BackLinkViewModel
            {
                BackLink = string.Empty,
                CurrentUser = currentUser,
                HideBackLink = GetBackLink() != ControllerNames.Dashboard
            }
        };

        var runDetails = await calculatorRunDetailsService.GetCalculatorRundetailsAsync(
            HttpContext,
            runId);
        if (runDetails != null && runDetails!.RunId > 0)
            viewModel.CalculatorRunDetails = runDetails;

        return viewModel;
    }
}
