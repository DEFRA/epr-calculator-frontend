using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace EPR.Calculator.Frontend.Controllers
{
    public partial class CalculationRunNameController : Controller
    {
        private const string CalculationRunNameIndexView = ViewNames.CalculationRunNameIndex;

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

            this.ViewBag.CalculationName = calculationName;
            this.HttpContext.Session.SetString(SessionConstants.CalculationName, (string)this.ViewBag.CalculationName);

            return this.RedirectToAction(ActionNames.RunCalculatorConfirmation);
        }

        public ViewResult Confirmation()
        {
            return this.View(ViewNames.CalculationRunConfirmation);
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

            if (!AllowOnlyAlphaNumeric().IsMatch(calculationName))
            {
                return ValidationResult.Fail(ErrorMessages.CalculationRunNameMustBeAlphaNumeric);
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

        [GeneratedRegex(@"^[a-zA-Z0-9]{1,100}$")]
        private static partial Regex AllowOnlyAlphaNumeric();
    }
}