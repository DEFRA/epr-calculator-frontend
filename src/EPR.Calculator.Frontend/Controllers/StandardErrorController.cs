using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

public class StandardErrorController : BaseController
{
    public IActionResult Index()
    {
        return View();
    }
}
