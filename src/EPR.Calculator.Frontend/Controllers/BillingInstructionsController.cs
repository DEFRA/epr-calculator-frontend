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
using System.Text.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Organisation Controller.
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
        [HttpGet("BillingInstructions/{calculationRunId}", Name = "BillingInstructions_Index")]
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

                var billingData = await this.GetBillingData(calculationRunId, request);
                var viewModel1 = mapper.MapToViewModel(billingData, request, CommonUtil.GetUserName(this.HttpContext), isSelectAll, isSelectAllPage);
                this.HttpContext.Session.SetObject("ProducerIds", viewModel1.ProducerIds);

                foreach (var item in viewModel1.OrganisationBillingInstructions)
                {
                    item.IsSelected = isSelectAll || isSelectAllPage;
                }

                return this.View(viewModel1);
            }
            catch (Exception ex)
            {
                this.TelemetryClient.TrackException(ex);
                return this.RedirectToStandardError;
            }
        }

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
        /// <param name="request">Pagination parameters for the current page of billing instructions.</param>
        /// <returns>An <see cref="ActionResult"/> that renders the updated view or redirects as appropriate.</returns>
        [HttpPost]
        public async Task<IActionResult> SelectAll(BillingInstructionsViewModel model, int currentPage, int pageSize)
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
