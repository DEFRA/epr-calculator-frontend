using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

[Route("[controller]")]
public class DesignatedRunController(IEprCalculatorApiService eprCalculatorApiService)
    : BaseController
{
    [HttpGet("{runId:int}")]
    public async Task<IActionResult> Index(int runId)
    {
        var viewModel = await CreateViewModel(runId);

        if (viewModel == null || !IsRunEligibleForDisplay(viewModel.CalculatorRunDetails))
            return RedirectToError();

        return View(ViewNames.ClassifyRunConfirmationIndex, viewModel);
    }

    private static bool IsRunEligibleForDisplay(CalculatorRunDto runDto)
    {
        return runDto.RunClassification
            is RunClassification.INITIAL_RUN
            or RunClassification.INTERIM_RECALCULATION_RUN
            or RunClassification.FINAL_RUN
            or RunClassification.FINAL_RECALCULATION_RUN
            or RunClassification.TEST_RUN;
    }

    private async Task<ClassifyRunConfirmationViewModel?> CreateViewModel(int runId)
    {
        var runDto = await eprCalculatorApiService.GetCalculatorRun(runId);

        if (runDto == null)
            return null;

        return new ClassifyRunConfirmationViewModel
        {
            CalculatorRunDetails = runDto
        };
    }
}
