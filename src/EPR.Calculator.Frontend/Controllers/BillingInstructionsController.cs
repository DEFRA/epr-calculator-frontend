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
