using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EPR.Calculator.Frontend.Controllers
{
    [Authorize(Roles = "SASuperUser")]
    [Route("[controller]")]
    public class SendBillingFileController : BaseController
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationRunDetailsNewController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration settings.</param>
        /// <param name="clientFactory">The HTTP client factory.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="tokenAcquisition">The token acquisition service.</param>
        /// <param name="telemetryClient">The telemetry client.</param>
        public SendBillingFileController(
            IConfiguration configuration,
            IHttpClientFactory clientFactory,
            ILogger<SendBillingFileController> logger,
            ITokenAcquisition tokenAcquisition,
            TelemetryClient telemetryClient)
            : base(configuration, tokenAcquisition, telemetryClient)
    {
            _configuration = configuration;
        }

        [Route("sendbillingfile/{runId}")]
        public IActionResult Index(int runId)
        {
            var billingFileViewModel = new SendBillingFileViewModel()
            {
                RunId = runId,
                CalcRunName = "Calculation run 99",
                ConfirmationContent = CommonConstants.ConfirmationContent,
                SendBillFileHeading = CommonConstants.SendBillingFile,
                WarningContent = CommonConstants.WarningContent,
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                BackLink = string.Empty,
            };

            return this.View(billingFileViewModel);
        }

        [HttpPost]
        public IActionResult SubmitSendBillingFile(int runId)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index", new { runId });
            }

            return RedirectToAction("Index", "CalculationRunDelete", new { runId = runId });
        }
    }
}
