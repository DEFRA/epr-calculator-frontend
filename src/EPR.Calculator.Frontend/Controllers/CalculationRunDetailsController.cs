using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using System.Net;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CalculationRunDetailsController"/> class.
    /// </summary>
    /// <param name="configuration">The configuration settings.</param>
    /// <param name="clientFactory">The HTTP client factory.</param>
    /// <param name="logger">The logger instance.</param>
    public class CalculationRunDetailsController(
        IConfiguration configuration,
        IHttpClientFactory clientFactory,
        ILogger<CalculationRunDetailsController> logger,
        ITokenAcquisition tokenAcquisition,
        TelemetryClient telemetryClient)
        : BaseController(configuration, tokenAcquisition, telemetryClient, clientFactory)
    {
        private ILogger<CalculationRunDetailsController> Logger { get; init; } = logger;

        /// <summary>
        /// Displays the calculation run details index view.
        /// </summary>
        /// <param name="runId">The ID of the calculation run.</param>
        /// <returns>The calculation run details index view.</returns>
        [Route("ViewCalculationRunDetails/{runId}")]
        public async Task<IActionResult> IndexAsync(int runId)
        {
            try
            {
                using var getCalculationDetailsResponse = await this.GetCalculatorRunAsync(runId);

                if (!getCalculationDetailsResponse.IsSuccessStatusCode)
                {
                    this.Logger.LogError(
                        "Request failed with status code {StatusCode}", getCalculationDetailsResponse.StatusCode);

                    return this.RedirectToAction(
                        ActionNames.StandardErrorIndex,
                        CommonUtil.GetControllerName(typeof(StandardErrorController)));
                }

                var calculatorRun = JsonConvert.DeserializeObject<CalculatorRunDto>(getCalculationDetailsResponse.Content.ReadAsStringAsync().Result);

                if (calculatorRun == null)
                {
                    throw new ArgumentNullException($"Calculator with run id {runId} not found");
                }
                else if (!IsRunEligibleForDisplay(calculatorRun))
                {
                    return this.View(ViewNames.CalculationRunDetailsErrorPage, new ViewModelCommonData
                    {
                        CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                    });
                }

                var statusUpdateViewModel = new CalculatorRunStatusUpdateViewModel
                {
                    CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                    Data = new CalculatorRunStatusUpdateDto
                    {
                        RunId = runId,
                        ClassificationId = calculatorRun!.RunClassificationId,
                        CalcName = calculatorRun.RunName,
                        CreatedDate = calculatorRun.CreatedAt.ToString("dd MMM yyyy"),
                        CreatedTime = calculatorRun.CreatedAt.ToString("HH:mm"),
                    },
                };

                this.SetDownloadParameters(statusUpdateViewModel);

                return this.View(ViewNames.CalculationRunDetailsIndex, statusUpdateViewModel);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "An error occurred while processing the request.");
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
        }

        /// <summary>
        /// Deletes the calculation details.
        /// </summary>
        /// <param name="runId">The status update view model.</param>
        /// <param name="calcName">The calculation name.</param>
        /// <param name="createdTime">The created time.</param>
        /// <param name="createdDate">The created date.</param>
        /// <param name="deleteChecked">The delete is checked or not.</param>
        /// <returns>The delete confirmation view.</returns>
        [Route("DeleteCalculationRun")]
        public async Task<IActionResult> DeleteCalculation(int runId, string calcName, string createdTime, string createdDate, bool deleteChecked)
        {
            try
            {
                var dashboardCalculatorRunApi = this.Configuration.GetSection(ConfigSection.DashboardCalculatorRun).GetSection(ConfigSection.DashboardCalculatorRunApi).Value;

                if (dashboardCalculatorRunApi != null)
                {
                    var calculatorRunStatusUpdate = new CalculatorRunStatusUpdateDto
                    {
                        RunId = runId,
                        CalcName = calcName,
                        ClassificationId = (int)RunClassification.DELETED,
                        CreatedDate = createdDate,
                        CreatedTime = createdTime,
                    };
                    var statusUpdateViewModel = new CalculatorRunStatusUpdateViewModel
                    {
                        CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                        Data = calculatorRunStatusUpdate,
                    };

                    this.SetDownloadParameters(statusUpdateViewModel);

                    if (!deleteChecked)
                    {
                        statusUpdateViewModel.Errors = CreateErrorViewModel(ErrorMessages.SelectDeleteCalculation);
                        return this.View(ViewNames.CalculationRunDetailsIndex, statusUpdateViewModel);
                    }

                    using var response = await this.PutCalculatorRunsAsync(runId, RunClassification.DELETED);

                    if (response.StatusCode != HttpStatusCode.Created)
                    {
                        statusUpdateViewModel.Errors = CreateErrorViewModel(ErrorMessages.DeleteCalculationError);
                        return this.View(ViewNames.CalculationRunDetailsIndex, statusUpdateViewModel);
                    }

                    return this.View(ViewNames.DeleteConfirmation, statusUpdateViewModel);
                }

                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "An error occurred while processing the request.");
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
        }

        /// <summary>
        /// Creates an error view model.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>The error view model.</returns>
        private static ErrorViewModel CreateErrorViewModel(string errorMessage)
        {
            return new ErrorViewModel
            {
                DOMElementId = ViewControlNames.DeleteCalculationName,
                ErrorMessage = errorMessage,
            };
        }

        private static bool IsRunEligibleForDisplay(CalculatorRunDto calculatorRun)
        {
            if (calculatorRun.RunClassificationId == (int)RunClassification.UNCLASSIFIED)
            {
                return true;
            }

            return false;
        }

        private void SetDownloadParameters(CalculatorRunStatusUpdateViewModel statusUpdateViewModel)
        {
            var downloadResultApi = this.Configuration
                          .GetSection(ConfigSection.CalculationRunSettings)
                          .GetValue<string>(ConfigSection.DownloadResultApi);

            string? timeout = this.Configuration
                  .GetSection(ConfigSection.CalculationRunSettings)
                  .GetValue<string>(ConfigSection.DownloadResultTimeoutInMilliSeconds);
            int timeoutValue = int.TryParse(timeout, out timeoutValue) ? timeoutValue : 0;
            statusUpdateViewModel.DownloadTimeout = timeoutValue;

            statusUpdateViewModel.DownloadResultURL = new Uri($"{downloadResultApi}/{statusUpdateViewModel.Data.RunId}", UriKind.Absolute);
            statusUpdateViewModel.DownloadErrorURL = $"/DownloadFileError/{statusUpdateViewModel.Data.RunId}";
        }

        /// <summary>
        /// Calls the "calculatorRuns" PUT endpoint.
        /// </summary>
        /// <returns>The response message returned by the endpoint.</returns>
        private async Task<HttpResponseMessage> PutCalculatorRunsAsync(int runId, RunClassification classification)
        {
            var apiUrl = this.GetApiUrl(
                ConfigSection.DashboardCalculatorRun,
                ConfigSection.DashboardCalculatorRunApi);
            var args = new CalculatorRunStatusUpdateDto() { RunId = runId, ClassificationId = (int)classification };
            return await this.CallApi(HttpMethod.Put, apiUrl, string.Empty, args);
        }

        private async Task<HttpResponseMessage> GetCalculatorRunAsync(int runId)
        {
            var apiUrl = this.GetApiUrl(
                ConfigSection.DashboardCalculatorRun,
                ConfigSection.DashboardCalculatorRunApi);
            return await this.CallApi(
                HttpMethod.Get,
                apiUrl,
                runId.ToString(),
                null);
        }
    }
}