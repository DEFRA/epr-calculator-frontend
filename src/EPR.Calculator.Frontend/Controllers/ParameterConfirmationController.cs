using EPR.Calculator.Frontend.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    [Authorize(Roles = "SASuperUser")]
    public class ParameterConfirmationController : Controller
    {
        public IActionResult Index()
        {
            return this.View(ViewNames.ParameterConfirmationIndex);
        }
    }
}
