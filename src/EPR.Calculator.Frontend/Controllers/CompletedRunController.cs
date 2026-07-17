using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

[Route("[controller]")]
public class CompletedRunController(IEprCalculatorApiService eprCalculatorApiService)
    : BaseController
{
    /// <summary>
    ///     Displays the calculation run post billing file details index view.
    /// </summary>
    /// <param name="runId">The ID of the calculation run.</param>
    /// <returns>The post billing file index view.</returns>
    [HttpGet]
    [Route("{runId:int}")]
    public async Task<IActionResult> Index(int runId)
    {
        var viewModel = await CreateViewModel(runId);

        if (viewModel == null || !IsRunEligibleForDisplay(viewModel.CalculatorRunStatus))
            return RedirectToError();

        return View(ViewNames.PostBillingFileIndex, viewModel);
    }

    private static bool IsRunEligibleForDisplay(CalculatorRunDto calculatorRunDetails)
    {
        return calculatorRunDetails.RunClassification
            is RunClassification.UNCLASSIFIED
            or RunClassification.INITIAL_RUN
            or RunClassification.INTERIM_RECALCULATION_RUN
            or RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED
            or RunClassification.FINAL_RECALCULATION_RUN
            or RunClassification.FINAL_RECALCULATION_RUN_COMPLETED
            or RunClassification.FINAL_RUN
            or RunClassification.FINAL_RUN_COMPLETED
            or RunClassification.INITIAL_RUN_COMPLETED;
    }

    private async Task<PostBillingFileViewModel?> CreateViewModel(int runId)
    {
        var run = await eprCalculatorApiService.GetCalculatorRun(runId);

        if (run == null)
            return null;

        return new PostBillingFileViewModel
        {
            CalculatorRunStatus = run
        };
    }
}
