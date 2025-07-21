using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Controller for handling result/billing file downloads.
    /// </summary>
    [Route("[controller]")]
    public class DownloadFileController(
        IConfiguration configuration,
        ITokenAcquisition tokenAcquisition,
        TelemetryClient telemetryClient,
        IHttpClientFactory httpClientFactory)
        : BaseController(configuration, tokenAcquisition, telemetryClient, httpClientFactory)
    {
        /// <summary>
        /// Download file entry point.
        /// </summary>
        /// <param name="runId">The ID of the calculation run.</param>
        /// <returns>A results or download file.</returns>
        [HttpGet]
        [Route("{runId}")]
        public async Task<IActionResult> Index(int runId)
        {
            throw new NotImplementedException("This method is not implemented yet. Please use PostBillingFileController instead.");
        }
    }
}
