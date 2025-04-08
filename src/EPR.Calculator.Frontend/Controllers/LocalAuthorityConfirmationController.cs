namespace EPR.Calculator.Frontend.Controllers
{
    using EPR.Calculator.Frontend.Constants;
    using EPR.Calculator.Frontend.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Controller responsible for handling Local Authority parameter confirmation.
    /// </summary>
    [Authorize(Roles = "SASuperUser")]
    public class LocalAuthorityConfirmationController : Controller
    {
        /// <summary>
        /// Displays the Local Authority parameter confirmation view.
        /// </summary>
        /// <returns>The Local Authority parameter confirmation view.</returns>
        [Authorize(Roles = "SASuperUser")]
        public IActionResult Index()
        {
            // Create a view model for parameter confirmation
            var parameterConfirmationViewModel = new ConfirmationViewModel
            {
                Title = ParameterConfirmation.Title,
                Body = ParameterConfirmation.Body,
                RedirectController = ParameterConfirmation.RedirectController,
                SubmitText = ParameterConfirmation.SubmitText,
                AdditionalParagraphs = LocalAuthorityConfirmation.AdditionalParagraphs,
            };

            // Return the view with the view model
            return this.View(ViewNames.LocalAuthorityConfirmationIndex);
        }
    }
}