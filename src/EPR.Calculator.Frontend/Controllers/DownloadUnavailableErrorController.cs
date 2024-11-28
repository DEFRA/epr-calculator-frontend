using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    public class DownloadUnavailableErrorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
