using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    public class PostBillingFileController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var billingFileViewModel = new PostBillingFileViewModel
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                BillingFileSentDate = "17 March 2025",
                BillingFileRunBy = "Steve Jones",
                BillingFileSentBy = "Steve Jones",
                SelectedCalcRunOption = "Initial run",
                CalculatorRunStatus = new CalculatorRunStatusUpdateDto
                {
                    RunId = 240008,
                    ClassificationId = 3,
                    CalcName = "Calculation run 99",
                    CreatedDate = "01 May 2024",
                    CreatedTime = "12:09",
                    FinancialYear = "2024-25",
                },
            };

            return this.View(billingFileViewModel);
        }
    }
}
