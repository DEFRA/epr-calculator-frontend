using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EPR.Calculator.Frontend.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public DashboardController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View("Index");
        }
    }
}
