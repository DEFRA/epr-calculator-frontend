using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Services;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Controller for handling file downloads related to calculation runs.
    /// </summary>
    public class FileDownloadController : BaseController
    {
        private readonly IResultBillingFileService fileDownloadService;

        public FileDownloadController(
            IConfiguration configuration,
            IEprCalculatorApiService eprCalculatorApiService,
            TelemetryClient telemetryClient,
            IResultBillingFileService fileDownloadService,
            ICalculatorRunDetailsService calculatorRunDetailsService)
            : base(configuration, telemetryClient, eprCalculatorApiService, calculatorRunDetailsService)
        {
            this.fileDownloadService = fileDownloadService;
        }

        [HttpGet]
        [Route("DownloadResultFile/{runId}")]
        public async Task<IActionResult> DownloadResultFile(int runId)
        {
            try
            {
                return await this.fileDownloadService.DownloadFileAsync($"v1/DownloadResult/{runId}", runId, this.HttpContext);
            }
            catch (Exception ex)
            {
                this.TelemetryClient.TrackException(ex);
                return this.RedirectToAction(ActionNames.IndexNew, ControllerNames.DownloadFileErrorNewController, new { runId });
            }
        }

        [HttpGet]
        [Route("DownloadBillingFile/{runId}")]
        public async Task<IActionResult> DownloadBillingFile(int runId, bool isBillingFile, bool isDraftBillingFile)
        {
            try
            {
                var runDetails = await this.CalculatorRunDetailsService.GetCalculatorRundetailsAsync(
                this.HttpContext,
                runId);

                if (runDetails == null || runDetails.RunName == null)
                {
                    return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
                }

                if (runDetails.IsBillingFileGeneratedLatest.HasValue && !runDetails.IsBillingFileGeneratedLatest.Value)
                {
                    return this.RedirectToAction(ActionNames.Index, ControllerNames.CalculationRunOverview, new { runId });
                }

                return await this.fileDownloadService.DownloadFileAsync($"v1/DownloadBillingFile/{runId}", runId, this.HttpContext, isBillingFile, isDraftBillingFile);
            }
            catch (Exception ex)
            {
                this.TelemetryClient.TrackException(ex);
                return this.RedirectToAction(ActionNames.IndexNew, ControllerNames.DownloadFileErrorNewController, new { runId });
            }
        }
    }
}
