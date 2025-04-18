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
        private readonly ILogger<ClassifyingCalculationRunScenario1Controller> logger;

        public ClassifyingCalculationRunScenario1Controller(IConfiguration configuration, IHttpClientFactory clientFactory, ILogger<ClassifyingCalculationRunScenario1Controller> logger, ITokenAcquisition tokenAcquisition, TelemetryClient telemetryClient)
            : base(configuration, tokenAcquisition, telemetryClient)
        {
            this.logger = logger;
        }

        [Route("{runId}")]
        [HttpGet]
        public IActionResult Index(int runId)
        {
            try
            {
                CalculatorRunDto calculatorRun = GetCalculationRunDetails(runId);

                var viewModel = CreateViewModel(runId, calculatorRun);

                return this.View(ViewNames.ClassifyingCalculationRunScenario1Index, viewModel);
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
        public IActionResult Submit(ClassifyCalculationRunScenerio1ViewModel model)
        {
            try
            {
                if (!this.ModelState.IsValid)
                {
                    var calculatorRun = GetCalculationRunDetails(model.RunId);
                    var viewModel = CreateViewModel(model.RunId, calculatorRun);

                    return View(ViewNames.ClassifyingCalculationRunScenario1Index, viewModel);
                }

                return this.RedirectToAction(ActionNames.Index, ControllerNames.ClassifyRunConfirmation, new { runId = model.RunId });
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "An error occurred while processing the request.");
                return this.RedirectToAction(ActionNames.StandardErrorIndex, ControllerNames.StandardErrorController);
            }
        }

        private static CalculatorRunDto GetCalculationRunDetails(int runId)
        {
            // Get the calculation run details from the API
            CalculatorRunDto calculatorRunDto = new()
            {
                RunId = runId,
                FinancialYear = "2024-25",
                FileExtension = "xlsx",
                RunClassificationStatus = "Draft",
                RunName = "Calculation Run 99",
                RunClassificationId = 240008,
                CreatedAt = new DateTime(2024, 5, 1, 12, 09, 0, DateTimeKind.Utc),
                CreatedBy = "Steve Jones",
            };
            var calculatorRun = calculatorRunDto;
            return calculatorRun;
        }

        private ClassifyCalculationRunScenerio1ViewModel CreateViewModel(int runId, CalculatorRunDto calculatorRun)
        {
            var viewModel = new ClassifyCalculationRunScenerio1ViewModel
            {
                CurrentUser = CommonUtil.GetUserName(HttpContext),
                RunId = runId,
                RunName = calculatorRun.RunName,
                CreatedAt = calculatorRun.CreatedAt,
                CreatedBy = calculatorRun.CreatedBy,
                FinancialYear = calculatorRun.FinancialYear,
                BackLink = ControllerNames.CalculationRunDetails,
            };

            return viewModel;
        }
    }
}