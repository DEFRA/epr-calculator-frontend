using System.Text.Json;
using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
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
        [HttpGet("BillingInstructions/{calculationRunId}", Name = "BillingInstructions_Index")]
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

        [HttpPost]
        public IActionResult ProcessSelection(int calculationRunId, [FromForm] OrganisationSelectionsViewModel selections)
        {
            return this.RedirectToAction("Index", new { calculationRunId });
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
                // TODO search query ticket not in main yet
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
