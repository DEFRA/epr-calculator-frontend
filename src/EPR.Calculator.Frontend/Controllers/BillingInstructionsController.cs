using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Extensions;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Mappers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie;
using Microsoft.Identity.Web;
using System.Reflection;
using System.Text.Json;

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

        [HttpPost("UpdateOrganisationSelection", Name = "BillingInstructions_OrganisationSelection")]
        public async Task<IActionResult> UpdateOrganisationSelectionAsync([FromBody] BillingSelectionOrgDto orgDto)
        {
            try
            {
                // Retrieve the SelectedProducerIds from session or create a new list
                //var selectedProducerIds = this.HttpContext.Session.G<List<int>>(SessionConstants.SelectedProducerIds) ?? new List<int>();

                //if (obj.IsSelected)
                //{
                //    // Add the organisation id if not already present
                //    if (!selectedProducerIds.Contains(model.Id))
                //    {
                //        selectedProducerIds.Add(model.Id);
                //    }
                //}
                //else
                //{
                //    // Remove the organisation id if present
                //    selectedProducerIds.Remove(model.Id);
                //}

                // TODO - the list is immutable - make sure you do a full replace 

                //// Save the updated list back to the session
                //this.HttpContext.Session.SetObject(SessionConstants.SelectedProducerIds, selectedProducerIds);

                return this.Ok();
            }
            catch (Exception ex)
            {
                this.TelemetryClient.TrackException(ex);
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
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
                if (this.HttpContext.Session.Keys.Contains("IsRedirected"))
                {
                    isSelectAllPage = this.HttpContext.Session.GetBooleanFlag(SessionConstants.IsSelectAllPage);
                    this.HttpContext.Session.Remove("IsRedirected");
                }

                var producerBillingInstructionsResponseDto = await this.GetBillingData(calculationRunId, request);
                var billingInstructionsViewModel = mapper.MapToViewModel(
                    producerBillingInstructionsResponseDto ?? new ProducerBillingInstructionsResponseDto(),
                    request,
                    CommonUtil.GetUserName(this.HttpContext),
                    isSelectAll,
                    isSelectAllPage);
                this.HttpContext.Session.SetObject("ProducerIds", billingInstructionsViewModel.ProducerIds);

                foreach (var item in billingInstructionsViewModel.OrganisationBillingInstructions)
                {
                    item.IsSelected = isSelectAll || isSelectAllPage;
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
            this.HttpContext.Session.SetString(SessionConstants.IsSelectAllPage, model.OrganisationSelections.SelectPage.ToString());
            if (model.OrganisationSelections.SelectPage)
            {
                this.HttpContext.Session.SetString("IsRedirected", "true");
            }

            return this.RedirectToRoute("BillingInstructions_Index", new { calculationRunId = model.CalculationRun.Id, page = currentPage, PageSize = pageSize });
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
    }
}
