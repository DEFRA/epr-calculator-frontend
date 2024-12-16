using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    [Authorize(Roles = "SASuperUser")]
    public class DownloadFileErrorController : Controller
    {
        [Authorize(Roles = "SASuperUser")]
        public IActionResult Index()
        {
            return this.View(
                ViewNames.DownloadFileErrorIndex,
                new ViewModelCommonData
                {
                    CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                });
        }
    }
}
