using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    public class ParameterUploadFileController : Controller
    {
        private IActionResult RedirectToErrorPage => this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");

        public IActionResult Index()
        {
            var currentUser = CommonUtil.GetUserName(this.HttpContext);
            return this.View(
                 ViewNames.ParameterUploadFileIndex,
                 new ParameterUploadViewModel
                 {
                     CurrentUser = currentUser,
                     BackLinkViewModel = new BackLinkViewModel()
                     {
                         BackLink = "ViewDefaultParameters",
                         CurrentUser = currentUser,
                     },
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
                var filePath = this.TempData["FilePath"]?.ToString();
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

        public IActionResult DownloadCsvTemplate()
        {
            try
            {
                var templateFilePath = Path.Combine(Directory.GetCurrentDirectory(), StaticHelpers.Path);
                var templateFileName = Path.GetFileName(templateFilePath);

                return this.PhysicalFile(templateFilePath, StaticHelpers.MimeType, templateFileName);
            }
            catch (Exception ex)
            {
                return this.StatusCode(500, "An error occured while processing request" + ex.Message);
            }
        }

        private async Task<IActionResult> ProcessUploadAsync(IFormFile fileUpload)
        {
            try
            {
                var viewName = this.GetViewName(fileUpload);
                if (viewName == ViewNames.ParameterUploadFileIndex)
                {
                    var viewModel = this.CreateParameterUploadViewModel();
                    return this.View(viewName, viewModel);
                }
                else
                {
                    var schemeTemplateParameterValues = await CsvFileHelper.PrepareSchemeParameterDataForUpload(fileUpload);
                    var viewModel = new ParameterRefreshViewModel
                    {
                        ParameterTemplateValues = schemeTemplateParameterValues,
                        FileName = fileUpload.FileName,
                        BackLinkViewModel = new BackLinkViewModel()
                        {
                            BackLink = ControllerNames.ViewDefaultParameters,
                            CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                        },
                    };
                    return this.View(viewName, viewModel);
                }
            }
            catch (Exception)
            {
                return this.RedirectToErrorPage;
            }
        }

        private ParameterUploadViewModel CreateParameterUploadViewModel()
        {
            var errors = this.TempData[UploadFileErrorIds.DefaultParameterUploadErrors] != null
                ? JsonConvert.DeserializeObject<ErrorViewModel>(this.TempData[UploadFileErrorIds.DefaultParameterUploadErrors]?.ToString())
                : null;

            if (errors != null)
            {
                errors.DOMElementId = ViewControlNames.FileUpload;
            }

            this.ModelState.Clear();
            return new ParameterUploadViewModel
            {
                Errors = errors,
                BackLinkViewModel = new BackLinkViewModel()
                {
                    BackLink = ControllerNames.ViewDefaultParameters,
                    CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                },
            };
        }

        private string GetViewName(IFormFile fileUpload)
        {
            if (this.ValidateCSV(fileUpload).ErrorMessage is not null)
            {
                return ViewNames.ParameterUploadFileIndex;
            }

            return ViewNames.ParameterUploadFileRefresh;
        }

        private ErrorViewModel ValidateCSV(IFormFile fileUpload)
        {
            ErrorViewModel validationErrors = CsvFileHelper.ValidateCSV(fileUpload);

            if (validationErrors.ErrorMessage != null)
            {
                this.TempData[UploadFileErrorIds.DefaultParameterUploadErrors] = JsonConvert.SerializeObject(validationErrors);
            }

            return validationErrors;
        }
    }
}
