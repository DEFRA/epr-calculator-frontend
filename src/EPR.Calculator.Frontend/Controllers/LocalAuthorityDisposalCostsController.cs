using EPR.Calculator.Frontend.Constants;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    public class LocalAuthorityDisposalCostsController : Controller
    {
        public IActionResult Index()
        {
            return View(ViewNames.LocalAuthorityDisposalCostsIndex);
        }
    }
}
