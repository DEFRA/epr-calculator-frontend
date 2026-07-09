using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Services;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

public class FileDownloadController(
    TelemetryClient telemetryClient,
    IResultBillingFileService fileDownloadService,
    ICalculatorRunDetailsService calculatorRunDetailsService)
    : BaseController
{
    [HttpGet]
    [Route("DownloadResultFile/{runId}")]
    public async Task<IActionResult> DownloadResultFile(int runId)
    {
        try
        {
            return await fileDownloadService.DownloadFileAsync($"v1/DownloadResult/{runId}", runId, HttpContext);
        }
        catch (Exception ex)
        {
            telemetryClient.TrackException(ex);
            return RedirectToAction(ActionNames.IndexNew, ControllerNames.DownloadFileErrorNewController, new { runId });
        }
    }

    [HttpGet]
    [Route("DownloadBillingFile/{runId}")]
    public async Task<IActionResult> DownloadBillingFile(int runId, bool isBillingFile, bool isDraftBillingFile)
    {
        try
        {
            var runDetails = await calculatorRunDetailsService.GetCalculatorRundetailsAsync(
                HttpContext,
                runId);

            if (runDetails == null || runDetails.RunName == null)
                return RedirectToError();

            if (runDetails.IsBillingFileGeneratedLatest.HasValue && !runDetails.IsBillingFileGeneratedLatest.Value)
                return RedirectToAction(ActionNames.Index, ControllerNames.CalculationRunOverview, new { runId });

            return await fileDownloadService.DownloadFileAsync($"v1/DownloadBillingFile/{runId}", runId, HttpContext, isBillingFile, isDraftBillingFile);
        }
        catch (Exception ex)
        {
            telemetryClient.TrackException(ex);
            return RedirectToAction(ActionNames.IndexNew, ControllerNames.DownloadFileErrorNewController, new { runId });
        }
    }
}
