using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Services
{
    public interface IFileDownloadService
    {
        Task<FileResult> DownloadFileAsync(Uri apiUrl, int runId, string accessToken, bool isBillingFile = false, bool isDraftBillingFile = false);
    }
}