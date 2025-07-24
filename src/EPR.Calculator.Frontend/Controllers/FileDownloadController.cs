using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Services;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Controller for handling file downloads related to calculation runs and billing files.
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="apiService"></param>
    /// <param name="tokenAcquisition"></param>
    /// <param name="telemetryClient"></param>
    /// <param name="fileDownloadService"></param>
    public class FileDownloadController : BaseController
    {
        private readonly IApiService apiService;
        private readonly IResultBillingFileService fileDownloadService;

        public FileDownloadController(
            IConfiguration configuration,
            IApiService apiService,
            ITokenAcquisition tokenAcquisition,
            TelemetryClient telemetryClient,
            IResultBillingFileService fileDownloadService,
            ICalculatorRunDetailsService calculatorRunDetailsService)
            : base(configuration, tokenAcquisition, telemetryClient, apiService, calculatorRunDetailsService)
        {
            this.apiService = apiService;
            this.fileDownloadService = fileDownloadService;
        }

        [HttpGet]
        [Route("DownloadResultFile/{runId}")]
        public async Task<IActionResult> DownloadResultFile(int runId)
        {
            try
            {
                var apiUrl = apiService.GetApiUrl(ConfigSection.CalculationRunSettings, ConfigSection.DownloadResultApi);

                var accessToken = await this.AcquireToken();
                return await this.fileDownloadService.DownloadFileAsync(apiUrl, runId, accessToken);
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
                var apiUrl = apiService.GetApiUrl(ConfigSection.CalculationRunSettings, ConfigSection.DownloadCsvBillingApi);

                var accessToken = await this.AcquireToken();
                return await this.fileDownloadService.DownloadFileAsync(apiUrl, runId, accessToken, isBillingFile, isDraftBillingFile);
            }
            catch (Exception ex)
            {
                this.TelemetryClient.TrackException(ex);
                return this.RedirectToAction(ActionNames.IndexNew, ControllerNames.DownloadFileErrorNewController, new { runId });
            }
        }
    }
}
