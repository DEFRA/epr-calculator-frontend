﻿using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Net;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Controller for handling payment calculations.
    /// </summary>
    [Route("[controller]")]
    public class PaymentCalculatorController(
        IConfiguration configuration,
        ITokenAcquisition tokenAcquisition,
        TelemetryClient telemetryClient,
        IHttpClientFactory httpClientFactory)
        : BaseController(configuration, tokenAcquisition, telemetryClient, httpClientFactory)
    {
        /// <summary>
        /// The main action method that handles the request to display the AcceptInvoiceInstructionsViewModel view.
        /// </summary>
        /// <param name="runId">The identifier of the calculation run.</param>
        /// <returns>An IActionResult representing the view with the populated model.</returns>
        [HttpGet]
        [Route("{runId:int}")]
        public async Task<IActionResult> Index(int runId)
        {
            var acceptInvoiceInstructionsViewModel = this.InitializeAcceptInvoiceInstructionsViewModel();

            var runDetails = await this.GetCalculatorRundetails(runId);

            if (runDetails != null && runDetails!.RunId != 0)
            {
                acceptInvoiceInstructionsViewModel.RunId = runId;
                acceptInvoiceInstructionsViewModel.CalculationRunTitle = runDetails.RunName;
            }

            return this.View(acceptInvoiceInstructionsViewModel);
        }

        /// <summary>
        /// Handles the submission of invoice acceptance instructions. Validates the model, prepares the billing file request, and triggers the billing file generation via API.
        /// </summary>
        /// <param name="model">The view model containing invoice instruction data and the calculator run ID.</param>
        /// <returns>
        /// A redirection to the calculation run overview on success and a redirection to the error page or a re-render of the current view if validation fails or an error occurs.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(AcceptInvoiceInstructionsViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(ViewNames.PaymentCalculatorIndex, model);
            }

            var billingFileApiUrl = this.GetApiUrl(ConfigSection.BillingFileSettings, ConfigSection.BillingFileApi);

            var billingFileRequestDto = new GenerateBillingFileRequestDto
            {
                CalculatorRunId = model.RunId,
            };

            try
            {
                var response = await this.CallApi(HttpMethod.Post, billingFileApiUrl, string.Empty, billingFileRequestDto);

                if (response.StatusCode != HttpStatusCode.Accepted)
                {
                    this.TelemetryClient.TrackTrace($"Billing file generation failed for RunId {model.RunId}. StatusCode: {response.StatusCode}, Reason: {response.ReasonPhrase}");
                    return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
                }
            }
            catch (Exception ex)
            {
                this.TelemetryClient.TrackException(ex);
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }

            return this.RedirectToAction(ActionNames.Index, ControllerNames.CalculationRunOverview, new { model.RunId });
        }

        /// <summary>
        /// Displays a billing file sent confirmation screen.
        /// </summary>
        /// <returns>Billing file sent page.</returns>
        [Route("BillingFileSuccess")]
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

        /// <summary>
        /// Initializes and returns a new AcceptInvoiceInstructionsViewModel with default values.
        /// </summary>
        /// <returns>An initialized AcceptInvoiceInstructionsViewModel instance.</returns>
        private AcceptInvoiceInstructionsViewModel InitializeAcceptInvoiceInstructionsViewModel()
        {
            return new AcceptInvoiceInstructionsViewModel
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                BackLink = ControllerNames.ClassifyRunConfirmation,
                AcceptAll = false,
            };
        }
    }
}