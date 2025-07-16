using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Configuration;
using System.Net.Http;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Controller for reason for rejection
    /// </summary>
    [Route("[controller]")]
    public class ReasonForRejectionController(
        IConfiguration configuration,
        ITokenAcquisition tokenAcquisition,
        TelemetryClient telemetryClient,
        IHttpClientFactory httpClientFactory)
        : BaseController(configuration, tokenAcquisition, telemetryClient, httpClientFactory)
    {
        [Route("{calculationRunId}")]
        public async Task<IActionResult> Index(int calculationRunId)
        {
            var runDetails = await this.GetCalculatorRundetails(calculationRunId);

            var viewModel = new AcceptRejectConfirmationViewModel()
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                CalculationRunId = calculationRunId,
                CalculationRunName = runDetails?.RunName,
                Reason = string.Empty,
                Status = BillingStatus.Rejected,
                BackLink = ControllerNames.BillingInstructionsController,
            };

            return this.View(ViewNames.ReasonForRejectionIndex, viewModel);
        }

        [HttpPost]
        [ActionName("Index")]
        [Route("{runId}")]
        public IActionResult IndexPost(int runId, AcceptRejectConfirmationViewModel model)
        {
            if (string.IsNullOrEmpty(model.Reason))
            {
                this.ModelState.AddModelError(nameof(model.Reason), "Provide a reason that applies to all the billing instructions you selected for rejection.");
                return this.View(ViewNames.ReasonForRejectionIndex, model);
            }

            this.ModelState.Clear();
            return this.View(ViewNames.AcceptRejectConfirmationIndex, model);
        }
    }
}
