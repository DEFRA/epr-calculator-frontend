using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
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
        [Route("{runId}")]
        public async Task<IActionResult> Index(int runId)
        {
            var runDetails = await this.GetCalculatorRundetails(runId);

            var viewModel = new ReasonForRejectionViewModel()
            {
                CalculationRunId = runId,
                CalculationRunName = runDetails?.RunName,
                Reason = string.Empty,
                BackLink = ControllerNames.BillingInstructionsController,
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
            };

            return this.View(ViewNames.ReasonForRejectionIndex, viewModel);
        }

        [HttpPost]
        [ActionName("Index")]
        [Route("{runId}")]
        public IActionResult IndexPost(int runId, ReasonForRejectionViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(ViewNames.ReasonForRejectionIndex, model);
            }

            return this.RedirectToAction(ActionNames.Index, new { runId= model.CalculationRunId });
        }
    }
}
