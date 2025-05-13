using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using System.Net;
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
    public class SetRunClassificationController(
        IConfiguration configuration,
        IHttpClientFactory clientFactory,
        ILogger<SetRunClassificationController> logger,
        ITokenAcquisition tokenAcquisition,
        TelemetryClient telemetryClient)
        : BaseController(configuration, tokenAcquisition, telemetryClient, clientFactory)
    {
        private readonly ILogger<SetRunClassificationController> logger = logger;

        [Route("{runId}")]
        [HttpGet]
        public async Task<IActionResult> Index(int runId)
        {
            var viewModel = await this.CreateViewModel(runId);

            if (viewModel.CalculatorRunDetails == null || viewModel.CalculatorRunDetails.RunId == 0)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
            else if (!IsRunEligibleForDisplay(viewModel.CalculatorRunDetails))
            {
                this.ModelState.AddModelError(viewModel.CalculatorRunDetails.RunName!, ErrorMessages.RunDetailError);
                return this.View(ViewNames.CalculationRunDetailsNewErrorPage, viewModel);
            }

            return this.View(ViewNames.SetRunClassificationIndex, viewModel);
        }

        [Route("Submit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(SetRunClassificationViewModel model)
        {
            try
            {
                if (!this.ModelState.IsValid)
                {
                    var viewModel = await this.CreateViewModel(model.CalculatorRunDetails.RunId);

                    return this.View(ViewNames.SetRunClassificationIndex, viewModel);
                }

                var apiUrl = this.GetApiUrl(
                ConfigSection.DashboardCalculatorRun,
                ConfigSection.DashboardCalculatorRunV2);
                var result = await this.CallApi(
                    HttpMethod.Put,
                    apiUrl,
                    string.Empty,
                    new ClassificationDto
                {
                    RunId = model.CalculatorRunDetails.RunId,
                    ClassificationId = (int)model.ClassifyRunType,
                });

                if (result.StatusCode == HttpStatusCode.Created)
                {
                    return this.RedirectToAction(ActionNames.Index, ControllerNames.ClassifyRunConfirmation, new { runId = model.CalculatorRunDetails.RunId });
                }
                else
                {
                    this.logger.LogError("API did not return successful.");
                    return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "An error occurred while processing the request.");
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
        }

        private static bool IsRunEligibleForDisplay(CalculatorRunDetailsViewModel calculatorRunDetails)
        {
            return calculatorRunDetails.RunClassificationId == RunClassification.UNCLASSIFIED;
        }

        private async Task<SetRunClassificationViewModel> CreateViewModel(int runId)
        {
            var viewModel = new SetRunClassificationViewModel()
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                CalculatorRunDetails = new CalculatorRunDetailsViewModel(),
                BackLink = ControllerNames.CalculationRunDetails,
            };

            var runDetails = await this.GetCalculatorRundetails(runId);
            if (runDetails != null && runDetails!.RunId != 0)
            {
                viewModel.CalculatorRunDetails = runDetails;
            }

            return viewModel;
        }
    }
}