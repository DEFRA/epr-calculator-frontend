using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteConfirmationController"/> class.
    /// </summary>
    [Route("[controller]")]
    public class DeleteConfirmationController(
        IConfiguration configuration,
        ITokenAcquisition tokenAcquisition,
        TelemetryClient telemetryClient,
        IApiService apiService,
        ICalculatorRunDetailsService calculatorRunDetailsService)
        : BaseController(
            configuration,
            tokenAcquisition,
            telemetryClient,
            apiService,
            calculatorRunDetailsService)
    {
        [Authorize(Roles = "SASuperUser")]
        public IActionResult Index(int runId, string calcName)
        {
            var calculatorRunStatusUpdate = new CalculatorRunStatusUpdateDto
            {
                RunId = runId,
                CalcName = calcName,
                ClassificationId = (int)RunClassification.DELETED,
            };
            var statusUpdateViewModel = new CalculatorRunStatusUpdateViewModel
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                Data = calculatorRunStatusUpdate,
            };
            return this.View(ViewNames.DeleteConfirmation, statusUpdateViewModel);
        }
    }
}