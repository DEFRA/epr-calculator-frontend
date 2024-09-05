namespace EPR.Calculator.Frontend.Controllers
{
    using EPR.Calculator.Frontend.Constants;
    using Microsoft.AspNetCore.Mvc;

    public class LocalAuthorityConfirmationController : Controller
    {
        public IActionResult Index()
        {
            return this.View(ViewNames.LocalAuthorityConfirmationIndex);
        }
    }
}