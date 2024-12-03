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
        public IActionResult Index()
        {
            return this.View(
                ViewNames.LocalAuthorityUploadFileIndex,
                new ViewModelCommonData
                {
                    CurrentUser = this.HttpContext.User.Identity?.Name ?? "[User Name Not Found]",
                });
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile fileUpload)
        {
            try
            {
                var lapcapViewName = await this.GetViewName(fileUpload);
                return this.View(lapcapViewName);
            }
            catch (Exception)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
        }

        public async Task<IActionResult> Upload()
        {
            try
            {
                var lapcapFilePath = this.TempData["LapcapFilePath"]?.ToString();

                if (!string.IsNullOrEmpty(lapcapFilePath))
                {
                    using var stream = System.IO.File.OpenRead(lapcapFilePath);
                    var fileUpload = new FormFile(stream, 0, stream.Length, string.Empty, Path.GetFileName(stream.Name));

                    var viewName = await this.GetViewName(fileUpload);
                    ModelState.Clear();
                    return this.View(viewName);
                }

                // Code will reach this point if the uploaded file is not available
                return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
            catch (Exception)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
        }

        private async Task<string> GetViewName(IFormFile fileUpload)
        {
            if (this.ValidateCSV(fileUpload).ErrorMessage is not null)
            {
                var uploadErrors = this.TempData[UploadFileErrorIds.LocalAuthorityUploadErrors]?.ToString();
                if (!string.IsNullOrEmpty(uploadErrors))
                {
                    var localAuthorityUploadErrors = JsonConvert.DeserializeObject<ErrorViewModel>(uploadErrors);
                    if (localAuthorityUploadErrors != null)
                    {
                        localAuthorityUploadErrors.DOMElementId = ViewControlNames.FileUpload;
                    }

                    this.ViewBag.Errors = localAuthorityUploadErrors;
                    return ViewNames.LocalAuthorityUploadFileIndex;
                }
            }

            var localAuthorityDisposalCosts = await CsvFileHelper.PrepareLapcapDataForUpload(fileUpload);

            this.ViewData["localAuthorityDisposalCosts"] = localAuthorityDisposalCosts.ToArray();
            this.TempData["LapcapFileName"] = fileUpload.FileName;

            return ViewNames.LocalAuthorityUploadFileRefresh;
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
    }
}
