using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EPR.Calculator.Frontend.Controllers
{
    public class SendBillingFileController : BaseController
    {
        private readonly IConfiguration configuration;

        public SendBillingFileController(IConfiguration configuration, ITokenAcquisition tokenAcquisition, TelemetryClient telemetryClient)
            : base(configuration, tokenAcquisition, telemetryClient)
        {
            this.configuration = configuration;
        }

        public IActionResult Index()
        {
            var billingFileViewModel = new SendBillingFileViewModel()
            {
                CalcRunName = "Calculation run 99",
                ConfirmationContent = CommonConstants.ConfirmationContent,
                SendBillFileHeading = CommonConstants.SendBillingFile,
                WarningContent = CommonConstants.WarningContent,
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
            };

            return this.View(billingFileViewModel);
        }
    }
}
