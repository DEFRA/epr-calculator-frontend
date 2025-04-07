using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentCalculatorController"/> class.
    /// </summary>
    [Authorize(Roles = "SASuperUser")]
    [Route("[controller]")]
    public class PaymentCalculatorController : Controller
    {
        [HttpGet]
        [Route("paymentcalculator/{runId}")]
        public IActionResult Index(int runId)
        {
            var model = new AcceptInvoiceInstructionsViewModel
            {
                RunId = 99,
                AcceptAll = false,
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                CalculationRunTitle = "Calculation run 99",
            };

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AcceptInvoiceInstructions(int runId)
        {
            return RedirectToAction("Index", "CalculationRunOverview", new { runId = runId });
        }
    }
}
