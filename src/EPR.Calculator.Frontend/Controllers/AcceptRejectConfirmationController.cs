using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

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
        IHttpClientFactory httpClientFactory)
        : BaseController(configuration, tokenAcquisition, telemetryClient, httpClientFactory)
    {
        /// <summary>
        /// Displays the accept reject confirmation controller index view.
        /// </summary>
        /// <param name="calculationRunId">The ID of the calculation run.</param>
        /// <returns>accept reject confirmation index view.</returns>
        [Route("{calculationRunId}")]
        public IActionResult Index(int calculationRunId, AcceptRejectConfirmationViewModel? model)
        {
            if (model != null && !string.IsNullOrEmpty(model.Reason)) // rejected
            {
                // If the model has a reason, it means the user has already submitted a reason for rejection.
                // Redirect to the confirmation view with the provided model.
                return this.View(ViewNames.AcceptRejectConfirmationIndex, model);
            }

            var viewModel = new AcceptRejectConfirmationViewModel()
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                CalculationRunId = calculationRunId,
                BackLink = ControllerNames.Organisation,
                CalculationRunName = "Calculation run 99",
                Status = BillingStatus.Accepted,
            };
            return this.View(ViewNames.AcceptRejectConfirmationIndex, viewModel);
        }

        /// <summary>
        /// Handles the submission of accept all selected yes or no billing instructions.
        /// </summary>
        /// <returns>
        /// A redirection to the organisation view on selection of yes or no.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Submit()
        {
            //validation

            // API call

            return this.RedirectToAction(ActionNames.Index, ControllerNames.Organisation);
        }
    }
}