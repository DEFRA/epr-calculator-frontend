using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    public class DefaultParametersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
