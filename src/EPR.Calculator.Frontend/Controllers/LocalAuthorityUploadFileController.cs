using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    [Authorize(Roles = "SASuperUser")]
    public class LocalAuthorityUploadFileController : Controller
    {
        private IActionResult RedirectToErrorPage => this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");

        [Authorize(Roles = "SASuperUser")]
        public IActionResult Index()
        {
            return this.View(
                ViewNames.LocalAuthorityUploadFileIndex,
                new LapcapUploadViewModel
                {
                    CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                });
        }

        [Authorize(Roles = "SASuperUser")]
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile fileUpload) => await this.ProcessUploadAsync(fileUpload);

        [Authorize(Roles = "SASuperUser")]
        public async Task<IActionResult> Upload()
        {
            if (!this.TryGetFileUpload(out var fileUpload))
            {
                return this.RedirectToErrorPage;
            }

            return await this.ProcessUploadAsync(fileUpload);
        }

        private bool TryGetFileUpload(out IFormFile fileUpload)
        {
            fileUpload = null;
            var filePath = this.TempData["LapcapFilePath"]?.ToString();
            if (string.IsNullOrEmpty(filePath)) { return false; }

            try
            {
                var stream = System.IO.File.OpenRead(filePath);
                fileUpload = new FormFile(stream, 0, stream.Length, string.Empty, Path.GetFileName(stream.Name));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<IActionResult> ProcessUploadAsync(IFormFile fileUpload)
        {
            try
            {
                var viewName = this.GetViewName(fileUpload);
                this.ModelState.Clear();

                if (viewName == ViewNames.LocalAuthorityUploadFileIndex)
                {
                    var viewModel = this.CreateLapcapUploadViewModel();
                    return this.View(viewName, viewModel);
                }
                else
                {
                    var localAuthorityDisposalCosts = await CsvFileHelper.PrepareLapcapDataForUpload(fileUpload);
                    return this.View(viewName, new LapcapRefreshViewModel { LapcapTemplateValue = localAuthorityDisposalCosts });
                }
            }
            catch (Exception)
            {
                return this.HandleProcessingException();
            }
        }

        private IActionResult HandleProcessingException() => this.RedirectToErrorPage;

        private ErrorViewModel ValidateCSV(IFormFile fileUpload)
        {
            ErrorViewModel validationErrors = CsvFileHelper.ValidateCSV(fileUpload);

            if (validationErrors.ErrorMessage != null)
            {
                this.TempData[UploadFileErrorIds.LocalAuthorityUploadErrors] = JsonConvert.SerializeObject(validationErrors);
            }

            return validationErrors;
        }

        private string GetViewName(IFormFile fileUpload)
        {
            if (this.ValidateCSV(fileUpload).ErrorMessage is not null)
            {
                return ViewNames.LocalAuthorityUploadFileIndex;
            }

            this.HttpContext.Session.SetString(SessionConstants.LapcapFileName, fileUpload.FileName);

            return ViewNames.LocalAuthorityUploadFileRefresh;
        }

        private LapcapUploadViewModel CreateLapcapUploadViewModel()
        {
            var errors = this.TempData[UploadFileErrorIds.LocalAuthorityUploadErrors] != null
                ? JsonConvert.DeserializeObject<ErrorViewModel>(this.TempData[UploadFileErrorIds.LocalAuthorityUploadErrors]?.ToString())
                : null;

            if (errors != null)
            {
                errors.DOMElementId = ViewControlNames.FileUpload;
            }

            return new LapcapUploadViewModel { Errors = errors };
        }
    }
}
