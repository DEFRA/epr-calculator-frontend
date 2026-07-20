using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Services;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

public class FileDownloadController(
    IEprCalculatorApiService api,
    IFileDownloadService fileDownloads,
    TelemetryClient telemetryClient)
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
            telemetryClient.TrackException(ex);
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
            telemetryClient.TrackException(ex);
            return RedirectToAction(nameof(DownloadError));
        }
    }

    [Route("DownloadError")]
    public IActionResult DownloadError()
    {
        return View();
    }
}
