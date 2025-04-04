using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Controller responsible for payments.
    /// </summary>
    [Authorize(Roles = "SASuperUser")]
    [Route("PaymentCalculator")]
    public class PaymentCalculatorController : Controller
    {
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
                    RedirectController =CommonConstants.DashBoard,
                }
            };

            // Return the view
            return this.View(ViewNames.BillingConfirmationSuccess, model);
        }
    }
}