﻿using EPR.Calculator.Frontend.Constants;
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
    [Route("[controller]")]
    public class ClassifyingCalculationRunScenario1Controller : BaseController
    {
        private const string ClassifyingCalculationRunIndexView = ViewNames.ClassifyingCalculationRunScenario1Index;
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory clientFactory;
        private readonly ILogger<ClassifyingCalculationRunScenario1Controller> logger;

        public ClassifyingCalculationRunScenario1Controller(IConfiguration configuration, IHttpClientFactory clientFactory, ILogger<ClassifyingCalculationRunScenario1Controller> logger, ITokenAcquisition tokenAcquisition, TelemetryClient telemetryClient)
            : base(configuration, tokenAcquisition, telemetryClient)
        {
            this.configuration = configuration;
            this.clientFactory = clientFactory;
            this.logger = logger;
        }

        /// </summary>
        /// <param name="runId"> runId.</param>
        /// <returns>The index view.</returns>
        [Route("{runId}")]
        [HttpGet]
        public IActionResult Index(int runId)
        {
            try
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
            catch (Exception ex)
            {
                this.logger.LogError(ex, "An error occurred while processing the request.");
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
        }

        [Route("Submit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Submit(ClassifyCalculationRunScenerio1SubmitViewModel model)
        {
            try
            {
                if (!this.ModelState.IsValid)
                {
                    var classifyCalculationRunViewModel = new ClassifyCalculationRunScenerio1ViewModel
                    {
                        CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                        CalculatorRunStatus = new CalculatorRunStatusUpdateDto
                        {
                            RunId = model.RunId,
                            ClassificationId = 240008,
                            CalcName = "Calculation Run 99",
                            CreatedDate = "01 May 2024",
                            CreatedTime = "12:09",
                            FinancialYear = "2024-25",
                        },
                        BackLink = ControllerNames.CalculationRunDetails,
                    };

                    classifyCalculationRunViewModel.Errors = ModelStateHelper.GetErrors(this.ModelState);

                    return this.View(ClassifyingCalculationRunIndexView, classifyCalculationRunViewModel);
                }

                return this.RedirectToAction(ActionNames.Index, ControllerNames.ClassifyRunConfirmation, new { runId = model.RunId });
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "An error occurred while processing the request.");
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
        }
    }
}