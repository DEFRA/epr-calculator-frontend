using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

/// <summary>
///     Controller for reason for rejection
/// </summary>
[Route("[controller]")]
public class ReasonForRejectionController(
    ICalculatorRunDetailsService calculatorRunDetailsService)
    : BaseController
{
    [Route("{runId}")]
    public async Task<IActionResult> Index(int runId)
    {
        var runDetails = await calculatorRunDetailsService
            .GetCalculatorRundetailsAsync(HttpContext, runId);
        var viewModel = new AcceptRejectConfirmationViewModel
        {
            CalculationRunId = runId,
            CalculationRunName = runDetails.RunName,
            Reason = TempData[nameof(AcceptRejectConfirmationViewModel.Reason)]?.ToString() ?? string.Empty,
            Status = BillingStatus.Rejected
        };

        return View(ViewNames.ReasonForRejectionIndex, viewModel);
    }

    [HttpPost]
    [ActionName("Index")]
    [Route("{runId}")]
    public IActionResult IndexPost(int runId, AcceptRejectConfirmationViewModel model)
    {
        if (string.IsNullOrEmpty(model.Reason))
        {
            ModelState.Remove("Reason");
            return View(ViewNames.ReasonForRejectionIndex, model);
        }

        ModelState.Clear();
        TempData[nameof(model.Reason)] = model.Reason;
        return View(ViewNames.AcceptRejectConfirmationIndex, model);
    }
}
