using Microsoft.AspNetCore.Mvc;
using EPR.Calculator.Frontend.Constants;

namespace EPR.Calculator.Frontend.Controllers
{
    public class StandardErrorController : Controller
    {
        public IActionResult Index()
        {
            return View(ViewNames.StandardErrorIndex);
        }
    }
}
