using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using EPR.Calculator.Frontend.ViewModels.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EPR.Calculator.Frontend.Controllers;

[Route("[controller]")]
public class CalculationRunDetailsNewController(IEprCalculatorApiService eprCalculatorApiService)
    : BaseController
{
    [Route("{runId:int}")]
    public async Task<IActionResult> Index(int runId)
    {
        var viewModel = await CreateViewModel(runId);

        if (viewModel == null)
            return RedirectToError();

        if (viewModel.RunClassification == RunClassification.ERROR)
        {
            ModelState.AddModelError(viewModel.RunName, ErrorMessages.RunDetailError);
            return View(ViewNames.CalculationRunDetailsNewErrorPage, viewModel);
        }

        return View(ViewNames.CalculationRunDetailsNewIndex, viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(CalculatorRunDetailsNewFormModel model)
    {
        if (!ModelState.IsValid)
        {
            if (ModelState[nameof(model.RunId)] is { ValidationState: ModelValidationState.Invalid })
                return RedirectToError();

            return await Index(model.RunId);
        }

        return model.SelectedCalcRunOption switch
        {
            CalculationRunOption.OutputClassify => RedirectToAction(ActionNames.Index, ControllerNames.ClassifyingCalculationRun, new { model.RunId }),
            CalculationRunOption.OutputDelete => RedirectToAction(ActionNames.Index, ControllerNames.CalculationRunDelete, new { model.RunId }),
            _ => RedirectToAction(ActionNames.Index, new { model.RunId })
        };
    }

    private async Task<CalculatorRunDetailsNewViewModel?> CreateViewModel(int runId)
    {
        var run = await eprCalculatorApiService.GetCalculatorRun(runId);

        if (run == null)
            return null;

        return new CalculatorRunDetailsNewViewModel
        {
            RunId = run.RunId,
            RunName = run.RunName,
            RunClassification = run.RunClassification,
            RelativeYear = run.RelativeYear,
            CreatedAt = run.CreatedAt,
            CreatedBy = run.CreatedBy
        };
    }
}
