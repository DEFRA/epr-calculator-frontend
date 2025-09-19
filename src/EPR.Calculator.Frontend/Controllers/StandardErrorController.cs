using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    public class StandardErrorController : Controller
    {
        public IActionResult Index()
        {
            var currentUser = CommonUtil.GetUserName(this.HttpContext);
            return this.View(
                ViewNames.StandardErrorIndex,
                new ViewModelCommonData
                {
                    CurrentUser = currentUser,
                    BackLinkViewModel = new BackLinkViewModel
                    {
                        BackLink = string.Empty,
                        CurrentUser = currentUser,
                    },
                });
        }
    }
}
