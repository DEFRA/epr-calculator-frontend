using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteConfirmationController"/> class.
    /// </summary>
    public class DeleteConfirmationController : Controller
    {
        /// <summary>
        /// Displays the delete confirmation screen.
        /// </summary>
        /// <param name="runId">The ID of the calculation run.</param>
        /// <returns>The delete success view.</returns>
        public IActionResult Index(int runId)
        {
            var calculatorRunStatusUpdate = new CalculatorRunStatusUpdateDto
            {
                RunId = runId,
                ClassificationId = (int)RunClassification.DELETED,
            };
            return this.View(ViewNames.DeleteConfirmation, calculatorRunStatusUpdate);
        }
    }
}
