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
        /// <param name="calcName">The calculation name.</param>
        /// <returns>The delete success view.</returns>
        public IActionResult Index(int runId, string calcName)
        {
            var calculatorRunStatusUpdate = new CalculatorRunStatusUpdateDto
            {
                RunId = runId,
                CalcName = calcName,
                ClassificationId = (int)RunClassification.DELETED,
            };
            return this.View(ViewNames.DeleteConfirmation, calculatorRunStatusUpdate);
        }
    }
}