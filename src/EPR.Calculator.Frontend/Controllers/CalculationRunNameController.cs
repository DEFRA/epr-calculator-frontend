using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    public class CalculationRunNameController : Controller
    {
        private const string CalculationRunNameIndexView = ViewNames.CalculationRunNameIndex;
        private const string CalculationRunConfirmationAction = "Index";
        private const string CalculationRunConfirmationController = "CalculationRunConfirmation";

        private readonly ILogger<CalculationRunNameController> _logger;

        public CalculationRunNameController(ILogger<CalculationRunNameController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return this.View(CalculationRunNameIndexView);
        }

        [HttpPost]
        public IActionResult RunCalculator(string calculationName)
        {   
            if (string.IsNullOrEmpty(calculationName))
            {
                this.ViewBag.Errors = CreateErrorViewModel();
                return this.View(CalculationRunNameIndexView);
            }
            throw new SystemException("SUNITA ERROR");
            //return this.RedirectToAction(CalculationRunConfirmationAction, CalculationRunConfirmationController);
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
