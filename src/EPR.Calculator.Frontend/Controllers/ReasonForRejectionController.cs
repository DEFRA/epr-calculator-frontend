using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Configuration;
using System.Drawing;
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
        IApiService apiService,
        ICalculatorRunDetailsService calculatorRunDetailsService)
        : BaseController(configuration, tokenAcquisition, telemetryClient, apiService, calculatorRunDetailsService)
    {
        [Route("{calculationRunId}")]
        public async Task<IActionResult> Index(int calculationRunId)
        {
            var runDetails = await this.CalculatorRunDetailsService
                .GetCalculatorRundetailsAsync(this.HttpContext, calculationRunId);

            var viewModel = new AcceptRejectConfirmationViewModel()
            {
                CurrentUser = currentUser,
                CalculationRunId = calculationRunId,
                CalculationRunName = runDetails?.RunName,
                Reason = this.TempData[nameof(AcceptRejectConfirmationViewModel.Reason)]?.ToString() ?? string.Empty,
                Status = BillingStatus.Rejected,
                BackLinkViewModel = new BackLinkViewModel
                {
                    BackLink = ControllerNames.BillingInstructionsController,
                    RunId = calculationRunId,
                    CurrentUser = currentUser,
                },
            };

            return this.View(ViewNames.ReasonForRejectionIndex, viewModel);
        }

        [HttpPost]
        [ActionName("Index")]
        [Route("{calculationRunId}")]
        public IActionResult IndexPost(int calculationRunId, AcceptRejectConfirmationViewModel model)
        {
            if (string.IsNullOrEmpty(model.Reason))
            {
                this.ModelState.Remove("Reason");
                return this.View(ViewNames.ReasonForRejectionIndex, model);
            }

            this.ModelState.Clear();
            model.BackLinkViewModel = new BackLinkViewModel
            {
                BackLink = ControllerNames.ReasonForRejectionController,
                RunId = calculationRunId,
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
            };
            this.TempData[nameof(model.Reason)] = model.Reason;
            return this.View(ViewNames.AcceptRejectConfirmationIndex, model);
        }
    }
}
