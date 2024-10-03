using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    public partial class CalculationRunNameController : Controller
    {
        private const string CalculationRunNameIndexView = ViewNames.CalculationRunNameIndex;
        private const string CalculationRunConfirmationAction = "Index";
        private const string CalculationRunConfirmationController = "CalculationRunConfirmation";

        public IActionResult Index()
        {
            return this.View(CalculationRunNameIndexView);
        }

        [HttpPost]
        public IActionResult RunCalculator(string calculationName)
        {
            var validationResult = ValidateCalculationRunName(calculationName);

            if (!validationResult.IsValid)
            {
                this.ViewBag.Errors = CreateErrorViewModel(validationResult.ErrorMessage);
                return this.View(CalculationRunNameIndexView);
            }

            return this.RedirectToAction(CalculationRunConfirmationAction, CalculationRunConfirmationController);
        }

        private static ValidationResult ValidateCalculationRunName(string calculationName)
        {
            if (string.IsNullOrEmpty(calculationName))
            {
                return ValidationResult.Fail(ErrorMessages.CalculationRunNameEmpty);
            }

            if (calculationName.Length > 100)
            {
                return ValidationResult.Fail(ErrorMessages.CalculationRunNameMaxLengthExceeded);
            }

            return ValidationResult.Success();
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
