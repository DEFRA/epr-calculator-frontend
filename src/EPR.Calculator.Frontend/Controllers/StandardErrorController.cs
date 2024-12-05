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
            return this.View(
                ViewNames.StandardErrorIndex,
                new ViewModelCommonData
                {
                    CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                });
        }
    }
}
