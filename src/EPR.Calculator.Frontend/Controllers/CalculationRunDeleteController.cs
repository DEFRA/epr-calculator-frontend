using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CalculationRunDeleteController"/> class.
    /// </summary>
    [Authorize(Roles = "SASuperUser")]
    public class CalculationRunDeleteController : Controller
    {
        /// <summary>
        /// Displays the calculate run delete confirmation screen.
        /// </summary>
        /// <param name="runId">The ID of the calculation run.</param>
        /// <returns>The delete confirmation view.</returns>
        public IActionResult Index(int runId)
        {
            var calculatorRunStatusUpdate = new CalculatorRunStatusUpdateDto
            {
                RunId = runId,
                CalcName = "Calculation Run Name",
                ClassificationId = (int)RunClassification.DELETED,
            };
            var calculationRunDeleteViewModel = new CalculationRunDeleteViewModel
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                Data = calculatorRunStatusUpdate,
            };
            return this.View(ViewNames.CalculationRunDeleteIndex, calculationRunDeleteViewModel);
        }
    }
}