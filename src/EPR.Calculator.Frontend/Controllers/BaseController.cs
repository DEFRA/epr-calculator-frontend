using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

public class BaseController : Controller
{
    protected RedirectToActionResult RedirectToError()
    {
        return RedirectToAction("Index", "StandardError");
    }
}
