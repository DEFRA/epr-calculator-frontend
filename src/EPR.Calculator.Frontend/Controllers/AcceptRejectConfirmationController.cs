using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Services;
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
        IApiService apiService,
        ICalculatorRunDetailsService calculatorRunDetailsService)
        : BaseController(configuration,
            tokenAcquisition, telemetryClient,
            apiService,
            calculatorRunDetailsService)
    {
        /// <summary>
        /// Displays the accept reject confirmation controller index view.
        /// </summary>
        /// <param name="runId">The ID of the calculation run.</param>
        /// <returns>accept reject confirmation index view.</returns>
        [Route("{runId}")]
        public IActionResult Index(int runId)
        {
            var viewModel = new AcceptRejectConfirmationViewModel()
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                RunId = runId,
                BackLink = ControllerNames.Organisation,
                CalculationRunTitle = "Calculation run 99",
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
            return this.RedirectToAction(ActionNames.Index, ControllerNames.Organisation);
        }
    }
}