using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EPR.Calculator.Frontend.Controllers
{
    [Authorize(Roles = "SASuperUser")]
    public class DownloadFileErrorController : Controller
    {
        /// <param name="runId">The ID of the calculation run.</param>
        /// <param name="calcName">The calcName of the calculation run.</param>
        /// <param name="createdDate">The Date of the calculation run.</param>
        /// <param name="createdTime">The Time of the calculation run.</param>
        [Authorize(Roles = "SASuperUser")]
        [Route("DownloadFileError/{runId}")]
        public IActionResult Index(int runId, string calcName, string createdDate, string createdTime)
        {
            var statusUpdateViewModel = new CalculatorRunStatusUpdateViewModel
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                Data = new CalculatorRunStatusUpdateDto
                {
                    RunId = runId,
                    ClassificationId = (int)RunClassification.DELETED,
                    CalcName = calcName,
                    CreatedDate = createdDate,
                    CreatedTime = createdTime,
                },
            };
            return this.View(ViewNames.DownloadFileErrorIndex, statusUpdateViewModel);
        }
    }
}
