using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Extensions;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EPR.Calculator.Frontend.Controllers;

[Route("[controller]")]
public class AcceptRejectConfirmationController(
    IEprCalculatorApiService eprCalculatorApiService,
    ILogger<AcceptRejectConfirmationController> logger)
    : BaseController
{
    [Route("{runId:int}")]
    public async Task<IActionResult> IndexAsync(int runId)
    {
        var viewModel = await CreateViewModel(runId, BillingStatus.Accepted, null);

        if (viewModel == null)
            return RedirectToError();

        return View(ViewNames.AcceptRejectConfirmationIndex, viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(AcceptRejectConfirmationFormModel model)
    {
        if (!ModelState.IsValid)
        {
            if (ModelState[nameof(model.RunId)] is { ValidationState: ModelValidationState.Invalid })
                return RedirectToError();

            if (ModelState[nameof(model.ApproveData)] is { ValidationState: ModelValidationState.Invalid })
                ModelState.AddModelError($"Summary_{nameof(model.ApproveData)}", ErrorMessages.AcceptRejectConfirmationApproveDataRequiredSummary); // Summary message

            // Redisplay preserving the posted Status/Reason so the Reject flow doesn't fall back to Accept.
            var redisplay = await CreateViewModel(
                model.RunId,
                model.Status ?? BillingStatus.Accepted,
                model.Reason);

            if (redisplay == null)
                return RedirectToError();

            return View(ViewNames.AcceptRejectConfirmationIndex, redisplay);
        }

        if (model.ApproveData is true)
        {
            var requestDto = new ProducerBillingInstructionsHttpPutRequestDto
            {
                OrganisationIds = ARJourneySessionHelper.GetFromSession(HttpContext.Session),
                Status = model.Status!.Value.ToString(),
                ReasonForRejection = model.Reason
            };

            try
            {
                var response = await eprCalculatorApiService.CallApi(
                    HttpMethod.Put,
                    $"v2/producerBillingInstructions/{model.RunId}",
                    body: requestDto);

                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Error updating producer billing instructions for run {RunId}", model.RunId);
                return RedirectToError();
            }

            HttpContext.Session.ClearAllSession();
        }

        return RedirectToAction("Index", "BillingInstructions", new { model.RunId });
    }

    private async Task<AcceptRejectConfirmationViewModel?> CreateViewModel(int runId, BillingStatus status, string? reason)
    {
        var run = await eprCalculatorApiService.GetCalculatorRun(runId);

        if (run == null)
        {
            logger.LogInformation("Calculator run {RunId} was not found; redirecting to error page", runId);
            return null;
        }

        return new AcceptRejectConfirmationViewModel
        {
            RunId = runId,
            RunName = run.RunName,
            Status = status,
            Reason = reason
        };
    }
}
