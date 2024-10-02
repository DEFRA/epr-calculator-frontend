using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    public class CalculationRunNameController : Controller
    {
        private const string CalculationRunNameIndexView = ViewNames.CalculationRunNameIndex;

        public IActionResult Index()
        {
            return this.View(CalculationRunNameIndexView);
        }

        [HttpPost]
        public IActionResult RunCalculator(string calculationName)
        {
            if (string.IsNullOrWhiteSpace(calculationName))
            {
                this.ViewBag.Errors = CreateErrorViewModel();
                return this.View(CalculationRunNameIndexView);
            }

            this.ViewBag.CalculationName = calculationName;
            this.HttpContext.Session.SetString(SessionConstants.CalculationName, (string)this.ViewBag.CalculationName);

            return this.RedirectToAction(ActionNames.RunCalculatorConfirmation);
        }

        public ViewResult Confirmation()
        {
            return this.View(ViewNames.CalculationRunConfirmation);
        }

        private static ErrorViewModel CreateErrorViewModel()
        {
            return new ErrorViewModel
            {
                DOMElementId = ViewControlNames.CalculationRunName,
                ErrorMessage = ErrorMessages.CalculationRunNameEmpty,
            };
        }
    }
}