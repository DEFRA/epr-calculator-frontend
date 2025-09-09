using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    public class LocalAuthorityUploadFileController : Controller
    {
        private IActionResult RedirectToErrorPage => this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");

        public IActionResult Index()
        {
            return this.View(
                ViewNames.LocalAuthorityUploadFileIndex,
                new LapcapUploadViewModel
                {
                    CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                });
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile fileUpload)
        {
            return await this.ProcessUploadAsync(fileUpload);
        }

        public async Task<IActionResult> Upload()
        {
            try
            {
                var filePath = this.TempData["LapcapFilePath"]?.ToString();
                if (string.IsNullOrEmpty(filePath))
                {
                    return this.RedirectToErrorPage;
                }

                using var stream = System.IO.File.OpenRead(filePath);
                var fileUpload = new FormFile(stream, 0, stream.Length, string.Empty, Path.GetFileName(stream.Name));
                return await this.ProcessUploadAsync(fileUpload);
            }
            catch (Exception)
            {
                return this.RedirectToErrorPage;
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
                    return this.View(viewName, new LapcapRefreshViewModel { LapcapTemplateValue = localAuthorityDisposalCosts, FileName = fileUpload.FileName });
                }
            }
            catch (Exception)
            {
                return this.RedirectToErrorPage;
            }
        }

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

            return ViewNames.LocalAuthorityUploadFileRefresh;
        }

        private LapcapUploadViewModel CreateLapcapUploadViewModel()
        {
            var errors = this.TempData[UploadFileErrorIds.LocalAuthorityUploadErrors] != null
                ? JsonConvert.DeserializeObject<ErrorViewModel>(this.TempData[UploadFileErrorIds.LocalAuthorityUploadErrors]?.ToString() ?? string.Empty)
                : null;

            if (errors != null)
            {
                errors.DOMElementId = ViewControlNames.FileUpload;
            }

            return new LapcapUploadViewModel { Errors = new List<ErrorViewModel> { errors! } };
        }
    }
}
