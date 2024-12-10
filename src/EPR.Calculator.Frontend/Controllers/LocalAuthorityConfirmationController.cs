namespace EPR.Calculator.Frontend.Controllers
{
    using EPR.Calculator.Frontend.Constants;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize(Roles = "SASuperUser")]
    public class LocalAuthorityConfirmationController : Controller
    {
        [Authorize(Roles = "SASuperUser")]
        public IActionResult Index()
        {
            return this.View(ViewNames.LocalAuthorityConfirmationIndex);
        }
    }
}