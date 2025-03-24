using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Standard Error Controller
    /// </summary>
    [Authorize(Roles = "SASuperUser")]
    public class StandardErrorController : Controller
    {
        [Authorize(Roles = "SASuperUser")]
        public IActionResult Index()
        {
            return this.View(
                ViewNames.StandardErrorIndex,
                new ViewModelCommonData
                {
                    CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                });
        }
    }
}
