using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Controller responsible for payments.
    /// </summary>
    [Authorize(Roles = "SASuperUser")]
    [Route("payment-calculator")]
    public class PaymentCalculatorController : Controller
    {
        /// <summary>
        /// Displays a billing file sent confirmation screen.
        /// </summary>
        /// <returns>Billing file sent page.</returns>
        [Route("billing-file-success")]
        public IActionResult BillingFileSuccess()
        {
            // Create the view model
            var model = new ConfirmationViewModel
            {
                Title = ConfirmationMessages.BillingFileSuccess.Title,
                Body = ConfirmationMessages.BillingFileSuccess.Body,
                NextText = ConfirmationMessages.BillingFileSuccess.NextText,
                RedirectController = "Dashboard",
            };

            // Return the view
            return this.View(ViewNames.BillingConfirmationSuccess, model);
        }
    }
}