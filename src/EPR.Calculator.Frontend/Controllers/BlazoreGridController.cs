using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.Controllers
{
    [AllowAnonymous]
    public class BlazoreGridController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("blazoreGrid/submit")]
        public IActionResult Submit([FromBody] List<OrgProducerData> model)
        {
            return Json(new { message = "Success" });
        }
    }
}
