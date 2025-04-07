using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClassifyingCalculationRunScenario1Controller"/> class.
    /// </summary>
    /// <param name="configuration">The configuration settings.</param>
    /// <param name="clientFactory">The HTTP client factory.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="tokenAcquisition">token acquisition.</param>
    /// <param name="telemetryClient">telemetry client.</param>
    public class ClassifyingCalculationRunScenario1Controller(
        IConfiguration configuration,
        IHttpClientFactory clientFactory,
        ILogger<ClassifyingCalculationRunScenario1Controller> logger,
        ITokenAcquisition tokenAcquisition,
        TelemetryClient telemetryClient)
        : BaseController(configuration, tokenAcquisition, telemetryClient, clientFactory)
    {
        private const string ClassifyingCalculationRunIndexView = ViewNames.ClassifyingCalculationRunScenario1Index;
        private readonly ILogger<ClassifyingCalculationRunScenario1Controller> logger = logger;

        /// <summary>
        /// Displays the index view for classifying calculation runs.
        /// </summary>
        /// <param name="runId"> runId.</param>
        /// <returns>The index view.</returns>
        [Route("ClassifyingCalculationRunScenario1/{runId}")]
        public IActionResult Index(int runId)
        {
            try
            {
                var classifyCalculationRunViewModel = new ClassifyCalculationRunScenerio1ViewModel
                {
                    CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                    CalculatorRunStatus = new CalculatorRunStatusUpdateDto
                    {
                        RunId = 240008,
                        ClassificationId = 3, // TODO: Replace with actual data,
                        CalcName = "Calculation run 99", // TODO: Replace with actual data,
                        CreatedDate = "01 May 2024", // TODO: Replace with actual data,
                        CreatedTime = "12:09", // TODO: Replace with actual data,
                        FinancialYear = "2024-25", // TODO: Replace with actual data,
                    },
                };

                return this.View(ClassifyingCalculationRunIndexView, classifyCalculationRunViewModel);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "An error occurred while processing the request.");
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
        }
    }
}