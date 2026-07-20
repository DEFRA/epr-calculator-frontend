using System.Globalization;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Extensions;
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
                ClassificationId = (int)model.ClassifyRunType!
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

        var classificationsForRelativeYear = await eprCalculatorApiService.Get<RelativeYearClassificationResponseDto>("v1/ClassificationByRelativeYear", new Dictionary<string, string?>
        {
            ["RunId"] = run.RunId.ToString(),
            ["RelativeYearValue"] = run.RelativeYear.ToString()
        });

        if (classificationsForRelativeYear == null)
            return null;

        SetStatusDescriptions(classificationsForRelativeYear.Classifications);

        var importantSection = ImportantRunClassificationHelper.CreateclassificationViewModel(
            classificationsForRelativeYear.ClassifiedRuns,
            run.RelativeYear);

        if (classificationsForRelativeYear.Classifications.Count == 1 &&
            classificationsForRelativeYear.Classifications.Exists(x => x.Id == (int)RunClassification.TEST_RUN) && !importantSection.IsAnyRunInProgress)
            importantSection.IsDisplayTestRun = true;

        return new SetRunClassificationViewModel
        {
            RunId = run.RunId,
            RunName = run.RunName,
            RunClassification = run.RunClassification,
            RelativeYear = run.RelativeYear,
            CreatedAt = run.CreatedAt,
            CreatedBy = run.CreatedBy,
            RelativeYearClassifications = classificationsForRelativeYear,
            ImportantViewModel = importantSection
        };
    }

    private static void SetStatusDescriptions(List<CalculatorRunClassificationDto> model)
    {
        foreach (var classification in model)
        {
            classification.Description = GetStatusDescription(classification.Id);
            classification.Status = GetStatus(classification);
        }

        static string GetStatusDescription(int classificationId)
        {
            return classificationId switch
            {
                (int)RunClassification.INITIAL_RUN => CommonConstants.InitialRunDescription,
                (int)RunClassification.TEST_RUN => CommonConstants.TestRunDescription,
                (int)RunClassification.INTERIM_RECALCULATION_RUN => CommonConstants.InterimRunDescription,
                (int)RunClassification.FINAL_RECALCULATION_RUN => CommonConstants.FinalRecalculationRunDescription,
                (int)RunClassification.FINAL_RUN => CommonConstants.FinalRecalculationRunDescription,
                _ => string.Empty
            };
        }
    }

    private static string GetStatus(CalculatorRunClassificationDto classificationDto)
    {
        return classificationDto.Id switch
        {
            (int)RunClassification.INITIAL_RUN => CommonConstants.InitialRunStatus,
            (int)RunClassification.TEST_RUN => CommonConstants.TestRunStatus,
            (int)RunClassification.INTERIM_RECALCULATION_RUN => CommonConstants.InterimRunStatus,
            (int)RunClassification.FINAL_RECALCULATION_RUN => CommonConstants.FinalRecalculationRunStatus,
            (int)RunClassification.FINAL_RUN => CommonConstants.FinalRunStatus,
            _ => new CultureInfo("en-GB", false).TextInfo.ToFirstLetterCap(classificationDto.Status)
        };
    }
}
