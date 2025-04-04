using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentCalculatorController"/> class.
    /// </summary>
    [Authorize(Roles = "SASuperUser")]
    [Route("PaymentCalculator")]
    public class PaymentCalculatorController : Controller
    {
        [HttpGet]
        [Route("accept-invoice-instructions")]
        public IActionResult AcceptInvoiceInstructions()
        {
            var model = new AcceptInvoiceInstructionsViewModel
            {
                AcceptAll = false,
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                CalculationRunTitle = "Calculation run 99",
            };

            return this.View(model);
        }

        [HttpPost]
        [Route("accept-invoice-instructions")]
        [ValidateAntiForgeryToken]
        public IActionResult AcceptInvoiceInstructions(AcceptInvoiceInstructionsViewModel model)
        {
            if (model.AcceptAll)
            {
                return this.RedirectToAction("Overview"); // dummy return url
            }

            this.ModelState.AddModelError("AcceptAll", "You must confirm acceptance to proceed.");
            return this.View(model);
        }

        /// <summary>
        /// Displays a billing file sent confirmation screen.
        /// </summary>
        /// <returns>Billing file sent page.</returns>
        [Route("BillingFileSuccess")]
        public IActionResult BillingFileSuccess()
        {
            // Create the view model
            var model = new BillingFileSuccessViewModel
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                ConfirmationViewModel = new ConfirmationViewModel
                {
                    Title = ConfirmationMessages.BillingFileSuccess.Title,
                    Body = ConfirmationMessages.BillingFileSuccess.Body,
                    AdditionalParagraphs = ConfirmationMessages.BillingFileSuccess.NextText,
                    RedirectController = CommonConstants.DashBoard,
                }
            };

            // Return the view
            return this.View(ViewNames.BillingConfirmationSuccess, model);
        }
    }
}