using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
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
    /// Controller for classifying after final run.
    /// </summary>
    [Route("[controller]")]
    public class ClassifyAfterFinalRunController : BaseController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassifyAfterFinalRunController"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="tokenAcquisition">The token acquisition service.</param>
        /// <param name="telemetryClient">The telemetry client.</param>
        /// <param name="apiService">The API service.</param>
        /// <param name="calculatorRunDetailsService">The calculator run details service.</param>
        public ClassifyAfterFinalRunController(
            IConfiguration configuration,
            ITokenAcquisition tokenAcquisition,
            TelemetryClient telemetryClient,
            IApiService apiService,
            ICalculatorRunDetailsService calculatorRunDetailsService)
            : base(configuration, tokenAcquisition, telemetryClient, apiService, calculatorRunDetailsService)
        {
        }

        /// <summary>
        /// Displays the classify after final run page.
        /// </summary>
        /// <param name="runId">The ID of the run.</param>
        /// <returns>The classify after final run view.</returns>
        [Route("{runId}")]
        public async Task<IActionResult> IndexAsync(int runId)
        {
            var financialYear = CommonUtil.GetFinancialYear(this.HttpContext.Session);

            var classificationsResponse = await this.GetClassfications(new CalcFinancialYearRequestDto
            {
                RunId = runId,
                FinancialYear = financialYear,
            });
            var responseContent = await classificationsResponse.Content.ReadAsStringAsync();
            var financialYearClassificationResponseDto = JsonConvert.DeserializeObject<FinancialYearClassificationResponseDto>(responseContent);

            var completedStatusIds = new List<int>
                {
                    (int)RunClassification.INITIAL_RUN_COMPLETED,
                    (int)RunClassification.FINAL_RECALCULATION_RUN_COMPLETED,
                    (int)RunClassification.FINAL_RUN_COMPLETED,
                };

            var completedRuns = financialYearClassificationResponseDto?.ClassifiedRuns?
                .Where(run => completedStatusIds.Contains(run.RunClassificationId))
                .OrderByDescending(run => run.CreatedAt) // Ensure the latest run is first
                .ToList() ?? new List<ClassifiedCalculatorRunDto>();

            var latestCompletedRun = completedRuns.FirstOrDefault(x => x.RunClassificationId == (int)RunClassification.UNCLASSIFIED); // unclassifiled Run

            var classifyAfterFinalRunViewModel = new ClassifyAfterFinalRunViewModel
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                CalculatorRunStatus = latestCompletedRun != null
                    ? new CalculatorRunStatusUpdateDto
                    {
                        RunId = latestCompletedRun.RunId,
                        ClassificationId = latestCompletedRun.RunClassificationId,
                        CalcName = latestCompletedRun.RunName,
                        CreatedDate = latestCompletedRun.CreatedAt.ToString("dd MMM yyyy"),
                        CreatedTime = latestCompletedRun.CreatedAt.ToString("HH:mm"),
                        UpdatedTime = latestCompletedRun.UpdatedAt?.ToString("HH:mm") ?? string.Empty,
                        FinancialYear = financialYear,
                    }
                    : new CalculatorRunStatusUpdateDto
                    {
                        RunId = runId,
                        ClassificationId = 0,
                        CalcName = string.Empty,
                        CreatedDate = string.Empty,
                        CreatedTime = string.Empty,
                        FinancialYear = financialYear,
                    },
                ClassifiedCalculatorRun = completedRuns,
            };

            return this.View(ViewNames.ClassifyAfterFinalRunIndex, classifyAfterFinalRunViewModel);
        }

        /// <summary>
        /// Gets the classifications for a financial year.
        /// </summary>
        /// <param name="dto">The financial year request DTO.</param>
        /// <returns>The HTTP response message.</returns>
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
    }
}
