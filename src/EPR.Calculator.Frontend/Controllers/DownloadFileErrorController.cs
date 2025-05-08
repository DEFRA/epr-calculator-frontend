using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    public class DownloadFileErrorController : Controller
    {
        /// <summary>
        /// Displays the error page for a specific calculation run.
        /// </summary>
        /// <param name="runId">The ID of the calculation run.</param>
        /// <param name="calcName">The calcName of the calculation run.</param>
        /// <param name="createdDate">The Date of the calculation run.</param>
        /// <param name="createdTime">The Time of the calculation run.</param>
        /// <returns>A view displaying the error details.</returns>
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

        /// <summary>
        /// Displays the new error page for a specific calculation run.
        /// </summary>
        /// <param name="runId">The ID of the calculation run.</param>
        /// <param name="calcName">The calcName of the calculation run.</param>
        /// <param name="createdDate">The Date of the calculation run.</param>
        /// <param name="createdTime">The Time of the calculation run.</param>
        /// <returns>A view displaying the new error details.</returns>
        [Route("DownloadFileErrorNew/{runId}")]
        public IActionResult IndexNew(int runId, string calcName, string createdDate, string createdTime)
        {
            var commonDataModel = new ViewModelCommonData
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
            };

            return this.View(ViewNames.DownloadFileErrorIndexNew, commonDataModel);
        }
    }
}
