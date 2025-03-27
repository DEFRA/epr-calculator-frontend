using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClassifyCalculationRunController"/> class.
    /// </summary>
    [Authorize(Roles = "SASuperUser")]
    [Route("[controller]")]
    public class ClassifyCalculationRunController : BaseController
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory clientFactory;
        private readonly ILogger<ClassifyCalculationRunController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassifyCalculationRunController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration settings.</param>
        /// <param name="clientFactory">The HTTP client factory.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="tokenAcquisition">The token acquisition service.</param>
        /// <param name="telemetryClient">The telemetry client for logging and monitoring.</param>
        public ClassifyCalculationRunController(IConfiguration configuration, IHttpClientFactory clientFactory, ILogger<ClassifyCalculationRunController> logger, ITokenAcquisition tokenAcquisition, TelemetryClient telemetryClient)
            : base(configuration, tokenAcquisition, telemetryClient)
        {
            this.configuration = configuration;
            this.clientFactory = clientFactory;
            this.logger = logger;
        }

        /// <summary>
        /// Handles the Index action for the controller.
        /// </summary>
        /// <remarks>
        /// This action is restricted to users with the "SASuperUser" role.
        /// It initializes a <see cref="ClassifyCalculationViewModel"/> with specific parameters and sets the current user.
        /// The view returned is <see cref="ViewNames.CalculationRunClassification"/>.
        /// </remarks>
        /// <returns>
        /// An <see cref="IActionResult"/> that renders the CalculationRunClassification view with the initialized view model.
        /// </returns>
        [Authorize(Roles = "SASuperUser")]

        public IActionResult Index()
        {
            ClassifyCalculationViewModel classifyCalculationViewModel = new ClassifyCalculationViewModel("Calculation run 99", "2024-25", 240008, true, "1 December 2024 at 12:09")
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
            };
            return this.View(ViewNames.CalculationRunClassification, classifyCalculationViewModel);
        }
    }
}