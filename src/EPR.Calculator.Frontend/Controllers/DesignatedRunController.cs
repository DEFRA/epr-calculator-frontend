using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

[Route("[controller]")]
public class DesignatedRunController(
    ICalculatorRunDetailsService calculatorRunDetailsService)
    : BaseController
{
    [HttpGet("{runId}")]
    public async Task<IActionResult> Index(int runId)
    {
        var viewModel = await CreateViewModel(runId);

        if (viewModel.CalculatorRunDetails == null || viewModel.CalculatorRunDetails.RunId == 0 || !IsRunEligibleForDisplay(viewModel.CalculatorRunDetails))
            return RedirectToError();

        return View(ViewNames.ClassifyRunConfirmationIndex, viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Submit(int runId)
    {
        if (!ModelState.IsValid)
            return RedirectToAction(ActionNames.Index, new { runId });

        return RedirectToRoute(RouteNames.BillingInstructionsIndex, new { runId });
    }

    private static bool IsRunEligibleForDisplay(CalculatorRunDetailsViewModel calculatorRunDetails)
    {
        return calculatorRunDetails.RunClassificationId == RunClassification.INITIAL_RUN
               ||
               calculatorRunDetails.RunClassificationId == RunClassification.INTERIM_RECALCULATION_RUN
               ||
               calculatorRunDetails.RunClassificationId == RunClassification.FINAL_RUN
               ||
               calculatorRunDetails.RunClassificationId == RunClassification.FINAL_RECALCULATION_RUN
               ||
               calculatorRunDetails.RunClassificationId == RunClassification.TEST_RUN;
    }

    private async Task<ClassifyRunConfirmationViewModel> CreateViewModel(int runId)
    {
        var currentUser = CommonUtil.GetUserName(HttpContext);

        var viewModel = new ClassifyRunConfirmationViewModel
        {
            CurrentUser = currentUser,
            CalculatorRunDetails = new CalculatorRunDetailsViewModel(),
            BackLinkViewModel = new BackLinkViewModel
            {
                BackLink = string.Empty,
                CurrentUser = currentUser,
                HideBackLink = GetBackLink() != ControllerNames.Dashboard
            }
        };

        var runDetails = await calculatorRunDetailsService.GetCalculatorRundetailsAsync(
            HttpContext,
            runId);
        if (runDetails != null && runDetails!.RunId != 0)
            viewModel.CalculatorRunDetails = runDetails;

        return viewModel;
    }
}
