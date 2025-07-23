using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using EPR.Calculator.Frontend.ViewModels.Enums;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Controller responsible for displaying the details of a calculation run.
    /// </summary>
    [Route("[controller]")]
    public class CalculationRunDetailsNewController : BaseController
    {
        private readonly IConfiguration _configuration;

        public CalculationRunDetailsNewController(
            IConfiguration configuration,
            IHttpClientFactory clientFactory,
            ITokenAcquisition tokenAcquisition,
            TelemetryClient telemetryClient)
            : base(configuration, tokenAcquisition, telemetryClient, clientFactory)
        {
            _configuration = configuration;
        }

        [Route("{runId}")]
        public async Task<IActionResult> Index(int runId)
        {
            var viewModel = await this.CreateViewModel(runId);
            viewModel.BackLinkViewModel = new BackLinkViewModel()
            {
                BackLink = string.Empty,
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
            };
            if (viewModel.CalculatorRunDetails == null || viewModel.CalculatorRunDetails.RunId <= 0)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }

            if (viewModel.CalculatorRunDetails.RunClassificationId == RunClassification.ERROR)
            {
                this.ModelState.AddModelError(viewModel.CalculatorRunDetails.RunName!, ErrorMessages.RunDetailError);
                return this.View(ViewNames.CalculationRunDetailsNewErrorPage, viewModel);
            }

            return this.View(ViewNames.CalculationRunDetailsNewIndex, viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(CalculatorRunDetailsNewViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                var viewModel = await this.CreateViewModel(model.CalculatorRunDetails.RunId);
                viewModel.BackLinkViewModel = new BackLinkViewModel()
                {
                    BackLink = ControllerNames.ClassifyingCalculationRun,
                    CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                };
                return this.View(ViewNames.CalculationRunDetailsNewIndex, viewModel);
            }

            return model.SelectedCalcRunOption switch
            {
                CalculationRunOption.OutputClassify => this.RedirectToAction(ActionNames.Index, ControllerNames.ClassifyingCalculationRun, new { model.CalculatorRunDetails.RunId }),
                CalculationRunOption.OutputDelete => this.RedirectToAction(ActionNames.Index, ControllerNames.CalculationRunDelete, new { model.CalculatorRunDetails.RunId }),
                _ => this.RedirectToAction(ActionNames.Index, new { model.CalculatorRunDetails.RunId }),
            };
        }

        private async Task<CalculatorRunDetailsNewViewModel> CreateViewModel(int runId)
        {
            var viewModel = new CalculatorRunDetailsNewViewModel()
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                CalculatorRunDetails = new CalculatorRunDetailsViewModel(),
            };

            var runDetails = await this.GetCalculatorRundetails(runId);
            if (runDetails != null && runDetails!.RunId > 0)
            {
                viewModel.CalculatorRunDetails = runDetails;
                this.SetDownloadParameters(viewModel);
            }

            return viewModel;
        }

        private void SetDownloadParameters(CalculatorRunDetailsNewViewModel viewModel)
        {
            var baseApiUrl = this._configuration.GetValue<string>($"{ConfigSection.CalculationRunSettings}:{ConfigSection.DownloadResultApi}");
            viewModel.DownloadResultURL = new Uri($"{baseApiUrl}/{viewModel.CalculatorRunDetails.RunId}");

            viewModel.DownloadErrorURL = $"/DownloadFileErrorNew/{viewModel.CalculatorRunDetails.RunId}";
            viewModel.DownloadTimeout = this._configuration.GetValue<int>($"{ConfigSection.CalculationRunSettings}:{ConfigSection.DownloadResultTimeoutInMilliSeconds}");
        }
    }
}