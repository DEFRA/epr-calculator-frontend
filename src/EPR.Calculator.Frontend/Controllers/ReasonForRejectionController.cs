using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

[Route("[controller]")]
public class ReasonForRejectionController(IEprCalculatorApiService eprCalculatorApiService)
    : BaseController
{
    [HttpGet("{runId:int}")]
    public async Task<IActionResult> Index(int runId)
    {
        var viewModel = await CreateViewModel(runId, null);

        if (viewModel == null)
            return RedirectToError();

        return View(ViewNames.ReasonForRejectionIndex, viewModel);
    }

    [HttpPost("{runId:int}")]
    [ActionName("Index")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IndexPost(int runId, ReasonForRejectionFormModel model)
    {
        var viewModel = await CreateViewModel(runId, model.Reason);

        if (viewModel == null)
            return RedirectToError();

        return ModelState.IsValid
            ? View(ViewNames.AcceptRejectConfirmationIndex, viewModel)
            : View(ViewNames.ReasonForRejectionIndex, viewModel);
    }

    private async Task<AcceptRejectConfirmationViewModel?> CreateViewModel(int runId, string? reason)
    {
        var run = await eprCalculatorApiService.GetCalculatorRun(runId);

        if (run == null)
            return null;

        return new AcceptRejectConfirmationViewModel
        {
            RunId = runId,
            RunName = run.RunName,
            Status = BillingStatus.Rejected,
            Reason = reason
        };
    }
}
