using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using EPR.Calculator.Frontend.ViewModels.Enums;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Controller responsible for displaying the details of a calculation run.
    /// </summary>
    [Route("[controller]")]
    public class CalculationRunDetailsNewController : BaseController
    {
        private readonly IConfiguration _configuration;

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

        [Route("{runId}")]
        public IActionResult Index(int runId)
        {
            CalculatorRunDto calculatorRun = GetCalculationRunDetails(runId);

            var viewModel = this.CreateViewModel(runId, calculatorRun);

            if (calculatorRun == null)
            {
                throw new ArgumentNullException($"Calculator with run id {runId} not found");
            }

            if (!IsRunEligibleForDisplay(calculatorRun))
            {
                this.ModelState.AddModelError(viewModel.RunName, ErrorMessages.RunDetailError);
                return this.View(ViewNames.CalculationRunDetailsNewErrorPage, viewModel);
            }

            return this.View(ViewNames.CalculationRunDetailsNewIndex, viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Submit(CalculatorRunDetailsNewViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                var calculatorRun = GetCalculationRunDetails(model.RunId);
                var viewModel = CreateViewModel(model.RunId, calculatorRun);

                return View(ViewNames.CalculationRunDetailsNewIndex, viewModel);
            }

            switch (model.SelectedCalcRunOption)
            {
                case CalculationRunOption.OutputClassify:
                    return RedirectToAction(ActionNames.Index, ControllerNames.ClassifyingCalculationRun, new { model.RunId });

                case CalculationRunOption.OutputDelete:
                    return RedirectToAction(ActionNames.Index, ControllerNames.CalculationRunDelete, new { model.RunId });

                default:
                    return RedirectToAction(ActionNames.Index, new { model.RunId });
            }
        }

        private static CalculatorRunDto GetCalculationRunDetails(int runId)
        {
            // Get the calculation run details from the API
            var calculatorRuns = new List<CalculatorRunDto>();

            CalculatorRunDto calculatorRunDto = new()
            {
                RunId = runId,
                FinancialYear = "2024-25",
                FileExtension = "xlsx",
                RunClassificationStatus = "Unclassified",
                RunName = "Calculation Run 99",
                RunClassificationId = 3,
                CreatedAt = new DateTime(2024, 5, 1, 12, 09, 0, DateTimeKind.Utc),
                CreatedBy = "Steve Jones",
            };

            CalculatorRunDto calculatorRunErrorDto = new()
            {
                RunId = 23,
                FinancialYear = "2024-25",
                FileExtension = "xlsx",
                RunClassificationStatus = "Error",
                RunName = "Calculation Run 99",
                RunClassificationId = 5,
                CreatedAt = new DateTime(2024, 5, 1, 12, 09, 0, DateTimeKind.Utc),
                CreatedBy = "Steve Jones",
            };

            calculatorRuns.Add(calculatorRunDto);
            calculatorRuns.Add(calculatorRunErrorDto);
            var calculatorRun = calculatorRuns.Where(x => x.RunId == runId).FirstOrDefault();

            return calculatorRun;
        }

        private static bool IsRunEligibleForDisplay(CalculatorRunDto calculatorRun)
        {
            if (calculatorRun.RunClassificationId == (int)RunClassification.UNCLASSIFIED)
            {
                return true;
            }

            return false;
        }

        private CalculatorRunDetailsNewViewModel CreateViewModel(int runId, CalculatorRunDto calculatorRun)
        {
            var viewModel = new CalculatorRunDetailsNewViewModel
            {
                CurrentUser = CommonUtil.GetUserName(HttpContext),
                RunId = runId,
                RunName = calculatorRun.RunName,
                CreatedAt = calculatorRun.CreatedAt,
                CreatedBy = calculatorRun.CreatedBy,
                FinancialYear = calculatorRun.FinancialYear,
            };

            SetDownloadParameters(viewModel);

            return viewModel;
        }

        private void SetDownloadParameters(CalculatorRunDetailsNewViewModel viewModel)
        {
            var baseApiUrl = _configuration.GetValue<string>($"{ConfigSection.CalculationRunSettings}:{ConfigSection.DownloadResultApi}");
            viewModel.DownloadResultURL = new Uri($"{baseApiUrl}/{viewModel.RunId}");

            viewModel.DownloadErrorURL = $"/DownloadFileError/{viewModel.RunId}";
            viewModel.DownloadTimeout = _configuration.GetValue<int>($"{ConfigSection.CalculationRunSettings}:{ConfigSection.DownloadResultTimeoutInMilliSeconds}");
        }
    }
}