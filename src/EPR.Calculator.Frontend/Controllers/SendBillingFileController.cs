using System.Net;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

/// <summary>
///     Controller for sending billing files.
/// </summary>
[Route("[controller]")]
public class SendBillingFileController(
    TelemetryClient telemetryClient,
    IEprCalculatorApiService eprCalculatorApiService,
    ICalculatorRunDetailsService calculatorRunDetailsService)
    : BaseController
{
    [Route("{runId}")]
    public async Task<IActionResult> Index(int runId)
    {
        var runDetails = await calculatorRunDetailsService.GetCalculatorRundetailsAsync(
            HttpContext,
            runId);
        if (runDetails == null || runDetails.RunName == null)
            return RedirectToError();

        if (runDetails.IsBillingFileGeneratedLatest.HasValue && !runDetails.IsBillingFileGeneratedLatest.Value)
            return RedirectToError();

        var billingFileViewModel = new SendBillingFileViewModel
        {
            RunId = runId,
            CalcRunName = runDetails.RunName,
        };

        return View(billingFileViewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(SendBillingFileViewModel viewModel)
    {
        if (viewModel.ConfirmSend != true || !ModelState.IsValid)
            return View(ViewNames.SendBillingFileIndex, viewModel);

        var response = await PrepareBillingFileSendToFSSAsync(viewModel.RunId);

        if (response.StatusCode == HttpStatusCode.Accepted)
            return RedirectToAction(ActionNames.BillingFileSuccess, CommonUtil.GetControllerName(typeof(BillingInstructionsController)));

        if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            viewModel.IsBillingFileLatest = false;
            return View(ActionNames.Index, viewModel);
        }

        telemetryClient.TrackTrace($"1.Request (send billing file) not accepted response code:{response.StatusCode}");
        var contentString = await response.Content.ReadAsStringAsync();
        telemetryClient.TrackTrace($"2.Request (send billing file) not accepted response message:{contentString}");
        return RedirectToError();
    }

    /// <summary>
    ///     Calls the "prepareBillingFileSendToFSS" POST endpoint.
    /// </summary>
    /// <param name="runId">The runId parameter to be used as url parameter.</param>
    /// <returns>The response message returned by the endpoint.</returns>
    private async Task<HttpResponseMessage> PrepareBillingFileSendToFSSAsync(int runId)
    {
        return await eprCalculatorApiService.CallApi(
            HttpContext,
            HttpMethod.Post,
            $"v2/prepareBillingFileSendToFSS/{runId}");
    }
}
