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
    [Route("[controller]")]
    public class ClassifyRunConfirmationController : BaseController
    {
        private readonly ILogger<ClassifyRunConfirmationController> logger;

        public ClassifyRunConfirmationController(IConfiguration configuration, IHttpClientFactory clientFactory, ILogger<ClassifyRunConfirmationController> logger, ITokenAcquisition tokenAcquisition, TelemetryClient telemetryClient)
            : base(configuration, tokenAcquisition, telemetryClient)
        {
            this.logger = logger;
        }

        [Route("{runId}")]
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
                        RunName = "Calculation Run 99",
                        CreatedAt = new DateTime(2024, 5, 1, 12, 09, 0, DateTimeKind.Utc),
                        FileExtension = ".csv",
                        RunClassificationStatus = "3",
                        FinancialYear = "2024-25",
                        Classification = "Initial run",
                    },
                    BackLink = ControllerNames.ClassifyingCalculationRun,
                };

                return this.View(ViewNames.ClassifyRunConfirmationIndex, statusUpdateViewModel);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "An error occurred while processing the request.");
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Submit(int runId)
        {
            if (!this.ModelState.IsValid)
            {
                return RedirectToAction(ActionNames.Index, new { runId });
            }

            return RedirectToAction(ActionNames.Index, ControllerNames.PaymentCalculator, new { runId = runId });
        }
    }
}
