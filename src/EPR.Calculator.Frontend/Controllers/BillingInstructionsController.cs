using System.Text.Json;
using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Extensions;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Mappers;
using EPR.Calculator.Frontend.Models;
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
        IHttpClientFactory clientFactory,
        IBillingInstructionsMapper mapper)
        : BaseController(configuration, tokenAcquisition, telemetryClient, clientFactory)
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

                if (isSelectAll)
                {
                    this.HttpContext.Session.SetObject(SessionConstants.ProducerIds, billingInstructionsViewModel.ProducerIds);
                }

                var existingSelectedIds = this.HttpContext.Session.GetObject<IEnumerable<int>>(SessionConstants.ProducerIds) ?? [];

                if (!isSelectAll && producerBillingInstructionsResponseDto is not null && producerBillingInstructionsResponseDto.Records.TrueForAll(t => existingSelectedIds.Contains(t.ProducerId)))
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

        [HttpPost]
        public IActionResult ClearSelection(int calculationRunId, [FromForm] OrganisationSelectionsViewModel selections)
        {
            return this.RedirectToAction("Index", new { calculationRunId });
        }

        /// <summary>
        /// Handles the POST request to select all billing instructions.
        /// </summary>
        /// <param name="model">The view model containing billing instructions and selection state.</param>
        /// <param name="currentPage">current page.</param>
        /// <param name="pageSize">page size.</param>
        /// <returns>An <see cref="ActionResult"/> that renders the updated view or redirects as appropriate.</returns>
        [HttpPost]
        public IActionResult SelectAll(BillingInstructionsViewModel model, int currentPage, int pageSize)
        {
            this.HttpContext.Session.SetString(SessionConstants.IsSelectAll, model.OrganisationSelections.SelectAll.ToString());
            if (!model.OrganisationSelections.SelectAll)
            {
                this.HttpContext.Session.SetObject(SessionConstants.ProducerIds, new List<int>());
            }

            return this.RedirectToRoute(RouteNames.BillingInstructionsIndex, new { calculationRunId = model.CalculationRun.Id, page = currentPage, PageSize = pageSize });
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
            this.HttpContext.Session.SetString(SessionConstants.IsSelectAllPage, model.OrganisationSelections.SelectPage.ToString());

            var producerBillingInstructionsResponseDto = await this.GetBillingData(model.CalculationRun.Id, new PaginationRequestViewModel() { Page = currentPage, PageSize = pageSize });
            var producerIds = this.UpdateSession(producerBillingInstructionsResponseDto, model.OrganisationSelections.SelectPage);
            this.HttpContext.Session.SetObject(SessionConstants.ProducerIds, producerIds);
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
                // Retrieve the list from session or create a new one if it doesn't exist
                var selectedProducerIds = this.HttpContext.Session.GetObject<List<int>>(SessionConstants.ProducerIds) ?? new List<int>();

                if (orgDto.IsSelected)
                {
                    // Ensure the ID is in the collection
                    if (!selectedProducerIds.Contains(orgDto.Id))
                    {
                        selectedProducerIds.Add(orgDto.Id);
                    }
                    else
                    {
                        // if it already exists we don't want to do anything to the session state
                        // at all and want to return
                        return this.Ok();
                    }
                }
                else
                {
                    // Ensure the ID is not in the collection
                    selectedProducerIds.Remove(orgDto.Id);
                }

                // if we had to add or remove now we need to also falsify the SelectAll and SelectAllPage session flags
                this.HttpContext.Session.SetBooleanFlag(SessionConstants.IsSelectAll, false);

                // todo - we will need to do extra logic on this one, because for a particular page 
                // we may still have selected all the ids and this flag may still be true
                // although there is no real side effect except that the Selectpage checkbox will be reset
                this.HttpContext.Session.SetBooleanFlag(SessionConstants.IsSelectAllPage, false);

                // Overwrite the session object
                this.HttpContext.Session.SetObject(SessionConstants.ProducerIds, selectedProducerIds);

                return this.Ok();
            }
            catch (Exception ex)
            {
                this.TelemetryClient.TrackException(ex);
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        private async Task<ProducerBillingInstructionsResponseDto?> GetBillingData(
            int calculationRunId,
            PaginationRequestViewModel request)
        {
            var apiUrl = this.GetApiUrl(ConfigSection.ProducerBillingInstructions, ConfigSection.ProducerBillingInstructionsV1);

            // Build the request DTO
            var requestDto = new ProducerBillingInstructionsRequestDto
            {
                PageNumber = request.Page,
                PageSize = request.PageSize,
                SearchQuery = new ProducerBillingInstructionsSearchQueryDto { OrganisationId = request.OrganisationId },
            };

            // Pass the calculationRunId as the route argument, and the DTO as the body
            var response = await this.CallApi(HttpMethod.Post, apiUrl, calculationRunId.ToString(), requestDto);

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

        private List<int> UpdateSession(ProducerBillingInstructionsResponseDto? producerBillingInstructionsResponseDto, bool isSelected)
        {
            List<int> producers = [];

            var existingValues = this.HttpContext.Session.GetObject<IEnumerable<int>>(SessionConstants.ProducerIds) ?? [];

            if (existingValues.ToList().Count > 0)
            {
                producers = existingValues.ToList();
            }

            var producersId = producerBillingInstructionsResponseDto?.Records?.Select(t => t.ProducerId).ToList();

            if (producersId != null && producers is not null)
            {
                if (isSelected)
                {
                    producers.AddRange(producersId);
                }
                else
                {
                    producers.RemoveAll(producersId.Contains);
                }
            }

            return producers ?? [];
        }
    }
}
