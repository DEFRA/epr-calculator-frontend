using System.Net;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EPR.Calculator.Frontend.Controllers;

[Route("[controller]")]
public class SendBillingFileController(
    ILogger<SendBillingFileController> logger,
    IEprCalculatorApiService eprCalculatorApiService)
    : BaseController
{
    [Route("{runId:int}")]
    public async Task<IActionResult> Index(int runId)
    {
        var viewModel = await CreateViewModel(runId);

        if (viewModel == null)
            return RedirectToError();

        var redirect = RedirectIfNotShareable(viewModel.Run);

        if (redirect != null)
            return redirect;

        return View(ViewNames.SendBillingFileIndex, viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(ShareBillingFileFormModel model)
    {
        if (!ModelState.IsValid)
        {
            if (ModelState[nameof(model.RunId)] is { ValidationState: ModelValidationState.Invalid })
                return RedirectToError();

            return await Index(model.RunId);
        }

        var response = await eprCalculatorApiService.CallApi(HttpMethod.Post, $"v2/prepareBillingFileSendToFSS/{model.RunId}");

        if (response.StatusCode == HttpStatusCode.Accepted)
            return RedirectToAction(ActionNames.BillingFileSuccess, CommonUtil.GetControllerName(typeof(BillingInstructionsController)), new { runId = model.RunId });

        if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            var viewModel = await CreateViewModel(model.RunId);

            if (viewModel == null)
                return RedirectToError();

            var redirect = RedirectIfNotShareable(viewModel.Run);

            if (redirect != null)
                return redirect;
        }

        logger.LogInformation("1.Request (send billing file) not accepted response code: {StatusCode}", response.StatusCode);
        var contentString = await response.Content.ReadAsStringAsync();
        logger.LogInformation("2.Request (send billing file) not accepted response message: {ContentString}", contentString);
        return RedirectToError();
    }

    private RedirectToActionResult? RedirectIfNotShareable(CalculatorRunDto run)
    {
        if (run.BillingFile?.IsShared == true)
            return RedirectToAction(ActionNames.Index, CommonUtil.GetControllerName(typeof(CompletedRunController)), new { runId = run.RunId });

        if (run.RunClassification is not (RunClassification.Initial or RunClassification.Recalculation))
            return RedirectToError();

        if (run.BillingFile?.IsLatest != true)
            return RedirectToAction(ActionNames.Index, ControllerNames.CalculationRunOverview, new { runId = run.RunId });

        return null;
    }

    private async Task<ShareBillingFileViewModel?> CreateViewModel(int runId)
    {
        var runDto = await eprCalculatorApiService.GetCalculatorRun(runId);

        if (runDto == null)
            return null;

        return new ShareBillingFileViewModel
        {
            RunId = runId,
            Run = runDto
        };
    }
}
