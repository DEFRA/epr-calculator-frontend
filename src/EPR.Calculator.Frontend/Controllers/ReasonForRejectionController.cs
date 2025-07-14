using EPR.Calculator.Frontend.Constants;
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
            if (!this.ModelState.IsValid) { }

            var viewModel = new ReasonForRejectionViewModel()
            {
                CalculationRunId = runDetails.RunId,
                CalculationRunName = runDetails.RunName,
                Reason = string.Empty,
                BackLink = ControllerNames.BillingInstructionsController,
            };

            return View(viewModel);
        }

        [HttpPost]
        [ActionName("Index")]
        [Route("{runId}")]
        public IActionResult IndexPost(int runId, ReasonForRejectionViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            return this.RedirectToAction(ActionNames.Index, new { model.CalculationRunId });
        }
    }
}
