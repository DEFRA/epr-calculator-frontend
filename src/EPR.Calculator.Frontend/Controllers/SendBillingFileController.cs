using System.Net;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EPR.Calculator.Frontend.Controllers;

[Route("[controller]")]
public class SendBillingFileController(
    TelemetryClient telemetryClient,
    IEprCalculatorApiService eprCalculatorApiService)
    : BaseController
{
    [Route("{runId:int}")]
    public async Task<IActionResult> Index(int runId)
    {
        var viewModel = await CreateViewModel(runId);

        if (viewModel is not { IsBillingFileLatest: true })
            return RedirectToError();

        return View(ViewNames.SendBillingFileIndex, viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(SendBillingFileFormModel model)
    {
        if (!ModelState.IsValid)
        {
            if (ModelState[nameof(model.RunId)] is { ValidationState: ModelValidationState.Invalid })
                return RedirectToError();

            return await Index(model.RunId);
        }

        var response = await eprCalculatorApiService.CallApi(HttpMethod.Post, $"v2/prepareBillingFileSendToFSS/{model.RunId}");

        if (response.StatusCode == HttpStatusCode.Accepted)
            return RedirectToAction(ActionNames.BillingFileSuccess, CommonUtil.GetControllerName(typeof(BillingInstructionsController)));

        if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            var viewModel = await CreateViewModel(model.RunId);

            if (viewModel == null)
                return RedirectToError();

            return View(ActionNames.Index, viewModel with { IsBillingFileLatest = false });
        }

        telemetryClient.TrackTrace($"1.Request (send billing file) not accepted response code:{response.StatusCode}");
        var contentString = await response.Content.ReadAsStringAsync();
        telemetryClient.TrackTrace($"2.Request (send billing file) not accepted response message:{contentString}");
        return RedirectToError();
    }

    private async Task<SendBillingFileViewModel?> CreateViewModel(int runId)
    {
        var runDto = await eprCalculatorApiService.GetCalculatorRun(runId);

        if (runDto == null)
            return null;

        return new SendBillingFileViewModel
        {
            RunId = runId,
            CalcRunName = runDto.RunName,
            IsBillingFileLatest = runDto.BillingFile?.IsLatest == true
        };
    }
}
