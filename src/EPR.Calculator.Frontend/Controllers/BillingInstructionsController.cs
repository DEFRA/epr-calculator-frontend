using System.Text.Json;
using AspNetCoreGeneratedDocument;
using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Extensions;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Mappers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Billing Instructions Controller.
    /// </summary>
    public class BillingInstructionsController(
        IConfiguration configuration,
        ITokenAcquisition tokenAcquisition,
        TelemetryClient telemetryClient,
        IBillingInstructionsMapper mapper,
        IApiService apiService,
        ICalculatorRunDetailsService calculatorRunDetailsService)
        : BaseController(configuration, tokenAcquisition, telemetryClient, apiService, calculatorRunDetailsService)
    {
        private IActionResult RedirectToStandardError
        {
            get
            {
                var controllerName = CommonUtil.GetControllerName(typeof(StandardErrorController));
                return this.RedirectToAction(ActionNames.StandardErrorIndex, controllerName);
            }
        }

        /// <summary>
        /// Handles the HTTP GET request to display billing instructions for the specified calculation run.
        /// </summary>
        /// <param name="calculationRunId">The ID of the calculation run to retrieve billing data for.</param>
        /// <param name="request">The pagination and filtering parameters.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> that renders the billing instructions view,
        /// or redirects to a standard error page if the calculation run ID is invalid.
        /// </returns>
        [HttpGet("BillingInstructions/{calculationRunId}", Name = RouteNames.BillingInstructionsIndex)]
        public async Task<IActionResult> IndexAsync([FromRoute] int calculationRunId, [FromQuery] PaginationRequestViewModel request)
        {
            if (calculationRunId <= 0)
            {
                return this.RedirectToStandardError;
            }

            try
            {
                bool isSelectAll = this.HttpContext.Session.GetBooleanFlag(SessionConstants.IsSelectAll);
                bool isSelectAllPage = false;
                if (this.HttpContext.Session.Keys.Contains(SessionConstants.IsRedirected))
                {
                    isSelectAllPage = this.HttpContext.Session.GetBooleanFlag(SessionConstants.IsSelectAllPage);
                    this.HttpContext.Session.Remove(SessionConstants.IsRedirected);
                }

                var producerBillingInstructionsResponseDto = await this.GetBillingData(calculationRunId, request);
                var billingInstructionsViewModel = mapper.MapToViewModel(
                    producerBillingInstructionsResponseDto ?? new ProducerBillingInstructionsResponseDto(),
                    request,
                    CommonUtil.GetUserName(this.HttpContext),
                    isSelectAll,
                    isSelectAllPage);

                if (isSelectAll &&
                    billingInstructionsViewModel.ProducerIds != null &&
                    billingInstructionsViewModel.ProducerIds.Any())
                {
                    ARJourneySessionHelper.AddToSession(this.HttpContext.Session, billingInstructionsViewModel.ProducerIds);
                }

                var existingSelectedIds = ARJourneySessionHelper.GetFromSession(this.HttpContext.Session);

                if (!isSelectAll &&
                    producerBillingInstructionsResponseDto is not null &&
                    producerBillingInstructionsResponseDto.Records.TrueForAll(t => existingSelectedIds.Contains(t.ProducerId)) && billingInstructionsViewModel.TotalRecords > 0)
                {
                    billingInstructionsViewModel.OrganisationSelections.SelectPage = true;
                    this.HttpContext.Session.SetString(SessionConstants.IsSelectAllPage, "true");
                }

                foreach (var item in billingInstructionsViewModel.OrganisationBillingInstructions.Where(t => existingSelectedIds.Contains(t.OrganisationId)))
                {
                    item.IsSelected = true;
                }

                return this.View(billingInstructionsViewModel);
            }
            catch (Exception ex)
            {
                this.TelemetryClient.TrackException(ex);
                return this.RedirectToStandardError;
            }
        }

        /// <summary>
        /// Handles the selection of organisations and redirects to the Index action for the specified calculation run.
        /// </summary>
        /// <param name="calculationRunId">The unique identifier for the calculation run.</param>
        /// <param name="selections">The selected organisation IDs from the form.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> that redirects to the Index action for the specified calculation run.
        /// </returns>
        [HttpPost]
        public IActionResult ProcessSelection(int calculationRunId, [FromForm] OrganisationSelectionsViewModel selections)
        {
            return this.RedirectToAction("Index", new { calculationRunId });
        }

        /// <summary>
        /// Clears all selected billing instructions from the session and redirects to the Billing Instructions index page.
        /// </summary>
        /// <param name="calculationRunId">The ID of the calculation run.</param>
        /// <param name="currentPage">The current page number for pagination.</param>
        /// <param name="pageSize">The number of items displayed per page.</param>
        /// <returns>A redirect to the Billing Instructions index route.</returns>
        [HttpPost]
        public IActionResult ClearSelection(int calculationRunId, int currentPage, int pageSize)
        {
            this.ClearAllSession();
            return this.RedirectToRoute(RouteNames.BillingInstructionsIndex, new { calculationRunId = calculationRunId, page = currentPage, PageSize = pageSize });
        }

        /// <summary>
        /// Redirects to the Accept/Reject Confirmation index action for the specified calculation run.
        /// </summary>
        /// <param name="calculationRunId">The ID of the calculation run.</param>
        /// <returns>A redirect to the Accept/Reject Confirmation controller's index action.</returns>
        [HttpPost]
        public IActionResult AcceptSelected(int calculationRunId)
        {
            return this.RedirectToAction(ActionNames.Index, ControllerNames.AcceptRejectConfirmationController, new { calculationRunId });
        }

        /// <summary>
        /// Redirects to the Reason for Rejection index action for the specified calculation run.
        /// </summary>
        /// <param name="calculationRunId">The ID of the calculation run.</param>
        /// <returns>A redirect to the Reason for Rejection controller's index action.</returns>
        [HttpPost]
        public IActionResult RejectSelected(int calculationRunId)
        {
            this.TempData.Clear();
            return this.RedirectToAction(ActionNames.Index, ControllerNames.ReasonForRejectionController, new { calculationRunId });
        }

        /// <summary>
        /// Handles the POST request to select all billing instructions.
        /// </summary>
        /// <param name="model">The view model containing billing instructions and selection state.</param>
        /// <param name="currentPage">current page.</param>
        /// <param name="pageSize">page size.</param>
        /// <param name="organisationId">organisation Id.</param>
        /// <returns>An <see cref="ActionResult"/> that renders the updated view or redirects as appropriate.</returns>
        [HttpPost]
        public IActionResult SelectAll(BillingInstructionsViewModel model, int currentPage, int pageSize, int? organisationId)
        {
            this.HttpContext.Session.SetString(SessionConstants.IsSelectAll, model.OrganisationSelections.SelectAll.ToString());
            if (!model.OrganisationSelections.SelectAll)
            {
                ARJourneySessionHelper.ClearAllFromSession(this.HttpContext.Session);

                // If they just unselected all we also want to untick the select all page
                this.HttpContext.Session.SetString(SessionConstants.IsSelectAll, "false");
            }

            return this.RedirectToRoute(RouteNames.BillingInstructionsIndex, new { calculationRunId = model.CalculationRun.Id, page = currentPage, PageSize = pageSize, OrganisationId = organisationId });
        }

        /// <summary>
        /// Handles the POST request to select all billing instructions.
        /// </summary>
        /// <param name="model">The view model containing billing instructions and selection state.</param>
        /// <param name="currentPage">current page.</param>
        /// <param name="pageSize">page size.</param>
        /// <returns>An <see cref="ActionResult"/> that renders the updated view or redirects as appropriate.</returns>
        [HttpPost]
        public async Task<IActionResult> SelectAllPage(BillingInstructionsViewModel model, int currentPage, int pageSize)
        {
            // Sets the SelectAllPage flag to either true or false based on the model's selection state.
            this.HttpContext.Session.SetString(SessionConstants.IsSelectAllPage, model.OrganisationSelections.SelectPage.ToString());

            // Makes an api request to get the instructions for the current calculation run.
            var producerBillingInstructionsResponseDto =
                await this.GetBillingData(model.CalculationRun.Id, new PaginationRequestViewModel() { Page = currentPage, PageSize = pageSize });

            var producerIdsFromResponse =
                producerBillingInstructionsResponseDto?.Records?.Select(t => t.ProducerId).ToList();

            if (producerIdsFromResponse != null)
            {
                if (model.OrganisationSelections.SelectPage)
                {
                    // If the SelectPage is true, add the producer IDs to the session.
                    ARJourneySessionHelper.AddToSession(this.HttpContext.Session, producerIdsFromResponse);
                }
                else
                {
                    // If the SelectPage is false, remove the producer IDs from the session.
                    ARJourneySessionHelper.ClearAllFromSession(this.HttpContext.Session);
                }
            }

            this.HttpContext.Session.SetString(SessionConstants.IsRedirected, "true");
            return this.RedirectToRoute(RouteNames.BillingInstructionsIndex, new { calculationRunId = model.CalculationRun.Id, page = currentPage, PageSize = pageSize });
        }

        /// <summary>
        /// Handles AJAX updates to the selected organisation IDs in the user's session.
        /// Adds or removes the organisation ID from the session-based collection depending on the IsSelected flag.
        /// </summary>
        /// <param name="orgDto">
        /// The <see cref="BillingSelectionOrgDto"/> containing the organisation ID and selection state.
        /// </param>
        /// <returns>
        /// <see cref="IActionResult"/> indicating the result of the operation.
        /// Returns 200 OK on success, or 500 Internal Server Error if an exception occurs.
        /// </returns>
        [HttpPost("UpdateOrganisationSelection", Name = "BillingInstructions_OrganisationSelection")]
        public IActionResult UpdateOrganisationSelectionAsync([FromBody] BillingSelectionOrgDto orgDto)
        {
            try
            {
                if (orgDto.IsSelected)
                {
                    ARJourneySessionHelper.AddToSession(this.HttpContext.Session, [orgDto.Id]);
                }
                else
                {
                    ARJourneySessionHelper.RemoveFromSession(this.HttpContext.Session, [orgDto.Id]);
                }

                // If they just unselected an item then it clearly means we must untick the all is selected flag
                if (!orgDto.IsSelected)
                {
                    this.HttpContext.Session.SetBooleanFlag(SessionConstants.IsSelectAll, false);
                }

                return this.Ok();
            }
            catch (Exception ex)
            {
                this.TelemetryClient.TrackException(ex);
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Generate the billing file.
        /// </summary>
        /// <param name="runId">The unique identifier for the calculation run.</param>
        [HttpPost]
        public async Task<IActionResult> GenerateDraftBillingFile(int runId)
        {
            var result = await this.TryGenerateBillingFile(runId);
            if (result)
            {
                return this.RedirectToRoute(new
                {
                    controller = ControllerNames.CalculationRunOverview,
                    action = "Index",
                    runId,
                });
            }
            else
            {
                return this.RedirectToStandardError;
            }
        }

        /// <summary>
        /// Displays a billing file sent confirmation screen.
        /// </summary>
        /// <returns>Billing file sent page.</returns>
        [Route("BillingFileSuccess")]
        public IActionResult BillingFileSuccess()
        {
            var model = new BillingFileSuccessViewModel
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                ConfirmationViewModel = new ConfirmationViewModel
                {
                    Title = ConfirmationMessages.BillingFileSuccessTitle,
                    Body = ConfirmationMessages.BillingFileSuccessBody,
                    AdditionalParagraphs = ConfirmationMessages.BillingFileSuccessAdditionalParagraphs,
                    RedirectController = ControllerNames.Dashboard,
                },
            };

            return this.View(ViewNames.BillingConfirmationSuccess, model);
        }

        private void ClearAllSession()
        {
            var session = this.HttpContext.Session;
            ARJourneySessionHelper.ClearAllFromSession(session);
            session.RemoveKeyIfExists(SessionConstants.IsSelectAll);
            session.RemoveKeyIfExists(SessionConstants.IsSelectAllPage);
        }

        private async Task<ProducerBillingInstructionsResponseDto?> GetBillingData(
            int calculationRunId,
            PaginationRequestViewModel request)
        {
            var apiUrl = this.ApiService.GetApiUrl(ConfigSection.ProducerBillingInstructions, ConfigSection.ProducerBillingInstructionsV1);

            // Build the request DTO
            var requestDto = new ProducerBillingInstructionsRequestDto
            {
                PageNumber = request.Page,
                PageSize = request.PageSize,
                SearchQuery = new ProducerBillingInstructionsSearchQueryDto
                {
                    OrganisationId = request.OrganisationId,
                    Status = request.BillingStatus.HasValue ? new List<string> { request.BillingStatus.Value.ToString() } : null,
                },
            };

            // Pass the calculationRunId as the route argument, and the DTO as the body
            var response = await this.ApiService.CallApi(
                this.HttpContext,
                HttpMethod.Post,
                apiUrl,
                calculationRunId.ToString(),
                body: requestDto);

            if (!response.IsSuccessStatusCode)
            {
                this.TelemetryClient.TrackTrace($"BillingInstructions API call failed. StatusCode: {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync();
                this.TelemetryClient.TrackTrace($"BillingInstructions API error content: {errorContent}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var billingData = JsonSerializer.Deserialize<ProducerBillingInstructionsResponseDto>(json);

            return billingData;
        }

        private async Task<bool> TryGenerateBillingFile(int runId)
        {
            var acceptApiUrl = this.ApiService.GetApiUrl(
                ConfigSection.CalculationRunSettings,
                ConfigSection.ProducerBillingInstructionsAcceptApi);

            var responseDto = await this.ApiService.CallApi(
                this.HttpContext,
                HttpMethod.Put,
                acceptApiUrl,
                runId.ToString(),
                null);

            if (!responseDto.IsSuccessStatusCode)
            {
                this.TelemetryClient.TrackTrace($"Billing instructions acceptance failed for RunId {runId}. StatusCode: {responseDto.StatusCode}, Reason: {responseDto.ReasonPhrase}");
                return false;
            }

            return true;
        }
    }
}
