using System.Globalization;
using System.Net;
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
        IApiService apiService,
        ILogger<SetRunClassificationController> logger,
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
        private readonly ILogger<SetRunClassificationController> logger = logger;

        [Route("{runId}")]
        [HttpGet]
        public async Task<IActionResult> Index(int runId)
        {
            try
            {
                var viewModel = await this.CreateViewModel(runId);
                if (!await this.SetClassifications(runId, viewModel))
                {
                    return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
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
                    var viewModel = await this.CreateViewModel(model.CalculatorRunDetails.RunId);
                    await this.SetClassifications(model.CalculatorRunDetails.RunId, viewModel);

                    return this.View(ViewNames.SetRunClassificationIndex, viewModel);
                }

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
                    ClassificationId = (int)model.ClassifyRunType,
                });

                if (result.StatusCode == HttpStatusCode.Created)
                {
                    return this.RedirectToAction(ActionNames.Index, ControllerNames.ClassifyRunConfirmation, new { runId = model.CalculatorRunDetails.RunId });
                }
                else
                {
                    var message = $"API did not return successful ({result.StatusCode}).";
                    this.logger.LogError(message);
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

            var runDetails = await this.CalculatorRunDetailsService.GetCalculatorRundetailsAsync(
                this.HttpContext,
                runId);
            if (runDetails != null && runDetails!.RunId != 0)
            {
                viewModel.CalculatorRunDetails = runDetails;
            }

            return viewModel;
        }

        private async Task<HttpResponseMessage> GetClassfications(CalcFinancialYearRequestDto dto)
        {
            var apiUrl = this.ApiService.GetApiUrl(
               ConfigSection.CalculationRunSettings,
               ConfigSection.ClassificationByFinancialYearApi);
            return await this.ApiService.CallApi(
                this.HttpContext,
                HttpMethod.Get,
                apiUrl,
                $"RunId={dto.RunId}&FinancialYear={dto.FinancialYear}",
                null);
        }

        private void SetStatusDescriptions(SetRunClassificationViewModel model)
        {
            foreach (var classification in model.FinancialYearClassifications.Classifications)
            {
                classification.Description = this.GetStatusDescription(classification.Id);
                classification.Status = this.GetStatus(classification);
            }
        }

        private string GetStatusDescription(int classificationId)
        {
            return classificationId switch
            {
                (int)RunClassification.INITIAL_RUN => CommonConstants.InitialRunDescription,
                (int)RunClassification.TEST_RUN => CommonConstants.TestRunDescription,
                (int)RunClassification.INTERIM_RECALCULATION_RUN => CommonConstants.InterimRunDescription,
                (int)RunClassification.FINAL_RECALCULATION_RUN => CommonConstants.FinalRecalculationRunDescription,
                (int)RunClassification.FINAL_RUN => CommonConstants.FinalRecalculationRunDescription,
                _ => string.Empty,
            };
        }

        private string GetStatus(CalculatorRunClassificationDto classificationDto)
        {
            TextInfo myTI = new CultureInfo("en-GB", false).TextInfo;
            return classificationDto.Id switch
            {
                (int)RunClassification.INITIAL_RUN => CommonConstants.InitialRunStatus,
                (int)RunClassification.TEST_RUN => CommonConstants.TestRunStatus,
                (int)RunClassification.INTERIM_RECALCULATION_RUN => CommonConstants.InterimRunStatus,
                (int)RunClassification.FINAL_RECALCULATION_RUN => CommonConstants.FinalRecalculationRunStatus,
                (int)RunClassification.FINAL_RUN => CommonConstants.FinalRunStatus,
                _ => myTI.ToFirstLetterCap(classificationDto.Status),
            };
        }


        private async Task<bool> SetClassifications(int runId, SetRunClassificationViewModel viewModel)
        {
            var classifications = await this.GetClassfications(new CalcFinancialYearRequestDto() { RunId = runId, FinancialYear = CommonUtil.GetFinancialYear(this.HttpContext.Session) });
            if (!classifications.IsSuccessStatusCode)
            {
                return false;
            }

            viewModel.FinancialYearClassifications = JsonConvert.DeserializeObject<FinancialYearClassificationResponseDto>(classifications.Content.ReadAsStringAsync().Result);
            this.SetStatusDescriptions(viewModel);

            if (viewModel.FinancialYearClassifications != null)
            {
                viewModel.ClassificationStatusInformation = this.GetClassificationStatusInformation(viewModel.FinancialYearClassifications.Classifications);
            }

            return true;
        }

        private ClassificationStatusInformationViewModel GetClassificationStatusInformation(List<CalculatorRunClassificationDto> classificationList)
        {
            ClassificationStatusInformationViewModel classificationStatusInformationViewModel = new ClassificationStatusInformationViewModel();

            if (classificationList != null && classificationList.Count > 0)
            {
                string financialYear = CommonUtil.GetFinancialYear(this.HttpContext.Session);

                // check if calculator classification list have initial run
                if (classificationList.Exists(n => n.Id == (int)RunClassification.INITIAL_RUN))
                {
                    classificationStatusInformationViewModel.ShowInterimRecalculationRunDescription = true;
                    classificationStatusInformationViewModel.InterimRecalculationRunDescription = ClassifyCalculationRunStatusInformation.InterimReCalculationStatusDescription;

                    classificationStatusInformationViewModel.ShowFinalRecalculationRunDescription = true;
                    classificationStatusInformationViewModel.FinalRecalculationRunDescription = ClassifyCalculationRunStatusInformation.FinalRecalculationStatusDescription;

                    classificationStatusInformationViewModel.ShowFinalRunDescription = true;
                    classificationStatusInformationViewModel.FinalRunDescription = ClassifyCalculationRunStatusInformation.FinalRunStatusDescription;
                }
                else
                {
                    classificationStatusInformationViewModel.ShowInitialRunDescription = true;
                    classificationStatusInformationViewModel.InitialRunDescription = string.Format(ClassifyCalculationRunStatusInformation.RunStatusDescription, financialYear);

                    // check if calculator classification list does not have final recalculation run status to be initiated
                    if (!classificationList.Exists(n => n.Id == (int)RunClassification.FINAL_RECALCULATION_RUN))
                    {
                        classificationStatusInformationViewModel.ShowFinalRecalculationRunDescription = true;
                        classificationStatusInformationViewModel.FinalRecalculationRunDescription = $"{string.Format(ClassifyCalculationRunStatusInformation.RunStatusDescription, financialYear)}";
                    }

                    // check if list doesn't have initial run status
                    if (!classificationList.Exists(n => n.Id == (int)RunClassification.FINAL_RUN))
                    {
                        classificationStatusInformationViewModel.ShowFinalRunDescription = true;
                        classificationStatusInformationViewModel.FinalRunDescription = $"{string.Format(ClassifyCalculationRunStatusInformation.RunStatusDescription, financialYear)}";
                    }
                }
            }

            return classificationStatusInformationViewModel;
        }
    }
}