using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Services;
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
    public class CalculationRunDetailsNewController(IConfiguration configuration,
           IApiService apiService,
           ITokenAcquisition tokenAcquisition,
           TelemetryClient telemetryClient,
           ICalculatorRunDetailsService calculatorRunDetailsService) : BaseController(configuration,
                 tokenAcquisition,
                 telemetryClient,
                 apiService,
                 calculatorRunDetailsService)
    {
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
                    RunId = model.CalculatorRunDetails.RunId,
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

            var runDetails = await this.CalculatorRunDetailsService.GetCalculatorRundetailsAsync(
                this.HttpContext,
                runId);
            if (runDetails != null && runDetails!.RunId > 0)
            {
                viewModel.CalculatorRunDetails = runDetails;
            }

            return viewModel;
        }
    }
}