using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Extensions;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Controller for handling classifying calculation run scenario 1.
    /// </summary>
    /// <param name="configuration">The configuration settings.</param>
    /// <param name="clientFactory">The HTTP client factory.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="telemetryClient">telemetry client.</param>
    [Route("[controller]")]
    public class SetRunClassificationController(
        IConfiguration configuration,
        IEprCalculatorApiService eprCalculatorApiService,
        ILogger<SetRunClassificationController> logger,
        TelemetryClient telemetryClient,
        ICalculatorRunDetailsService calculatorRunDetailsService)
        : BaseController(
            configuration,
            telemetryClient,
            eprCalculatorApiService,
            calculatorRunDetailsService)
    {
        private readonly ILogger<SetRunClassificationController> logger = logger;
        private readonly int relativeYearStartingMonth = CommonUtil.GetRelativeYearStartingMonth(configuration);

        [Route("{runId}")]
        [HttpGet]
        public async Task<IActionResult> Index(int runId)
        {
            var relativeYear = CommonUtil.GetRelativeYear(this.HttpContext.Session, this.relativeYearStartingMonth);

            var classificationsResponse = await this.GetClassfications(new CalcRelativeYearRequestDto
            {
                RunId = runId,
                RelativeYear = relativeYear,
            });

            var responseContent = await classificationsResponse.Content.ReadAsStringAsync();
            var relativeYearClassificationResponseDto = JsonConvert.DeserializeObject<RelativeYearClassificationResponseDto>(responseContent);
            if (relativeYearClassificationResponseDto == null)
            {
                this.TelemetryClient.TrackTrace($"API did not return successful.");
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }

            var viewModel = await this.CreateViewModel(runId);
            var classifyViewModel = ImportantRunClassificationHelper.CreateclassificationViewModel(
                relativeYearClassificationResponseDto!.ClassifiedRuns,
                relativeYear);

            viewModel.ImportantViewModel = classifyViewModel;

            if (!await this.SetClassifications(runId, viewModel))
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }

            if (viewModel.CalculatorRunDetails == null || viewModel.CalculatorRunDetails.RunId == 0)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
            else
            {
                // Only Test Run available
                if (relativeYearClassificationResponseDto.Classifications.Count == 1 &&
                    relativeYearClassificationResponseDto.Classifications.Exists(x => x.Id == (int)RunClassification.TEST_RUN) && !classifyViewModel.IsAnyRunInProgress)
                {
                    classifyViewModel.IsDisplayTestRun = true;
                }

                viewModel.ImportantViewModel = classifyViewModel;
                return this.View(ViewNames.SetRunClassificationIndex, viewModel);
                }
        }

        [Route("Submit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(SetRunClassificationViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                var viewModel = await this.CreateViewModel(model.CalculatorRunDetails.RunId);
                await this.SetClassifications(model.CalculatorRunDetails.RunId, viewModel);

                return this.View(ViewNames.SetRunClassificationIndex, viewModel);
            }

            var result = await this.EprCalculatorApiService.CallApi(
                httpContext: this.HttpContext,
                httpMethod: HttpMethod.Put,
                relativePath: "v2/calculatorRuns",
                body: new ClassificationDto
                {
                    RunId = model.CalculatorRunDetails.RunId,
                    ClassificationId = (int)model.ClassifyRunType!,
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

        private async Task<HttpResponseMessage> GetClassfications(CalcRelativeYearRequestDto dto)
        {
            return await this.EprCalculatorApiService.CallApi(
                httpContext: this.HttpContext,
                httpMethod: HttpMethod.Get,
                relativePath: "v1/ClassificationByRelativeYear",
                queryParams: new Dictionary<string, string?>
                {
                    ["RunId"] = dto.RunId.ToString(),
                    ["RelativeYearValue"] = dto.RelativeYear.ToString(),
                });
        }

        private void SetStatusDescriptions(SetRunClassificationViewModel model)
        {
            foreach (var classification in model.RelativeYearClassifications?.Classifications ?? Enumerable.Empty<CalculatorRunClassificationDto>())
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
            var classifications = await this.GetClassfications(new CalcRelativeYearRequestDto() {
                RunId = runId,
                RelativeYear = CommonUtil.GetRelativeYear(this.HttpContext.Session, this.relativeYearStartingMonth)
            });

            if (!classifications.IsSuccessStatusCode)
            {
                return false;
            }

            viewModel.RelativeYearClassifications = JsonConvert.DeserializeObject<RelativeYearClassificationResponseDto>(classifications.Content.ReadAsStringAsync().Result);
            this.SetStatusDescriptions(viewModel);

            if (viewModel.RelativeYearClassifications != null)
            {
                viewModel.ClassificationStatusInformation = this.GetClassificationStatusInformation(viewModel.RelativeYearClassifications.Classifications);
            }

            return true;
        }

        private ClassificationStatusInformationViewModel GetClassificationStatusInformation(List<CalculatorRunClassificationDto> classificationList)
        {
            ClassificationStatusInformationViewModel classificationStatusInformationViewModel = new ClassificationStatusInformationViewModel();

            if (classificationList != null && classificationList.Count > 0)
            {
                var relativeYear = CommonUtil.GetRelativeYear(this.HttpContext.Session, this.relativeYearStartingMonth);

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
                    classificationStatusInformationViewModel.InitialRunDescription = string.Format(ClassifyCalculationRunStatusInformation.RunStatusDescription, relativeYear);

                    // check if calculator classification list does not have final recalculation run status to be initiated
                    if (!classificationList.Exists(n => n.Id == (int)RunClassification.FINAL_RECALCULATION_RUN))
                    {
                        classificationStatusInformationViewModel.ShowFinalRecalculationRunDescription = true;
                        classificationStatusInformationViewModel.FinalRecalculationRunDescription = $"{string.Format(ClassifyCalculationRunStatusInformation.RunStatusDescription, relativeYear)}";
                    }

                    // check if list doesn't have initial run status
                    if (!classificationList.Exists(n => n.Id == (int)RunClassification.FINAL_RUN))
                    {
                        classificationStatusInformationViewModel.ShowFinalRunDescription = true;
                        classificationStatusInformationViewModel.FinalRunDescription = $"{string.Format(ClassifyCalculationRunStatusInformation.RunStatusDescription, relativeYear)}";
                    }
                }
            }

            return classificationStatusInformationViewModel;
        }
    }
}