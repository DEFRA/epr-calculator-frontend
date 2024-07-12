using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    public class DashboardControllers : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
