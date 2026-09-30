using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EPR.Calculator.Frontend.Controllers;

[Route("[controller]")]
public class SetRunClassificationController(
    IEprCalculatorApiService eprCalculatorApiService,
    ILogger<SetRunClassificationController> logger)
    : BaseController
{
    [Route("{runId:int}")]
    [HttpGet]
    public async Task<IActionResult> Index(int runId)
    {
        var viewModel = await CreateViewModel(runId);

        if (viewModel == null)
            return RedirectToError();

        return View(ViewNames.SetRunClassificationIndex, viewModel);
    }

    [Route("Submit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(SetRunClassificationFormModel model)
    {
        if (!ModelState.IsValid)
        {
            if (ModelState[nameof(model.RunId)] is { ValidationState: ModelValidationState.Invalid })
                return RedirectToError();

            return await Index(model.RunId);
        }

        var result = await eprCalculatorApiService.CallApi(
            HttpMethod.Put,
            "v2/calculatorRuns",
            body: new ClassificationDto
            {
                RunId = model.RunId,
                Classification = model.ClassifyRunType!.Value
            });

        if (!result.IsSuccessStatusCode)
        {
            logger.LogError( "API did not return successful ({ResultStatusCode})", result.StatusCode);
            return RedirectToError();
        }

        return RedirectToAction(ActionNames.Index, ControllerNames.ClassifyRunConfirmation, new { model.RunId });
    }

    private async Task<SetRunClassificationViewModel?> CreateViewModel(int runId)
    {
        var run = await eprCalculatorApiService.GetCalculatorRun(runId);

        if (run == null)
            return null;

        var response = await eprCalculatorApiService.Get<RelativeYearClassificationResponseDto>(
            "v1/ClassificationByRelativeYear",
            new Dictionary<string, string?>
            {
                ["RunId"] = run.RunId.ToString(),
                ["RelativeYearValue"] = run.RelativeYear.ToString()
            });

        if (response == null)
            return null;

        var importantSection = ImportantRunClassificationHelper.CreateNotificationViewModel(
            response.Classifications,
            response.ClassifiedRuns,
            run.RelativeYear);

        return new SetRunClassificationViewModel
        {
            RunId = run.RunId,
            RunName = run.RunName,
            RunClassification = run.RunClassification,
            RelativeYear = run.RelativeYear,
            CreatedAt = run.CreatedAt,
            CreatedBy = run.CreatedBy,
            RelativeYearClassifications = response,
            ImportantViewModel = importantSection
        };
    }
}
