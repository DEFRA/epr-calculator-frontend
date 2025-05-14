using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    [AllowAnonymous]
    public class BlazoreGridController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
