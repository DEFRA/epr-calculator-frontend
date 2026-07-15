using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

public class StandardErrorController : BaseController
{
    public IActionResult Index()
    {
        return View(ViewNames.StandardErrorIndex);
    }
}
