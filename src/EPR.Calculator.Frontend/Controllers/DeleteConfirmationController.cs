using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteConfirmationController"/> class.
    /// </summary>
    [Authorize(Roles = "SASuperUser")]
    public class DeleteConfirmationController : Controller
    {
        /// <summary>
        /// Displays the delete confirmation screen.
        /// </summary>
        /// <param name="runId">The ID of the calculation run.</param>
        /// <param name="calcName">The calculation name.</param>
        /// <returns>The delete success view.</returns>
        [Authorize(Roles = "SASuperUser")]
        [AuthorizeForScopes(Scopes = new[] { "api://542488b9-bf70-429f-bad7-1e592efce352/default" })]
        public IActionResult Index(int runId, string calcName)
        {
            var calculatorRunStatusUpdate = new CalculatorRunStatusUpdateDto
            {
                RunId = runId,
                CalcName = calcName,
                ClassificationId = (int)RunClassification.DELETED,
            };
            var statusUpdateViewModel = new CalculatorRunStatusUpdateViewModel
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                Data = calculatorRunStatusUpdate,
            };
            return this.View(ViewNames.DeleteConfirmation, statusUpdateViewModel);
        }
    }
}