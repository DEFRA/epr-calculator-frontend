using EPR.Calculator.Frontend.Constants;
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
            // Return the view
            return this.View(ViewNames.BillingConfirmationSuccess);
        }
    }
}