using EPR.Calculator.Frontend.Constants;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    public class CalculationRunDetailsController : Controller
    {
        public IActionResult Index()
        {
            return this.View(ViewNames.CalculationRunDetailsIndex);
        }

        public IActionResult Error()
        {
            return this.View(ViewNames.CalculationRunDetailsErrorPage);
        }
    }
}
