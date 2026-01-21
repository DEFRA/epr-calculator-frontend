using EPR.Calculator.Frontend.Common.Constants;
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
        IApiService apiService,
        ICalculatorRunDetailsService calculatorRunDetailsService)
        : BaseController(configuration,
            tokenAcquisition,
            telemetryClient,
            apiService,
            calculatorRunDetailsService)
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

            var runDetails = await this.CalculatorRunDetailsService
                .GetCalculatorRundetailsAsync(this.HttpContext, calculationRunId);

            if (runDetails == null)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, errorControllerName);
            }

            var currentUser = CommonUtil.GetUserName(this.HttpContext);

            var viewModel = new AcceptRejectConfirmationViewModel
            {
                CurrentUser = currentUser,
                CalculationRunId = calculationRunId,
                CalculationRunName = runDetails.RunName,
                Status = BillingStatus.Accepted,
                BackLinkViewModel = new BackLinkViewModel
                {
                    BackLink = ControllerNames.BillingInstructionsController,
                    RunId = calculationRunId,
                    CurrentUser = currentUser,
                },
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
                var currentUser = CommonUtil.GetUserName(this.HttpContext);

                model.BackLinkViewModel = new BackLinkViewModel
                {
                    BackLink = ControllerNames.BillingInstructionsController,
                    RunId = model.CalculationRunId,
                    CurrentUser = currentUser,
                };

                if (this.ModelState.ContainsKey(nameof(model.ApproveData)) && this.ModelState[nameof(model.ApproveData)]?.Errors?.Any() == true)
                {
                    this.ModelState.AddModelError($"Summary_{nameof(model.ApproveData)}", ErrorMessages.AcceptRejectConfirmationApproveDataRequiredSummary); // Summary message
                }

                // Return the same view with the model so errors display
                return this.View(ViewNames.AcceptRejectConfirmationIndex, model);
            }

            if (model.ApproveData is false)
            {
                return this.RedirectToAction(ActionNames.Index, ControllerNames.BillingInstructionsController, routeValues: new { calculationRunId = model.CalculationRunId });
            }

            var requestDto = new ProducerBillingInstructionsHttpPutRequestDto
            {
                OrganisationIds = ARJourneySessionHelper.GetFromSession(this.HttpContext.Session),
                Status = model.Status.ToString(),
                ReasonForRejection = model.Reason,
            };

            var response = await this.ApiService.CallApi(
                this.HttpContext,
                HttpMethod.Put,
                this.ApiService.GetApiUrl(ConfigSection.ProducerBillingInstructions, ConfigSection.ProducerBillingInstructionsV2),
                model.CalculationRunId.ToString(),
                body: requestDto);

            response.EnsureSuccessStatusCode();

            ARJourneySessionHelper.ClearAllFromSession(this.HttpContext.Session);
            this.HttpContext.Session.RemoveKeyIfExists(SessionConstants.IsSelectAll);
            this.HttpContext.Session.RemoveKeyIfExists(SessionConstants.IsSelectAllPage);

            return this.RedirectToAction(ActionNames.Index, ControllerNames.BillingInstructionsController, routeValues: new { calculationRunId = model.CalculationRunId });
        }
    }
}