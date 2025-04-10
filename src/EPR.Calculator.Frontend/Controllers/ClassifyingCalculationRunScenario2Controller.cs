using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClassifyingCalculationRunScenario2Controller"/> class.
    /// </summary>
    public class ClassifyingCalculationRunScenario2Controller : BaseController
    {
        private readonly ILogger<ClassifyingCalculationRunScenario2Controller> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassifyingCalculationRunScenario2Controller"/> class.
        /// </summary>
        /// <param name="configuration">The configuration settings.</param>
        /// <param name="clientFactory">The HTTP client factory.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="tokenAcquisition">token acquisition.</param>
        /// <param name="telemetryClient">telemetry client.</param>
        public ClassifyingCalculationRunScenario2Controller(IConfiguration configuration, IHttpClientFactory clientFactory, ILogger<ClassifyingCalculationRunScenario2Controller> logger, ITokenAcquisition tokenAcquisition, TelemetryClient telemetryClient)
            : base(configuration, tokenAcquisition, telemetryClient)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Displays the index view for classifying calculation runs.
        /// </summary>
        /// <param name="runId"> runId.</param>
        /// <returns>The index view.</returns>
        [Route("ClassifyingCalculationRunScenario2/{runId}")]
        public IActionResult Index(int runId)
        {
            try
            {
                var classifyCalculationRunViewModel = new ClassifyCalculationRunScenerio2ViewModel
                {
                    CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                    CalculatorRunStatus = new CalculatorRunStatusUpdateDto
                    {
                        RunId = 240008,
                        ClassificationId = 3,
                        CalcName = "Calculation run 99",
                        CreatedDate = "01 May 2024",
                        CreatedTime = "12:09",
                        FinancialYear = "2024-25",
                    },
                };

                return this.View(ViewNames.ClassifyingCalculationRunScenario2Index, classifyCalculationRunViewModel);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "An error occurred while processing the request.");
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
        }
    }
}