using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
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
        public IActionResult RunCalculator(InitiateCalculatorRunModel calculationRunName)
        {
            if (!ModelState.IsValid)
            {
                this.ViewBag.Errors = CreateErrorViewModel(ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).LastOrDefault());
                return this.View(CalculationRunNameIndexView);
            }

            this.ViewBag.CalculationName = calculationRunName?.CalculationName;
            if (!string.IsNullOrEmpty(this.ViewBag.CalculationName))
            {
                this.HttpContext.Session.SetString(SessionConstants.CalculationName, (string)this.ViewBag.CalculationName);
            }

            return this.RedirectToAction(ActionNames.RunCalculatorConfirmation);
        }

        public ViewResult Confirmation()
        {
            return this.View(ViewNames.CalculationRunConfirmation);
        }

        private static ErrorViewModel CreateErrorViewModel(string errorMessage)
        {
            return new ErrorViewModel
            {
                DOMElementId = ViewControlNames.CalculationRunName,
                ErrorMessage = errorMessage,
            };
        }
    }
}