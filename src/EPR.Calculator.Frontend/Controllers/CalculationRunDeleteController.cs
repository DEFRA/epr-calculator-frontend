using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CalculationRunDeleteController"/> class.
    /// </summary>
    /// <param name="configuration">The configuration settings.</param>
    /// <param name="clientFactory">The HTTP client factory.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="tokenAcquisition">The token acquisition service.</param>
    /// <param name="telemetryClient">The telemetry client for logging and monitoring.</param>
    [Authorize(Roles = "SASuperUser")]
    [Route("[controller]")]
    public class CalculationRunDeleteController(
        IConfiguration configuration,
        IHttpClientFactory clientFactory,
        ILogger<CalculationRunDeleteController> logger,
        ITokenAcquisition tokenAcquisition,
        TelemetryClient telemetryClient)
        : BaseController(configuration, tokenAcquisition, telemetryClient, clientFactory)
    {
        /// <summary>
        /// Displays the calculate run delete confirmation screen.
        /// </summary>
        /// <param name="runId">The ID of the calculation run.</param>
        /// <returns>The delete confirmation view.</returns>
        [Route("delete-confirmation/{runId}")]
        public IActionResult Index(int runId)
        {
            var calculatorRunStatusUpdate = new CalculatorRunStatusUpdateDto
            {
                RunId = runId,
                CalcName = "Calculation Run Name",
                ClassificationId = (int)RunClassification.DELETED,
            };
            var calculationRunDeleteViewModel = new CalculationRunDeleteViewModel
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                CalculatorRunStatusData = calculatorRunStatusUpdate,
            };
            return this.View(ViewNames.CalculationRunDeleteIndex, calculationRunDeleteViewModel);
        }

        /// <summary>
        /// Displays the calculate run delete confirmation screen.
        /// </summary>
        /// <returns>The delete confirmation success view.</returns>
        [Route("confirmation-success")]
        public IActionResult DeleteConfirmationSuccess()
        {
            var currentUser = CommonUtil.GetUserName(this.HttpContext);
            return this.View(ViewNames.CalculationRunDeleteConfirmationSuccess, model: currentUser);
        }
    }
}