using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Controller responsible for handling parameter confirmation.
    /// </summary>
    [Authorize(Roles = "SASuperUser")]
    public class ParameterConfirmationController : Controller
    {
        /// <summary>
        /// Displays the parameter confirmation view.
        /// </summary>
        /// <returns>The parameter confirmation view.</returns>
        [Authorize(Roles = "SASuperUser")]
        public IActionResult Index()
        {
            // Create a view model for parameter confirmation
            var parameterConfirmationViewModel = new ConfirmationViewModel
            {
                Title = ParameterConfirmation.Title,
                Body = ParameterConfirmation.Body,
                NextText = ParameterConfirmation.NextText,
                RedirectController = ParameterConfirmation.RedirectController,
                SubmitText = ParameterConfirmation.SubmitText,
            };

            // Return the view with the view model
            return this.View(ViewNames.ParameterConfirmationIndex, parameterConfirmationViewModel);
        }
    }
}