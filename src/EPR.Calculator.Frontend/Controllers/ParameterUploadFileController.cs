using CsvHelper;
using CsvHelper.Configuration;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EPR.Calculator.Frontend.Controllers
{
    public class ParameterUploadFileController : Controller
    {
        public IActionResult Index()
        {
            return this.View(ViewNames.ParameterUploadFileIndex);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile fileUpload)
        {
            try
            {
                var viewName = await this.GetViewName(fileUpload);
                return this.View(viewName);
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
                var filePath = this.TempData["FilePath"]?.ToString();

                if (!string.IsNullOrEmpty(filePath))
                {
                    using var stream = System.IO.File.OpenRead(filePath);
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

        public IActionResult DownloadCsvTemplate()
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), StaticHelpers.Path);
                return this.PhysicalFile(filePath, StaticHelpers.MimeType, StaticHelpers.Path);
            }
            catch (Exception ex)
            {
                return this.StatusCode(500, "An error occured while processing request" + ex.Message);
            }
        }

        private async Task<string> GetViewName(IFormFile fileUpload)
        {
            if (this.ValidateCSV(fileUpload).ErrorMessage is not null)
            {
                var uploadErrors = this.TempData[UploadFileErrorIds.DefaultParameterUploadErrors]?.ToString();
                if (!string.IsNullOrEmpty(uploadErrors))
                {
                    var parameterUploadErrors = JsonConvert.DeserializeObject<ErrorViewModel>(uploadErrors);
                    if (parameterUploadErrors != null)
                    {
                        parameterUploadErrors.DOMElementId = ViewControlNames.FileUpload;
                    }

                    this.ViewBag.Errors = parameterUploadErrors;
                    return ViewNames.ParameterUploadFileIndex;
                }
            }

            var schemeTemplateParameterValues = await CsvFileHelper.PrepareSchemeParameterDataForUpload(fileUpload);

            this.ViewData["schemeTemplateParameterValues"] = schemeTemplateParameterValues.ToArray();
            this.HttpContext.Session.SetString("FileName", fileUpload.FileName);

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
