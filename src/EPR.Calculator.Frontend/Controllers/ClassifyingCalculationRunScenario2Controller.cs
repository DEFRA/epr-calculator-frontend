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
    /// <remarks>
    /// Initializes a new instance of the <see cref="ClassifyingCalculationRunScenario2Controller"/> class.
    /// </remarks>
    /// <param name="configuration">The configuration settings.</param>
    /// <param name="tokenAcquisition">token acquisition.</param>
    /// <param name="telemetryClient">telemetry client.</param>
    [Route("[controller]")]
    public class ClassifyingCalculationRunScenario2Controller(
        IConfiguration configuration,
        ITokenAcquisition tokenAcquisition,
        TelemetryClient telemetryClient,
        IHttpClientFactory clientFactory)
        : BaseController(configuration, tokenAcquisition, telemetryClient, clientFactory)
    {

        /// <summary>
        /// Displays the index view for classifying calculation runs.
        /// </summary>
        /// <param name="runId"> runId.</param>
        /// <returns>The index view.</returns>
        [Route("PostFinalClassification/{runId}")]
        [HttpGet]
        public IActionResult Index(int runId)
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
    }
}