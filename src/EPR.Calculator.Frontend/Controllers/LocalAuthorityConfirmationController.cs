namespace EPR.Calculator.Frontend.Controllers
{
    using EPR.Calculator.Frontend.Constants;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Identity.Web;

    [Authorize(Roles = "SASuperUser")]
    public class LocalAuthorityConfirmationController : Controller
    {
        [Authorize(Roles = "SASuperUser")]
        [AuthorizeForScopes(Scopes = new[] { "api://542488b9-bf70-429f-bad7-1e592efce352/default" })]
        public IActionResult Index()
        {
            return this.View(ViewNames.LocalAuthorityConfirmationIndex);
        }
    }
}