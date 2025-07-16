using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Extensions;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AcceptRejectConfirmationController"/> class.
    /// </summary>
    [Route("[controller]")]
    public class AcceptRejectConfirmationController(
        IConfiguration configuration,
        ITokenAcquisition tokenAcquisition,
        TelemetryClient telemetryClient,
        IHttpClientFactory httpClientFactory,
        IBillingInstructionsApiService billingInstructionsApiService)
        : BaseController(configuration, tokenAcquisition, telemetryClient, httpClientFactory)
    {
        /// <summary>
        /// Displays the accept reject confirmation controller index view.
        /// </summary>
        /// <param name="calculationRunId">The ID of the calculation run.</param>
        /// <returns>accept reject confirmation index view.</returns>
        [Route("{calculationRunId}")]
        public async Task<IActionResult> IndexAsync(int calculationRunId)
        {
            var errorControllerName = CommonUtil.GetControllerName(typeof(StandardErrorController));

            if (calculationRunId <= 0)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, errorControllerName);
            }

            var runDetails = await this.GetCalculatorRundetails(calculationRunId);

            if (runDetails == null)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, errorControllerName);
            }

            var viewModel = new AcceptRejectConfirmationViewModel
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                CalculationRunId = calculationRunId,
                BackLink = ControllerNames.Organisation,
                CalculationRunName = runDetails.RunName,
                Status = BillingStatus.Accepted,
            };

            return this.View(ViewNames.AcceptRejectConfirmationIndex, viewModel);
        }

        /// <summary>
        /// Handles the submission of accept all selected yes or no billing instructions.
        /// </summary>
        /// <param name="model">The accept reject confirmation view model.</param>
        /// <returns>
        /// A redirection to the organisation view on selection of yes or no.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(AcceptRejectConfirmationViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                // Return the same view with the model so errors display
                return this.View(ViewNames.AcceptRejectConfirmationIndex, model);
            }

            if (model.ApproveData is false)
            {
                return this.RedirectToAction(ActionNames.Index, ControllerNames.BillingInstructionsController, routeValues: new { calculationRunId = model.CalculationRunId });
            }

            try
            {
                var requestDto = new ProducerBillingInstructionsHttpPutRequestDto
                {
                    OrganisationIds = ARJourneySessionHelper.GetFromSession(this.HttpContext.Session),
                    AuthorizationToken = this.HttpContext.Session.GetString("accessToken")!,
                    Status = model.Status.ToString(),
                    ReasonForRejection = model.Reason,
                };

                var isSuccessCode = await billingInstructionsApiService.PutAcceptRejectBillingInstructions(model.CalculationRunId, requestDto);
                if (!isSuccessCode)
                {
                    return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
                }

                ARJourneySessionHelper.ClearAllFromSession(this.HttpContext.Session);
                this.HttpContext.Session.RemoveKeyIfExists(SessionConstants.IsSelectAll);
                this.HttpContext.Session.RemoveKeyIfExists(SessionConstants.IsSelectAllPage);
            }
            catch (Exception ex)
            {
                this.TelemetryClient.TrackException(ex);
                return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }

            return this.RedirectToAction(ActionNames.Index, ControllerNames.BillingInstructionsController, routeValues: new { calculationRunId = model.CalculationRunId });
        }
    }
}