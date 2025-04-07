using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Controller responsible for classifying calculation runs.
    /// </summary>
    public class ClassifyCalculationRunController : Controller
    {
        /// <summary>
        /// Displays the classification view for a calculation run.
        /// </summary>
        /// <returns>The classification view with the calculation run details.</returns>
        public IActionResult Index()
        {
            // Create a view model with the details of the calculation run.
            ClassifyCalculationViewModel classifyCalculationViewModel = new ClassifyCalculationViewModel("Calculation run 99", "2024-25", 240008, true, "1 December 2024 at 12:09")
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
            };

            // Return the view with the view model.
            return this.View(ViewNames.CalculationRunClassification, classifyCalculationViewModel);
        }
    }
}