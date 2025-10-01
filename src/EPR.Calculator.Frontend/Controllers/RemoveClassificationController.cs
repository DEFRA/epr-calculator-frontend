using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Extensions;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using System.Globalization;
using System.Net;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Controller for handling classifying removecalculation run scenario 1.
    /// </summary>
    /// <param name="configuration">The configuration settings.</param>
    /// <param name="clientFactory">The HTTP client factory.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="tokenAcquisition">token acquisition.</param>
    /// <param name="telemetryClient">telemetry client.</param>
    [Route("[controller]")]
    public class RemoveClassificationController(
        IConfiguration configuration,
        IApiService apiService,
        ILogger<RemoveClassificationController> logger,
        ITokenAcquisition tokenAcquisition,
        TelemetryClient telemetryClient,
        ICalculatorRunDetailsService calculatorRunDetailsService)
        : BaseController(
            configuration,
            tokenAcquisition,
            telemetryClient,
            apiService,
            calculatorRunDetailsService)
    {
        private readonly ILogger<RemoveClassificationController> logger = logger;

        /// <summary>
        /// Displays the remove classification view for a specific calculation run.
        /// </summary>
        /// <param name="runId">The ID of the calculation run to be displayed.</param>
        /// <returns>The remove classification view populated with run details.</returns>
        /// <remarks>
        /// This method initializes the <see cref="SetRunClassificationViewModel"/> with run details and current user info.
        /// It is typically accessed via a GET request when a user wants to remove the classification of a specific run.
        /// </remarks>
        [Route("{runId}")]
        [HttpGet]
        public async Task<IActionResult> Index(int runId)
        {
            try
            {
                var financialYear = CommonUtil.GetFinancialYear(this.HttpContext.Session);
                var currentUser = CommonUtil.GetUserName(this.HttpContext);

                var viewModel = new SetRunClassificationViewModel
                {
                    CalculatorRunDetails = new CalculatorRunDetailsViewModel(),
                    CurrentUser = currentUser,
                    BackLinkViewModel = new BackLinkViewModel
                    {
                        BackLink = this.GetBackLink(),
                        RunId = runId,
                        CurrentUser = currentUser,
                    },
                    ClassifyRunType = null,
                };

                var runDetails = await this.CalculatorRunDetailsService.GetCalculatorRundetailsAsync(this.HttpContext, runId);
                if (runDetails != null && runDetails.RunId > 0)
                {
                    viewModel.CalculatorRunDetails = runDetails;
                }

                return this.View(ViewNames.RemoveClassification, viewModel);
           }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "An error occurred while processing the request.");
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
        }

        /// <summary>
        /// Handles form submission from the remove classification view.
        /// </summary>
        /// <param name="model">The submitted <see cref="SetRunClassificationViewModel"/> containing classification selection.</param>
        /// <returns>
        /// Redirects to:
        /// <list type="bullet">
        ///   <item><description>Confirmation page if "Test Run" is selected and classification is updated.</description></item>
        ///   <item><description>Delete controller if "Delete" is selected.</description></item>
        ///   <item><description>Error page if submission fails or unexpected input is encountered.</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// - If the model state is invalid, re-renders the view without saving.
        /// - If "Test Run" is selected, calls the API to update classification.
        /// - If "Delete" is selected, redirects to the delete confirmation view.
        /// </remarks>
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
                    return this.View(ViewNames.RemoveClassification, viewModel);
                }

                // If Test Run is selected, mark it as classified
                if (model.ClassifyRunType == (int)RunClassification.TEST_RUN)
                {
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
                            RunId = model.CalculatorRunDetails.RunId,
                            ClassificationId = (int)RunClassification.TEST_RUN,
                        });

                    if (result.StatusCode == HttpStatusCode.Created)
                    {
                        return this.RedirectToAction(ActionNames.Index, ControllerNames.ClassifyRunConfirmation, new { runId = model.CalculatorRunDetails.RunId });
                    }
                    else
                    {
                        this.logger.LogError($"API did not return successful ({result.StatusCode}).");
                        return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
                    }
                }

                // If Delete Run is selected, redirect to delete controller
                else if (model.ClassifyRunType == (int)RunClassification.DELETED)
                {
                    return this.RedirectToAction(ActionNames.Index, ControllerNames.CalculationRunDelete, new { runId = model.CalculatorRunDetails.RunId });
                }
                else
                {
                    return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "An error occurred while processing the request.");
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
        }

        private async Task<SetRunClassificationViewModel> CreateViewModel(int runId)
        {
            var viewModel = new SetRunClassificationViewModel()
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                CalculatorRunDetails = new CalculatorRunDetailsViewModel(),
                BackLinkViewModel = new BackLinkViewModel
                {
                    BackLink = ControllerNames.CalculationRunDetails,
                    RunId = runId,
                    CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                },
            };

            var runDetails = await this.CalculatorRunDetailsService.GetCalculatorRundetailsAsync(
                this.HttpContext,
                runId);
            if (runDetails != null && runDetails!.RunId != 0)
            {
                viewModel.CalculatorRunDetails = runDetails;
            }

            return viewModel;
        }
    }
}