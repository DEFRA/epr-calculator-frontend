using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Net;
using System.Web;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CalculationRunDetailsController"/> class.
    /// </summary>
    [Authorize(Roles = "SASuperUser")]
    public class CalculationRunDetailsController : BaseController
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory clientFactory;
        private readonly ILogger<CalculationRunDetailsController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationRunDetailsController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration settings.</param>
        /// <param name="clientFactory">The HTTP client factory.</param>
        /// <param name="logger">The logger instance.</param>
        public CalculationRunDetailsController(IConfiguration configuration, IHttpClientFactory clientFactory, ILogger<CalculationRunDetailsController> logger, ITokenAcquisition tokenAcquisition, TelemetryClient telemetryClient)
            : base(configuration, tokenAcquisition, telemetryClient)
        {
            this.configuration = configuration;
            this.clientFactory = clientFactory;
            this.logger = logger;
        }

        /// <summary>
        /// Displays the calculation run details index view.
        /// </summary>
        /// <param name="runId">The ID of the calculation run.</param>
        /// <param name="calcName">The calcName of the calculation run.</param>
        /// <returns>The calculation run details index view.</returns>
        [Authorize(Roles = "SASuperUser")]
        public async Task<IActionResult> IndexAsync(int runId, string calcName, string createdAt)
        {
            try
            {
                var getCalculationDetailsResponse = await this.GetCalculationDetailsAsync(runId);

                if (!getCalculationDetailsResponse.IsSuccessStatusCode)
                {
                    this.logger.LogError(
                        "Request failed with status code {StatusCode}", getCalculationDetailsResponse?.StatusCode);

                    return this.RedirectToAction(
                        ActionNames.StandardErrorIndex,
                        CommonUtil.GetControllerName(typeof(StandardErrorController)));
                }

                var statusUpdateViewModel = new CalculatorRunStatusUpdateViewModel
                {
                    CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                    Data = new CalculatorRunStatusUpdateDto
                    {
                        RunId = runId,
                        ClassificationId = (int)RunClassification.DELETED,
                        CalcName = calcName,
                        CreatedDate = SplitDateTime(createdAt).Item1,
                        CreatedTime = SplitDateTime(createdAt).Item2,
                    },
                };

                this.SetDownloadParameters(statusUpdateViewModel);

                return this.View(ViewNames.CalculationRunDetailsIndex, statusUpdateViewModel);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "An error occurred while processing the request.");
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
        [Authorize(Roles = "SASuperUser")]
        public IActionResult DeleteCalcDetails(int runId, string calcName, string createdTime, string createdDate, bool deleteChecked)
        {
            try
            {
                var dashboardCalculatorRunApi = this.configuration.GetSection(ConfigSection.DashboardCalculatorRun).GetSection(ConfigSection.DashboardCalculatorRunApi).Value;

                var client = this.clientFactory.CreateClient();
                if (dashboardCalculatorRunApi != null)
                {
                    client.BaseAddress = new Uri(dashboardCalculatorRunApi);
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
                        this.ViewBag.Errors = CreateErrorViewModel(ErrorMessages.SelectDeleteCalculation);
                        return this.View(ViewNames.CalculationRunDetailsIndex, statusUpdateViewModel);
                    }

                    var request = new HttpRequestMessage(
                        HttpMethod.Put,
                        new Uri(
                            $"{dashboardCalculatorRunApi}?runId={statusUpdateViewModel.Data.RunId}&classificationId={statusUpdateViewModel.Data.ClassificationId}"));
                    var response = client.SendAsync(request);

                    response.Wait();

                    if (response.Result.StatusCode != HttpStatusCode.Created)
                    {
                        this.ViewBag.Errors = CreateErrorViewModel(ErrorMessages.DeleteCalculationError);
                        return this.View(ViewNames.CalculationRunDetailsIndex, statusUpdateViewModel);
                    }

                    return this.View(ViewNames.DeleteConfirmation, statusUpdateViewModel);
                }

                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "An error occurred while processing the request.");
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
        }

        /// <summary>
        /// Error details page.
        /// </summary>
        /// <returns>Error details page</returns>
        [Authorize(Roles = "SASuperUser")]
        public IActionResult Error()
        {
            return this.View(ViewNames.CalculationRunDetailsErrorPage, new ViewModelCommonData
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
            });
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

        private static (string Date, string Time) SplitDateTime(string input)
        {
            string[] parts = input.Split(new string[] { " at " }, StringSplitOptions.None);
            return (parts[0], parts[1]);
        }

        /// <summary>
        /// Asynchronously retrieves calculation details for a given run ID.
        /// </summary>
        /// <param name="runId">The ID of the calculation run to retrieve details for.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the API URL is null or empty.</exception>
        private async Task<HttpResponseMessage> GetCalculationDetailsAsync(int runId)
        {
            var client = this.CreateHttpClient();
            var apiUrl = client.BaseAddress.ToString();
            var accessToken = await this.AcquireToken();

            client.DefaultRequestHeaders.Add("Authorization", accessToken);
            var requestUri = new Uri($"{apiUrl}/{runId}", UriKind.Absolute);
            return await client.GetAsync(requestUri);
        }

        /// <summary>
        /// Creates and configures an <see cref="HttpClient"/> instance.
        /// </summary>
        /// <returns>A configured <see cref="HttpClient"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the API URL is null or empty.</exception>
        private HttpClient CreateHttpClient()
        {
            var apiUrl = this.configuration.GetSection(ConfigSection.DashboardCalculatorRun).GetValue<string>(ConfigSection.DashboardCalculatorRunApi);

            var client = this.clientFactory.CreateClient();
            client.BaseAddress = new Uri(apiUrl);
            return client;
        }

        private void SetDownloadParameters(CalculatorRunStatusUpdateViewModel statusUpdateViewModel)
        {
            var downloadResultApi = this.configuration
                          .GetSection(ConfigSection.CalculationRunSettings)
                          .GetValue<string>(ConfigSection.DownloadResultApi);

            string? timeout = this.configuration
                  .GetSection(ConfigSection.CalculationRunSettings)
                  .GetValue<string>(ConfigSection.DownloadResultTimeoutInMilliSeconds);
            int timeoutValue = int.TryParse(timeout, out timeoutValue) ? timeoutValue : 0;
            statusUpdateViewModel.DownloadTimeout = timeoutValue;

            statusUpdateViewModel.DownloadResultURL = new Uri($"{downloadResultApi}/{statusUpdateViewModel.Data.RunId}", UriKind.Absolute);
            statusUpdateViewModel.DownloadErrorURL = this.GetDownloadErrorPageURL(statusUpdateViewModel);
        }

        private string GetDownloadErrorPageURL(CalculatorRunStatusUpdateViewModel statusUpdateViewModel)
        {
            var request = this.HttpContext.Request;
            var currentUri = new Uri($"{request.Scheme}://{request.Host}");

            var builder = new UriBuilder(currentUri);
            builder.Path = "/DownloadFileError/Index";
            var query = HttpUtility.ParseQueryString(builder.Query);
            query["runId"] = statusUpdateViewModel.Data.RunId.ToString();
            query["calcName"] = statusUpdateViewModel.Data.CalcName;
            query["createdDate"] = statusUpdateViewModel.Data.CreatedDate;
            query["createdTime"] = statusUpdateViewModel.Data.CreatedTime;
            builder.Query = query.ToString();
            return builder.ToString();
        }
    }
}