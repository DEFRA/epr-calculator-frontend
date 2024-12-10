using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Controller for handling calculation run errors.
    /// </summary>
    [Authorize(Roles = "SASuperUser")]
    public class CalculationRunErrorController : Controller
    {
        /// <summary>
        /// Handles the Index action for the controller.
        /// </summary>
        /// <param name="error">error message</param>
        /// <returns>CalcularionRunErrorIndex</returns>
        /// An <see cref="IActionResult"/> that renders the Calculation Run Error Index view with the error message redirects to the Calculation Run Error view.
        public IActionResult Index(ErrorDto error)
        {
            return this.View(
                ViewNames.CalculationRunErrorIndex,
                new CalculationRunErrorViewModel
                {
                    CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                    ErrorMessage = error.Message,
                });
        }
    }
}
