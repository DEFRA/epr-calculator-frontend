using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using System.Reflection;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Controller for handling classifying calculation run scenario 1.
    /// </summary>
    /// <param name="configuration">The configuration settings.</param>
    /// <param name="clientFactory">The HTTP client factory.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="tokenAcquisition">token acquisition.</param>
    /// <param name="telemetryClient">telemetry client.</param>
    [Route("[controller]")]
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

        [Route("{runId}")]
        public IActionResult Index(int runId)
        {
            var classifyCalculationRunViewModel = new ClassifyCalculationRunScenerio1ViewModel
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                CalculatorRunStatus = new CalculatorRunStatusUpdateDto
                {
                    RunId = runId,
                    ClassificationId = 240008,
                    CalcName = "Calculation Run 99",
                    CreatedDate = "01 May 2024",
                    CreatedTime = "12:09",
                    FinancialYear = "2024-25",
                },
                BackLink = ControllerNames.CalculationRunDetails,
            };

            return this.View(ClassifyingCalculationRunIndexView, classifyCalculationRunViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Submit(int runId)
        {
            if (!this.ModelState.IsValid)
            {
                return RedirectToAction(ActionNames.Index, new { runId });
            }

            return RedirectToAction(ActionNames.Index, ControllerNames.ClassifyRunConfirmation, new { runId = runId });
        }
    }
}