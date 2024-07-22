using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View("Index");
        }
    }
}
