﻿using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClassifyingCalculationRunScenario4Controller"/> class.
    /// </summary>
    [Route("[controller]")]
    public class ClassifyingCalculationRunScenario4Controller : BaseController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassifyingCalculationRunScenario4Controller"/> class.
        /// </summary>
        /// <param name="configuration">The configuration settings.</param>
        /// <param name="tokenAcquisition">token acquisition.</param>
        /// <param name="telemetryClient">telemetry client.</param>
        public ClassifyingCalculationRunScenario4Controller(IConfiguration configuration, ITokenAcquisition tokenAcquisition, TelemetryClient telemetryClient)
            : base(configuration, tokenAcquisition, telemetryClient)
        {
        }

        /// <summary>
        /// Displays the index view for classifying calculation runs.
        /// </summary>
        /// <param name="runId"> runId.</param>
        /// <returns>The index view.</returns>
        [Route("PostFinalClassificationFour/{runId}")]
        [HttpGet]
        public IActionResult Index(int runId)
        {
            var classifyCalculationRunViewModel = new ClassifyCalculationRunScenerio4ViewModel
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

            return this.View(ViewNames.ClassifyingCalculationRunScenario4Index, classifyCalculationRunViewModel);
        }
    }
}