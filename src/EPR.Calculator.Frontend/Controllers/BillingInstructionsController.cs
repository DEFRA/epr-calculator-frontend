using System.Reflection;
using System.Text.Json;
using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
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
        /// <summary>
        /// Displays the billing instructions for a given calculation run.
        /// </summary>
        /// <param name="calculationRunId">The unique identifier for the calculation run.</param>
        /// <param name="request">The pagination and filter request parameters.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> that renders the billing instructions view,
        /// or redirects to the standard error page if the calculation run ID is invalid or an error occurs.
        /// </returns>
        [HttpGet("BillingInstructions/{calculationRunId}", Name = RouteNames.BillingInstructionsIndex)]
        public async Task<IActionResult> IndexAsync([FromRoute] int calculationRunId, [FromQuery] PaginationRequestViewModel request)
        {
            try
            {
                if (calculationRunId <= 0)
                {
                    return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
                }

                var billingData = await this.GetBillingData(calculationRunId, request);

                if (billingData == null)
                {
                    // Optionally log the error here using TelemetryClient or ILogger
                    return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
                }

                var viewModel = mapper.MapToViewModel(billingData, request, CommonUtil.GetUserName(this.HttpContext));

                return this.View(viewModel);
            }
            catch (Exception ex)
            {
                // Optionally log the exception here using TelemetryClient or ILogger
                this.TelemetryClient.TrackException(ex);
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
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

        private async Task<ProducerBillingInstructionsResponseDto?> GetBillingData(
            int calculationRunId,
            PaginationRequestViewModel request)
        {
            var apiUrl = this.ApiService.GetApiUrl(ConfigSection.ProducerBillingInstructions, ConfigSection.ProducerBillingInstructionsV1);

            var requestDto = new ProducerBillingInstructionsRequestDto
            {
                PageNumber = request.Page,
                PageSize = request.PageSize,
                SearchQuery = new ProducerBillingInstructionsSearchQueryDto { OrganisationId = request.OrganisationId },
            };

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
    }
}
