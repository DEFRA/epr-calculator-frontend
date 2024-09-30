using EPR.Calculator.Frontend.Constants;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    public class CalculationRunNameController : Controller
    {
        public IActionResult Index()
        {
            return this.View(ViewNames.CalculationRunNameIndex);
        }

        public IActionResult CalculateRun(string? calculationName)
        {
            this.ViewBag.CalculationName = calculationName;
            this.HttpContext.Session.SetString("CalculationName", calculationName ?? this.HttpContext.Session.GetString("CalculationName"));
            return this.View(ViewNames.CalculationRunConfirmationIndex);
        }
    }
}