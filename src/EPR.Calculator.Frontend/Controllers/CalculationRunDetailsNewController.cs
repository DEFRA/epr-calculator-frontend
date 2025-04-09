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
using System.Reflection;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Controller responsible for displaying the details of a calculation run.
    /// </summary>
    [Authorize(Roles = "SASuperUser")]
    [Route("[controller]")]
    public class CalculationRunDetailsNewController : BaseController
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationRunDetailsNewController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration settings.</param>
        /// <param name="clientFactory">The HTTP client factory.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="tokenAcquisition">The token acquisition service.</param>
        /// <param name="telemetryClient">The telemetry client.</param>
        public CalculationRunDetailsNewController(
            IConfiguration configuration,
            IHttpClientFactory clientFactory,
            ILogger<CalculationRunDetailsNewController> logger,
            ITokenAcquisition tokenAcquisition,
            TelemetryClient telemetryClient)
            : base(configuration, tokenAcquisition, telemetryClient)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Gets the calculation run overview page.
        /// </summary>
        /// <param name="runId">Run ID.</param>
        /// <returns>View. </returns>
        [Route("{runId}")]
        public IActionResult Index(int runId)
        {
            CalculatorRunDto calculatorRun = GetCalculationRunDetails(runId);

            var viewModel = CreateViewModel(runId, calculatorRun);

            return View(ViewNames.CalculationRunDetailsNewIndex, viewModel);
        }

        [HttpPost]
        public IActionResult Submit(int runId, string SelectedCalcRunOption)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index", new { runId });
            }

            if (SelectedCalcRunOption == "outputClassify")
            {
                return RedirectToAction(ActionNames.Index, ControllerNames.ClassifyingCalculationRun, new { runId });
            }
            else if (SelectedCalcRunOption == "outputDelete")
            {
                return RedirectToAction(ActionNames.Index, ControllerNames.CalculationRunDelete, new { runId = runId });
            }
            else
            {
                return RedirectToAction(ActionNames.Index, new { runId });
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
                CreatedAt = new DateTime(2024, 5, 1, 12, 09, 0),
                CreatedBy = "Steve Jones",
            };
            var calculatorRun = calculatorRunDto;
            return calculatorRun;
        }

        private CalculatorRunDetailsNewViewModel CreateViewModel(int runId, CalculatorRunDto calculatorRun)
        {
            var viewModel = new CalculatorRunDetailsNewViewModel
            {
                CurrentUser = CommonUtil.GetUserName(HttpContext),
                Data = new CalculatorRunDto
                {
                    RunId = runId,
                    RunClassificationId = calculatorRun.RunClassificationId,
                    RunName = calculatorRun.RunName,
                    CreatedAt = calculatorRun.CreatedAt,
                    CreatedBy = calculatorRun.CreatedBy,
                    FinancialYear = calculatorRun.FinancialYear,
                    FileExtension = calculatorRun.FileExtension,
                    RunClassificationStatus = calculatorRun.RunClassificationStatus,
                },
            };

            SetDownloadParameters(viewModel);

            return viewModel;
        }

        private void SetDownloadParameters(CalculatorRunDetailsNewViewModel viewModel)
        {
            var baseApiUrl = _configuration.GetValue<string>($"{ConfigSection.CalculationRunSettings}:{ConfigSection.DownloadResultApi}");
            viewModel.DownloadResultURL = new Uri($"{baseApiUrl}/{viewModel.Data.RunId}");

            viewModel.DownloadErrorURL = $"/DownloadFileError/{viewModel.Data.RunId}";
            viewModel.DownloadTimeout = _configuration.GetValue<int>($"{ConfigSection.CalculationRunSettings}:{ConfigSection.DownloadResultTimeoutInMilliSeconds}");
        }
    }
}