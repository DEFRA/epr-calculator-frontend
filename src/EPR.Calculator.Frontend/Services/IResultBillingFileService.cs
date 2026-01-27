using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Services
{
    public interface IResultBillingFileService
    {
        Task<FileResult> DownloadFileAsync(Uri apiUrl, int runId, HttpContext httpContext, bool isBillingFile = false, bool isDraftBillingFile = false);
    }
}