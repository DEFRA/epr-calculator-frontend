using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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
