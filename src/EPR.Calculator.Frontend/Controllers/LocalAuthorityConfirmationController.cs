namespace EPR.Calculator.Frontend.Controllers
{
    using EPR.Calculator.Frontend.Constants;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize(Roles = "SASuperUser")]
    public class LocalAuthorityConfirmationController : Controller
    {
        public IActionResult Index()
        {
            return this.View(ViewNames.LocalAuthorityConfirmationIndex);
        }
    }
}