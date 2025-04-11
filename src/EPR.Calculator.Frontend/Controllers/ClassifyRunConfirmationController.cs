using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClassifyRunConfirmationController"/> class.
    /// </summary>
    [Authorize(Roles = "SASuperUser")]
    public class ClassifyRunConfirmationController : BaseController
    {
        private readonly ILogger<ClassifyRunConfirmationController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassifyRunConfirmationController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration settings.</param>
        /// <param name="clientFactory">The HTTP client factory.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="tokenAcquisition">The token acquisition service.</param>
        /// <param name="telemetryClient">The telemetry client.</param>
        public ClassifyRunConfirmationController(IConfiguration configuration, IHttpClientFactory clientFactory, ILogger<ClassifyRunConfirmationController> logger, ITokenAcquisition tokenAcquisition, TelemetryClient telemetryClient)
            : base(configuration, tokenAcquisition, telemetryClient)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Classify run confirmation index view.
        /// </summary>
        /// <param name="runId">The ID of the calculation run.</param>
        /// <returns>The classify run confirmation index view.</returns>
        [Authorize(Roles = "SASuperUser")]
        [Route("ClassifyRunConfirmation/{runId}")]
        [HttpGet]
        public IActionResult Index(int runId)
        {
            try
            {
                var statusUpdateViewModel = new ClassifyRunConfirmationViewModel
                {
                    CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                    Data = new CalculatorRunDto
                    {
                        RunId = runId,
                        RunClassificationId = 240008,
                        RunName = "Calculation run 99",
                        CreatedAt = DateTime.Now,
                        FileExtension = ".csv",
                        RunClassificationStatus = "3",
                        FinancialYear = "2024-25",
                        Classification = "Initial run",
                    },
                };

                return this.View(ViewNames.ClassifyRunConfirmationIndex, statusUpdateViewModel);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "An error occurred while processing the request.");
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
        }
    }
}
