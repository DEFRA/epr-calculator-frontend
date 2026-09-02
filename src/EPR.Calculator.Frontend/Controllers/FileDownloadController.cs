using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Services;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

public class FileDownloadController(
    IEprCalculatorApiService api,
    IFileDownloadService fileDownloads,
    ILogger<FileDownloadController> logger)
    : BaseController
{
    [HttpGet]
    [Route("DownloadResultFile/{runId:int}")]
    public async Task<IActionResult> DownloadResultFile(int runId)
    {
        try
        {
            var runDto = await api.GetCalculatorRun(runId);

            if (runDto == null)
                return RedirectToError();

            return await fileDownloads.DownloadResultFile(runId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when donwloading result file for {RunId}", runId);
            return RedirectToAction(nameof(DownloadError));
        }
    }

    [HttpGet]
    [Route("DownloadBillingFile/{runId:int}")]
    public async Task<IActionResult> DownloadBillingFile(int runId)
    {
        try
        {
            var runDto = await api.GetCalculatorRun(runId);

            if (runDto == null)
                return RedirectToError();

            if (runDto.BillingFile?.IsLatest != true)
                return RedirectToAction(ActionNames.Index, ControllerNames.CalculationRunOverview, new { runId });

            return await fileDownloads.DownloadBillingFile(runId, runDto.BillingFile!.HasBeenSentToFss);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when donwloading billing file for {RunId}", runId);
            return RedirectToAction(nameof(DownloadError));
        }
    }

    [Route("DownloadError")]
    public IActionResult DownloadError()
    {
        return View();

    }
}
