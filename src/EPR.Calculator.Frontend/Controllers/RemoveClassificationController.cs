using System.Net;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EPR.Calculator.Frontend.Controllers;

[Route("[controller]")]
public class RemoveClassificationController(
    IEprCalculatorApiService eprCalculatorApiService,
    ILogger<RemoveClassificationController> logger)
    : BaseController
{
    [Route("{runId:int}")]
    [HttpGet]
    public async Task<IActionResult> Index(int runId)
    {
        var viewModel = await CreateViewModel(runId);

        if (viewModel == null)
            return RedirectToError();

        return View(ViewNames.RemoveClassification, viewModel);
    }

    [HttpPost]
    [Route("Submit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(RemoveRunClassificationFormModel model)
    {
        if (!ModelState.IsValid)
        {
            if (ModelState[nameof(model.RunId)] is { ValidationState: ModelValidationState.Invalid })
                return RedirectToError();

            return await Index(model.RunId);
        }

        return await HandleClassificationSubmission(model);
    }

    private async Task<RemoveRunClassificationViewModel?> CreateViewModel(int runId)
    {
        var run = await eprCalculatorApiService.GetCalculatorRun(runId);

        if (run == null)
            return null;

        return new RemoveRunClassificationViewModel
        {
            RunId = run.RunId,
            RunName = run.RunName,
            RunClassification = run.RunClassification,
            RelativeYear = run.RelativeYear,
            CreatedAt = run.CreatedAt,
            CreatedBy = run.CreatedBy
        };
    }

    private async Task<IActionResult> HandleClassificationSubmission(RemoveRunClassificationFormModel model)
    {
        if (model.ClassifyRunType == (int)RunClassification.TEST_RUN)
        {
            var result = await eprCalculatorApiService.CallApi(
                HttpMethod.Put,
                "v2/calculatorRuns",
                body: new ClassificationDto
                {
                    RunId = model.RunId,
                    ClassificationId = (int)RunClassification.TEST_RUN
                });

            if (result.StatusCode == HttpStatusCode.Created)
            {
                return RedirectToAction(
                    ActionNames.Index,
                    ControllerNames.ClassifyRunConfirmation,
                    new { model.RunId });
            }

            logger.LogError("API did not return successful ({StatusCode})", result.StatusCode);
            return RedirectToError();
        }

        if (model.ClassifyRunType == (int)RunClassification.DELETED)
        {
            return RedirectToAction(
                ActionNames.Index,
                ControllerNames.CalculationRunDelete,
                new { model.RunId });
        }

        // Unexpected type
        return RedirectToError();
    }
}
