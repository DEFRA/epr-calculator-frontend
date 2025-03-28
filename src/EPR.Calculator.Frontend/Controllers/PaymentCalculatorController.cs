using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentCalculatorController"/> class.
    /// </summary>
    [Authorize(Roles = "SASuperUser")]
    [Route("payment-calculator")]
    public class PaymentCalculatorController : BaseController
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory clientFactory;
        private readonly ILogger<PaymentCalculatorController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentCalculatorController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration settings.</param>
        /// <param name="clientFactory">The HTTP client factory.</param>
        /// <param name="logger">The logger instance.</param>
        public PaymentCalculatorController(IConfiguration configuration, IHttpClientFactory clientFactory, ILogger<PaymentCalculatorController> logger, ITokenAcquisition tokenAcquisition, TelemetryClient telemetryClient)
            : base(configuration, tokenAcquisition, telemetryClient)
        {
            this.configuration = configuration;
            this.clientFactory = clientFactory;
            this.logger = logger;
        }

        [HttpGet]
        [Route("accept-invoice-instructions")]
        public IActionResult AcceptInvoiceInstructions()
        {
            var model = new AcceptInvoiceInstructionsViewModel
            {
                AcceptAll = false,
                ReturnUrl = this.Url.Action("Overview", "PaymentCalculator") ?? "/", // dummy return url
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                CalculationRunTitle = "Calculation run 99"
            };

            return this.View(model);
        }

        [HttpPost]
        [Route("accept-invoice-instructions")]
        public IActionResult AcceptInvoiceInstructions(AcceptInvoiceInstructionsViewModel model)
        {
            if (model.AcceptAll)
            {
                return this.RedirectToAction("Overview"); // dummy return url
            }

            this.ModelState.AddModelError("AcceptAll", "You must confirm acceptance to proceed.");
            return this.View(model);
        }
    }
}
