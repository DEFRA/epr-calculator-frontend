using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Controller for handling calculation run errors.
    /// </summary>
    public class CalculationRunErrorController : Controller
    {
        /// <summary>
        /// Handles the Index action for the controller.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> that renders the Calculation Run Error Index view with the error message redirects to the Calculation Run Error view.
        /// </returns>
        public IActionResult Index()
        {
            var errorMessage = this.TempData["ErrorMessage"] as string;
            return this.View(model: errorMessage);
        }
    }
}
