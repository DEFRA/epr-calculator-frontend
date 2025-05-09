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
using System.Globalization;

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
    public class ClassifyingCalculationRunScenario1Controller(
        IConfiguration configuration,
        IHttpClientFactory clientFactory,
        ILogger<ClassifyingCalculationRunScenario1Controller> logger,
        ITokenAcquisition tokenAcquisition,
        TelemetryClient telemetryClient)
        : BaseController(configuration, tokenAcquisition, telemetryClient, clientFactory)
    {
        private readonly ILogger<ClassifyingCalculationRunScenario1Controller> logger = logger;

        [Route("{runId}")]
        [HttpGet]
        public async Task<IActionResult> Index(int runId)
        {
            var viewModel = await this.CreateViewModel(runId);
            if (!await SetClassfications(runId, viewModel))
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, ControllerNames.StandardErrorController);
            }

            if (viewModel.CalculatorRunDetails == null || viewModel.CalculatorRunDetails.RunId == 0)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
            else if (!IsRunEligibleForDisplay(viewModel.CalculatorRunDetails))
            {
                this.ModelState.AddModelError(viewModel.CalculatorRunDetails.RunName!, ErrorMessages.RunDetailError);
                return this.View(ViewNames.CalculationRunDetailsNewErrorPage, viewModel);
            }

            return this.View(ViewNames.ClassifyingCalculationRunScenario1Index, viewModel);
        }

        private async Task<bool> SetClassfications(int runId, ClassifyCalculationRunScenerio1ViewModel viewModel)
        {
            var classifications = await this.GetClassfications(new CalcFinancialYearRequestDto() { RunId = runId, FinancialYear = this.GetFinancialYear() });
            if (!classifications.IsSuccessStatusCode)
            {
                return false;
            }

            viewModel.Classifications = JsonConvert.DeserializeObject<FinancialYearClassificationResponseDto>(classifications.Content.ReadAsStringAsync().Result);
            this.SetStatusDescriptions(viewModel);
            return true;
        }

        [Route("Submit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(ClassifyCalculationRunScenerio1ViewModel model)
        {
            try
            {
                if (!this.ModelState.IsValid)
                {
                    var viewModel = await this.CreateViewModel(model.CalculatorRunDetails.RunId);
                    await this.SetClassfications(model.CalculatorRunDetails.RunId, viewModel);

                    return View(ViewNames.ClassifyingCalculationRunScenario1Index, viewModel);
                }

                return this.RedirectToAction(ActionNames.Index, ControllerNames.ClassifyRunConfirmation, new { runId = model.CalculatorRunDetails.RunId });
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "An error occurred while processing the request.");
                return this.RedirectToAction(ActionNames.StandardErrorIndex, ControllerNames.StandardErrorController);
            }
        }

        private static bool IsRunEligibleForDisplay(CalculatorRunDetailsViewModel calculatorRunDetails)
        {
            return calculatorRunDetails.RunClassificationId == RunClassification.UNCLASSIFIED;
        }

        private async Task<ClassifyCalculationRunScenerio1ViewModel> CreateViewModel(int runId)
        {
            var viewModel = new ClassifyCalculationRunScenerio1ViewModel()
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

        private async Task<HttpResponseMessage> GetClassfications(CalcFinancialYearRequestDto dto)
        {
            var apiUrl = this.GetApiUrl(
               ConfigSection.CalculationRunSettings,
               ConfigSection.ClassificationByFinancialYearApi);
            return await this.CallApi(
                HttpMethod.Get,
                apiUrl,
                $"RundId={dto.RunId}&FinancialYear={dto.FinancialYear}",
                null);
        }

        private void SetStatusDescriptions(ClassifyCalculationRunScenerio1ViewModel model)
        {
            TextInfo myTI = new CultureInfo("en-GB", false).TextInfo;
            foreach (var classification in model.Classifications.Classifications)
            {
                classification.Description = GetStatusDescription(classification.Id);
                classification.Status = myTI.ToTitleCase(classification.Status.ToLower());
            }
        }

        private string GetStatusDescription(int classificationId)
        {
            return classificationId switch
            {
               (int)RunClassification.INITIAL_RUN => CommonConstants.InitialRunDescription,
                (int)RunClassification.TEST_RUN => CommonConstants.TestRunDescription,
                _ => string.Empty,
            };
        }
    }
}