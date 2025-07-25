﻿using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Controller for handling post billing file details.
    /// </summary>
    [Route("[controller]")]
    public class PostBillingFileController(
        IConfiguration configuration,
        ITokenAcquisition tokenAcquisition,
        TelemetryClient telemetryClient,
        IApiService apiService,
        ICalculatorRunDetailsService calculatorRunDetailsService)
        : BaseController(
            configuration,
            tokenAcquisition,
            telemetryClient,
            apiService,
            calculatorRunDetailsService)
    {
        /// <summary>
        /// Displays the calculation run post billing file details index view.
        /// </summary>
        /// <param name="runId">The ID of the calculation run.</param>
        /// <returns>The post billing file index view.</returns>
        [HttpGet]
        [Route("{runId}")]
        public async Task<IActionResult> Index(int runId)
        {
            var viewModel = await this.CreateViewModel(runId);

            if (viewModel.CalculatorRunStatus == null
                || viewModel.CalculatorRunStatus.RunId <= 0
                || !IsRunEligibleForDisplay(viewModel.CalculatorRunStatus))
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }

            return this.View(ViewNames.PostBillingFileIndex, viewModel);
        }

        private static bool IsRunEligibleForDisplay(CalculatorRunPostBillingFileDto calculatorRunDetails)
        {
            if (calculatorRunDetails.RunClassificationId == RunClassification.UNCLASSIFIED
                || calculatorRunDetails.RunClassificationId == RunClassification.INITIAL_RUN
                || calculatorRunDetails.RunClassificationId == RunClassification.INITIAL_RUN_COMPLETED)
            {
                return true;
            }

            return false;
        }

        private async Task<PostBillingFileViewModel> CreateViewModel(int runId)
        {
            var runDetails = await this.GetCalculatorRunWithBillingdetails(runId);

            var viewModel = new PostBillingFileViewModel()
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
            };

            if (runDetails != null && runDetails!.RunId > 0)
            {
                viewModel.CalculatorRunStatus = runDetails;
            }

            return viewModel;
        }
    }
}
