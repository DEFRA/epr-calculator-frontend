using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Extensions;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Mappers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers;

public class BillingInstructionsController(
    TelemetryClient telemetryClient,
    IBillingInstructionsMapper mapper,
    IEprCalculatorApiService eprCalculatorApiService)
    : BaseController
{
    /// <summary>
    ///     Handles the HTTP GET request to display billing instructions for the specified calculation run.
    /// </summary>
    /// <param name="runId">The ID of the calculation run to retrieve billing data for.</param>
    /// <param name="request">The pagination and filtering parameters.</param>
    /// <returns>
    ///     An <see cref="IActionResult" /> that renders the billing instructions view,
    ///     or redirects to a standard error page if the calculation run ID is invalid.
    /// </returns>
    [HttpGet("BillingInstructions/{runId}", Name = RouteNames.BillingInstructionsIndex)]
    public async Task<IActionResult> IndexAsync([FromRoute] int runId, [FromQuery] BillingInstructionsIndexModel request)
    {
        if (runId <= 0)
            throw new ArgumentOutOfRangeException(nameof(runId), "RunId must be greater than zero.");

        var isSelectAll = HttpContext.Session.GetBooleanFlag(SessionConstants.IsSelectAll);
        var isSelectAllPage = false;
        if (HttpContext.Session.Keys.Contains(SessionConstants.IsRedirected))
        {
            isSelectAllPage = HttpContext.Session.GetBooleanFlag(SessionConstants.IsSelectAllPage);
            HttpContext.Session.Remove(SessionConstants.IsRedirected);
        }

        var producerBillingInstructionsResponseDto = await GetBillingData(runId, request);
        var billingInstructionsViewModel = mapper.MapToViewModel(
            producerBillingInstructionsResponseDto,
            request,
            isSelectAll,
            isSelectAllPage);

        if (isSelectAll &&
            billingInstructionsViewModel.ProducerIds != null &&
            billingInstructionsViewModel.ProducerIds.Any())
            ARJourneySessionHelper.AddToSession(HttpContext.Session, billingInstructionsViewModel.ProducerIds);

        var existingSelectedIds = ARJourneySessionHelper.GetFromSession(HttpContext.Session);

        if (!isSelectAll &&
            producerBillingInstructionsResponseDto?.Records.Count > 0 &&
            producerBillingInstructionsResponseDto.Records.TrueForAll(t => existingSelectedIds.Contains(t.ProducerId)) &&
            billingInstructionsViewModel.TotalRecords > 0)
        {
            billingInstructionsViewModel.OrganisationSelections.SelectPage = true;
            HttpContext.Session.SetString(SessionConstants.IsSelectAllPage, "true");
        }

        foreach (var item in billingInstructionsViewModel.SelectedRows.Where(t => existingSelectedIds.Contains(t.OrganisationId)))
            item.IsSelected = true;

        return View(billingInstructionsViewModel);
    }

    /// <summary>
    ///     Handles the selection of organisations and redirects to the Index action for the specified calculation run.
    /// </summary>
    /// <param name="runId">The unique identifier for the calculation run.</param>
    /// <param name="selections">The selected organisation IDs from the form.</param>
    /// <returns>
    ///     An <see cref="IActionResult" /> that redirects to the Index action for the specified calculation run.
    /// </returns>
    [HttpPost]
    public IActionResult ProcessSelection(int runId, [FromForm] OrganisationSelectionsViewModel selections)
    {
        return RedirectToAction("Index", new { runId });
    }

    /// <summary>
    ///     Clears all selected billing instructions from the session and redirects to the Billing Instructions index page.
    /// </summary>
    /// <returns>A redirect to the Billing Instructions index route.</returns>
    [HttpPost]
    public IActionResult ClearSelection(BillingInstructionsClearModel model)
    {
        HttpContext.Session.ClearAllSession();
        return RedirectToRoute(RouteNames.BillingInstructionsIndex, model);
    }

    /// <summary>
    ///     Redirects to the Accept/Reject Confirmation index action for the specified calculation run.
    /// </summary>
    /// <param name="runId">The ID of the calculation run.</param>
    /// <returns>A redirect to the Accept/Reject Confirmation controller's index action.</returns>
    [HttpPost]
    public IActionResult AcceptSelected(int runId)
    {
        return RedirectToAction(ActionNames.Index, ControllerNames.AcceptRejectConfirmationController, new { runId });
    }

    /// <summary>
    ///     Redirects to the Reason for Rejection index action for the specified calculation run.
    /// </summary>
    /// <param name="runId">The ID of the calculation run.</param>
    /// <returns>A redirect to the Reason for Rejection controller's index action.</returns>
    [HttpPost]
    public IActionResult RejectSelected(int runId)
    {
        return RedirectToAction(ActionNames.Index, ControllerNames.ReasonForRejectionController, new { runId });
    }

    [HttpPost]
    public IActionResult SelectAll(BillingInstructionsSelectModel model)
    {
        HttpContext.Session.SetString(SessionConstants.IsSelectAll, model.OrganisationSelections.SelectAll.ToString());
        if (!model.OrganisationSelections.SelectAll)
        {
            ARJourneySessionHelper.ClearAllFromSession(HttpContext.Session);

            // If they just unselected all we also want to untick the select all page
            HttpContext.Session.SetString(SessionConstants.IsSelectAll, "false");
        }

        return RedirectToRoute(RouteNames.BillingInstructionsIndex, model);
    }

    [HttpPost]
    public async Task<IActionResult> SelectAllPage(BillingInstructionsSelectModel model)
    {
        // Sets the SelectAllPage flag to either true or false based on the model's selection state.
        HttpContext.Session.SetString(SessionConstants.IsSelectAllPage, model.OrganisationSelections.SelectPage.ToString());

        // Makes an api request to get the instructions for the current calculation run.
        var producerBillingInstructionsResponseDto = await GetBillingData(model.RunId, model);

        var producerIdsFromResponse = producerBillingInstructionsResponseDto?.Records
            .Where(t => t.SuggestedBillingInstruction != BillingInstructionConstants.NoSuggestedBillingInstructionPlaceholder)
            .Select(t => t.ProducerId)
            .ToList();

        if (producerIdsFromResponse != null)
        {
            if (model.OrganisationSelections.SelectPage)
            {
                // If the SelectPage is true, add the producer IDs to the session.
                ARJourneySessionHelper.AddToSession(HttpContext.Session, producerIdsFromResponse);
            }
            else
            {
                // If the SelectPage is false, remove the producer IDs from the session.
                ARJourneySessionHelper.ClearAllFromSession(HttpContext.Session);
            }
        }

        HttpContext.Session.SetString(SessionConstants.IsRedirected, "true");
        return RedirectToRoute(RouteNames.BillingInstructionsIndex, model);
    }

    /// <summary>
    ///     Handles AJAX updates to the selected organisation IDs in the user's session.
    ///     Adds or removes the organisation ID from the session-based collection depending on the IsSelected flag.
    /// </summary>
    /// <param name="orgDto">
    ///     The <see cref="BillingSelectionOrgDto" /> containing the organisation ID and selection state.
    /// </param>
    /// <returns>
    ///     <see cref="IActionResult" /> indicating the result of the operation.
    ///     Returns 200 OK on success, or 500 Internal Server Error if an exception occurs.
    /// </returns>
    [HttpPost("UpdateOrganisationSelection", Name = "BillingInstructions_OrganisationSelection")]
    public IActionResult UpdateOrganisationSelectionAsync([FromBody] BillingSelectionOrgDto orgDto)
    {
        if (orgDto.IsSelected)
            ARJourneySessionHelper.AddToSession(HttpContext.Session, [orgDto.Id]);
        else
            ARJourneySessionHelper.RemoveFromSession(HttpContext.Session, [orgDto.Id]);

        // If they just unselected an item then it clearly means we must untick the all is selected flag
        if (!orgDto.IsSelected)
            HttpContext.Session.SetBooleanFlag(SessionConstants.IsSelectAll, false);

        return Ok();
    }

    /// <summary>
    ///     Generate the billing file.
    /// </summary>
    /// <param name="runId">The unique identifier for the calculation run.</param>
    [HttpPost]
    public async Task<IActionResult> GenerateDraftBillingFile(int runId)
    {
        var result = await TryGenerateBillingFile(runId);

        if (result)
        {
            return RedirectToRoute(new
            {
                controller = ControllerNames.CalculationRunOverview,
                action = "Index",
                runId
            });
        }

        throw new InvalidOperationException($"Failed to generate draft billing file for calculation run {runId}.");
    }

    /// <summary>
    ///     Displays a billing file sent confirmation screen.
    /// </summary>
    /// <returns>Billing file sent page.</returns>
    [Route("BillingFileSuccess")]
    public IActionResult BillingFileSuccess()
    {
        var model = new BillingFileSuccessViewModel
        {
            ConfirmationViewModel = new ConfirmationViewModel
            {
                Title = ConfirmationMessages.BillingFileSuccessTitle,
                Body = ConfirmationMessages.BillingFileSuccessBody,
                AdditionalParagraphs = ConfirmationMessages.BillingFileSuccessAdditionalParagraphs,
                RedirectController = ControllerNames.Dashboard
            }
        };

        return View(ViewNames.BillingConfirmationSuccess, model);
    }

    private async Task<ProducerBillingInstructionsResponseDto?> GetBillingData(int runId, BillingInstructionsIndexModel request)
    {
        var requestDto = new ProducerBillingInstructionsRequestDto
        {
            PageNumber = request.Page,
            PageSize = request.PageSize,
            SearchQuery = new ProducerBillingInstructionsSearchQueryDto
            {
                OrganisationId     = request.OrganisationId,
                BillingInstruction = request.BillingInstructions.Select(x => x.ToString()),
                Status             = request.BillingStatuses.Select(x => x.ToString())
            }
        };

        var response = await eprCalculatorApiService.CallApi(
            HttpMethod.Post,
            $"v1/producerBillingInstructions/{runId}",
            body: requestDto);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            telemetryClient.TrackTrace($"BillingInstructions API call failed. StatusCode: {response.StatusCode} content: {errorContent}");
            throw new HttpRequestException(
                $"BillingInstructions API call failed for run {runId}. StatusCode: {response.StatusCode}. Content: {errorContent}");
        }

        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ProducerBillingInstructionsResponseDto>(json)!;
    }

    private async Task<bool> TryGenerateBillingFile(int runId)
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
}
