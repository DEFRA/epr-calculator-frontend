using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Extensions;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

[Route("[controller]")]
public class AcceptRejectConfirmationController(
    IEprCalculatorApiService eprCalculatorApiService,
    ICalculatorRunDetailsService calculatorRunDetailsService)
    : BaseController
{
    /// <summary>
    ///     Displays the accept reject confirmation controller index view.
    /// </summary>
    /// <param name="runId">The ID of the calculation run.</param>
    /// <returns>accept reject confirmation index view.</returns>
    [Route("{runId}")]
    public async Task<IActionResult> IndexAsync(int runId)
    {
        if (runId <= 0)
            return RedirectToError();

        var runDetails = await calculatorRunDetailsService
            .GetCalculatorRundetailsAsync(HttpContext, runId);

        if (runDetails == null)
            return RedirectToError();

        var currentUser = CommonUtil.GetUserName(HttpContext);

        var viewModel = new AcceptRejectConfirmationViewModel
        {
            CurrentUser = currentUser,
            CalculationRunId = runId,
            CalculationRunName = runDetails.RunName,
            Status = BillingStatus.Accepted,
            BackLinkViewModel = new BackLinkViewModel
            {
                BackLink = ControllerNames.BillingInstructionsController,
                RunId = runId,
                CurrentUser = currentUser
            }
        };

        return View(ViewNames.AcceptRejectConfirmationIndex, viewModel);
    }

    /// <summary>
    ///     Handles the submission of accept all selected yes or no billing instructions.
    /// </summary>
    /// <param name="model">The accept reject confirmation view model.</param>
    /// <returns>
    ///     A redirection to the organisation view on selection of yes or no.
    /// </returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(AcceptRejectConfirmationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var currentUser = CommonUtil.GetUserName(HttpContext);

            model.BackLinkViewModel = new BackLinkViewModel
            {
                BackLink = ControllerNames.BillingInstructionsController,
                RunId = model.CalculationRunId,
                CurrentUser = currentUser
            };

            if (ModelState.ContainsKey(nameof(model.ApproveData)) && ModelState[nameof(model.ApproveData)]?.Errors?.Any() == true)
                ModelState.AddModelError($"Summary_{nameof(model.ApproveData)}", ErrorMessages.AcceptRejectConfirmationApproveDataRequiredSummary); // Summary message

            // Return the same view with the model so errors display
            return View(ViewNames.AcceptRejectConfirmationIndex, model);
        }

        if (model.ApproveData is false)
            return RedirectToAction(ActionNames.Index, ControllerNames.BillingInstructionsController, new { runId = model.CalculationRunId });

        var requestDto = new ProducerBillingInstructionsHttpPutRequestDto
        {
            OrganisationIds = ARJourneySessionHelper.GetFromSession(HttpContext.Session),
            Status = model.Status.ToString(),
            ReasonForRejection = model.Reason
        };

        var response = await eprCalculatorApiService.CallApi(
            HttpContext,
            HttpMethod.Put,
            $"v2/producerBillingInstructions/{model.CalculationRunId}",
            body: requestDto);

        response.EnsureSuccessStatusCode();

        ARJourneySessionHelper.ClearAllFromSession(HttpContext.Session);
        HttpContext.Session.RemoveKeyIfExists(SessionConstants.IsSelectAll);
        HttpContext.Session.RemoveKeyIfExists(SessionConstants.IsSelectAllPage);

        return RedirectToAction(ActionNames.Index, ControllerNames.BillingInstructionsController, new { runId = model.CalculationRunId });
    }
}
