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
        [Route("AcceptInvoiceInstructions")]
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
        [Route("AcceptInvoiceInstructions")]
        [ValidateAntiForgeryToken]
        public IActionResult AcceptInvoiceInstructions(AcceptInvoiceInstructionsViewModel model)
        {
            if (model.AcceptAll)
            {
                return this.RedirectToAction("Overview"); // dummy return url I confirm that I have accepted all billing instructions in the results file.
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
        public IActionResult BillingFileSuccess()
        {
            // Create the view model
            var model = new BillingFileSuccessViewModel
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                ConfirmationViewModel = new ConfirmationViewModel
                {
                    Title = ConfirmationMessages.BillingFileSuccessTitle,
                    Body = ConfirmationMessages.BillingFileSuccessBody,
                    AdditionalParagraphs = ConfirmationMessages.BillingFileSuccessAdditionalParagraphs,
                    RedirectController = CommonConstants.DashBoard,
                }
            };

            // Return the view
            return this.View(ViewNames.BillingConfirmationSuccess, model);
        }
    }
}