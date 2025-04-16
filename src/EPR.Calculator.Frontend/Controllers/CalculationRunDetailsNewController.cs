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
using static EPR.Calculator.Frontend.Constants.CommonEnums;

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

            if (calculatorRun == null)
            {
                throw new ArgumentNullException($"Calculator with run id {runId} not found");
            }
            else if (!IsRunEligibleForDisplay(calculatorRun))
            {
                return this.View(ViewNames.CalculationRunDetailsNewErrorPage, new ViewModelCommonData
                {
                    CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                });
            }

            var viewModel = CreateViewModel(runId, calculatorRun);

            return View(ViewNames.CalculationRunDetailsNewIndex, viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Submit(CalculatorRunDetailsNewViewModel model)
        {
            if (model.SelectedCalcRunOption == null || model.SelectedCalcRunOption == CommonEnums.CalculationRunOption.None)
            {
                return RedirectToAction("Index", new { model.Data.RunId });
            }

            switch (model.SelectedCalcRunOption)
            {
                case CommonEnums.CalculationRunOption.OutputClassify:
                    return RedirectToAction(ActionNames.Index, ControllerNames.ClassifyingCalculationRun, new { model.Data.RunId });

                case CommonEnums.CalculationRunOption.OutputDelete:
                    return RedirectToAction(ActionNames.Index, ControllerNames.CalculationRunDelete, new { model.Data.RunId });

                default:
                    return RedirectToAction(ActionNames.Index, new { model.Data.RunId });
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

        private static bool IsRunEligibleForDisplay(CalculatorRunDto calculatorRun)
        {
            if (calculatorRun.RunClassificationId == (int)RunClassification.UNCLASSIFIED)
            {
                return true;
            }

            return false;
        }
    }
}