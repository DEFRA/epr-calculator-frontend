using EPR.Calculator.Frontend.Common.Constants;
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
        public IActionResult Index(int runId)
        {
            try
            {
                CalculatorRunDto calculatorRun = GetCalculationRunDetails(runId);

                var viewModel = this.CreateViewModel(runId, calculatorRun);

                return this.View(ViewNames.SetRunClassificationIndex, viewModel);
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
        public async Task<IActionResult> Submit(SetRunClassificationViewModel model)
        {
            try
            {
                if (!this.ModelState.IsValid)
                {
                    var calculatorRun = GetCalculationRunDetails(model.RunId);
                    var viewModel = this.CreateViewModel(model.RunId, calculatorRun);

                    return this.View(ViewNames.SetRunClassificationIndex, viewModel);
                }

                var apiUrl = this.GetApiUrl(
                ConfigSection.DashboardCalculatorRun,
                ConfigSection.DashboardCalculatorRunV2);
                await this.CallApi(
                    HttpMethod.Put,
                    apiUrl,
                    string.Empty,
                    new ClassificationDto
                {
                    RunId = model.RunId,
                    ClassificationId = (int)model.ClassifyRunType,
                });

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

        private SetRunClassificationViewModel CreateViewModel(int runId, CalculatorRunDto calculatorRun)
        {
            var viewModel = new SetRunClassificationViewModel
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
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