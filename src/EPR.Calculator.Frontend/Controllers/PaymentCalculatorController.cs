﻿using EPR.Calculator.Frontend.Constants;
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
    /// Controller for handling payment calculations.
    /// </summary>
    [Route("[controller]")]
    public class PaymentCalculatorController : BaseController
    {
        public PaymentCalculatorController(IConfiguration configuration, ITokenAcquisition tokenAcquisition, TelemetryClient telemetryClient) : base(configuration, tokenAcquisition, telemetryClient)
        {
        }

        [HttpGet]
        [Route("{runId}")]
        public IActionResult Index(int runId)
        {
            var model = new AcceptInvoiceInstructionsViewModel
            {
                RunId = runId,
                AcceptAll = false,
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                CalculationRunTitle = "Calculation Run 99",
                BackLink = ControllerNames.ClassifyRunConfirmation,
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Submit(int runId)
        {
            if (!this.ModelState.IsValid)
            {
                return RedirectToAction(ActionNames.Index, new { runId });
            }

            model.Errors.Add(new ErrorViewModel
            {
                DOMElementId = "AcceptAll",
                ErrorMessage = "You must confirm acceptance to proceed.",
            });
            return this.View(model);
        }

        /// <summary>
        /// Displays a billing file sent confirmation screen.
        /// </summary>
        /// <returns>Billing file sent page.</returns>
        [Route("BillingFileSuccess")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BillingFileSuccess()
        {
            var model = new BillingFileSuccessViewModel
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                ConfirmationViewModel = new ConfirmationViewModel
                {
                    Title = ConfirmationMessages.BillingFileSuccessTitle,
                    Body = ConfirmationMessages.BillingFileSuccessBody,
                    AdditionalParagraphs = ConfirmationMessages.BillingFileSuccessAdditionalParagraphs,
                    RedirectController = ControllerNames.Dashboard,
                },
            };

            return this.View(ViewNames.BillingConfirmationSuccess, model);
        }
    }
}