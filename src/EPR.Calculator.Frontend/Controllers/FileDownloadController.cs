using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Services;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Controller for handling file downloads related to calculation runs.
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="tokenAcquisition"></param>
    /// <param name="telemetryClient"></param>
    /// <param name="clientFactory"></param>
    /// <param name="fileDownloadService"></param>
    public class FileDownloadController(IConfiguration configuration,
        ITokenAcquisition tokenAcquisition,
        TelemetryClient telemetryClient,
        IHttpClientFactory clientFactory,
        IResultBillingFileService fileDownloadService) : BaseController(configuration, tokenAcquisition, telemetryClient, clientFactory)
    {
        private readonly IResultBillingFileService fileDownloadService = fileDownloadService;

        [HttpGet]
        [Route("DownloadResultFile/{runId}")]
        public async Task<IActionResult> DownloadResultFile(int runId)
        {
            try
            {
                var apiUrl = this.GetApiUrl(ConfigSection.CalculationRunSettings, ConfigSection.DownloadResultApi);

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
                var apiUrl = this.GetApiUrl(ConfigSection.CalculationRunSettings, ConfigSection.DownloadCsvBillingApi);

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
