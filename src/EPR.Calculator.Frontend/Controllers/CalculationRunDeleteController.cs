using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.Identity.Web;
using System.Net;
using System.Reflection;

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
    [Route("[controller]")]
    public class CalculationRunDeleteController(
        IConfiguration configuration,
        IApiService apiService,
        ITokenAcquisition tokenAcquisition,
        TelemetryClient telemetryClient,
        ICalculatorRunDetailsService calculatorRunDetailsService)
        : BaseController(configuration,
            tokenAcquisition,
            telemetryClient,
            apiService,
            calculatorRunDetailsService)
    {
        /// <summary>
        /// Displays the calculate run delete confirmation screen.
        /// </summary>
        /// <param name="runId">The ID of the calculation run.</param>
        /// <returns>The delete confirmation view.</returns>
        [Route("{runId}")]
        public async Task<IActionResult> Index(int runId)
        {
            var runDetails = await this.CalculatorRunDetailsService.GetCalculatorRundetailsAsync(
                this.HttpContext,
                runId);
            var calculatorRunStatusUpdate = new CalculatorRunStatusUpdateDto
            {
                RunId = runId,
                CalcName = runDetails?.RunName,
                ClassificationId = (int)RunClassification.DELETED,
            };
            var currentUser = CommonUtil.GetUserName(this.HttpContext);
            var calculationRunDeleteViewModel = new CalculationRunDeleteViewModel
            {
                CurrentUser = currentUser,
                CalculatorRunStatusData = calculatorRunStatusUpdate,
                BackLinkViewModel = new BackLinkViewModel
                {
                    BackLink = ControllerNames.CalculationRunDetails,
                    RunId = runId,
                    CurrentUser = currentUser,
                },
            };
            return this.View(ViewNames.CalculationRunDeleteIndex, calculationRunDeleteViewModel);
        }

        /// <summary>
        /// Displays the calculate run delete confirmation screen.
        /// </summary>
        /// <returns>The delete confirmation success view.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmationSuccess(CalculatorRunDetailsViewModel model)
        {
            var currentUser = CommonUtil.GetUserName(this.HttpContext);
            var viewModel = new CalculatorRunDetailsNewViewModel()
            {
                CurrentUser = currentUser,
                CalculatorRunDetails = model,
                BackLinkViewModel = new BackLinkViewModel()
                {
                    BackLink = ControllerNames.ClassifyingCalculationRun,
                    CurrentUser = currentUser,
                },
            };

            var apiUrl = this.ApiService.GetApiUrl(
                ConfigSection.DashboardCalculatorRun,
                ConfigSection.DashboardCalculatorRunV2);

            var result = await this.ApiService.CallApi(
                this.HttpContext,
                HttpMethod.Put,
                apiUrl,
                string.Empty,
                new ClassificationDto
                {
                    RunId = model.RunId,
                    ClassificationId = (int)RunClassification.DELETED,
                });

            if (result.StatusCode == HttpStatusCode.Created)
            {
                return this.View(ViewNames.CalculationRunDeleteConfirmationSuccess, viewModel);
            }
            else
            {
                this.TelemetryClient.TrackTrace($"API did not return successful ({result.StatusCode}).");
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
        }
    }
}