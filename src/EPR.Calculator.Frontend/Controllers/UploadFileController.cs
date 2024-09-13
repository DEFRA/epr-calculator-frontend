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
    public class UploadFileController : Controller
    {
        public IActionResult Index()
        {
            return this.View(ViewNames.UploadFileIndex);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile fileUpload)
        {
            try
            {
                if (this.ValidateCSV(fileUpload).ErrorMessage is not null)
                {
                    this.ViewBag.Errors = JsonConvert.DeserializeObject<ErrorViewModel>(this.TempData["Default_Parameter_Upload_Errors"].ToString());
                    return this.View(ViewNames.UploadFileIndex);
                }

                var schemeTemplateParameterValues = await CsvFileHelper.PrepareSchemeParameterDataForUpload(fileUpload);

                this.ViewData["schemeTemplateParameterValues"] = schemeTemplateParameterValues.ToArray();

                return this.View(ViewNames.UploadFileRefresh);
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
                if (this.TempData["FilePath"] != null)
                {
                    using var stream = System.IO.File.OpenRead(this.TempData["FilePath"].ToString());
                    var fileUpload = new FormFile(stream, 0, stream.Length, string.Empty, Path.GetFileName(stream.Name));

                    if (this.ValidateCSV(fileUpload).ErrorMessage is not null)
                    {
                        this.ViewBag.Errors = JsonConvert.DeserializeObject<ErrorViewModel>(this.TempData["Default_Parameter_Upload_Errors"].ToString());
                        return this.View(ViewNames.UploadFileIndex);
                    }

                    var schemeTemplateParameterValues = await CsvFileHelper.PrepareSchemeParameterDataForUpload(fileUpload);

                    this.ViewData["schemeTemplateParameterValues"] = schemeTemplateParameterValues.ToArray();

                    return this.View(ViewNames.UploadFileRefresh);
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
                return this.PhysicalFile(filePath, "text/csv", "SchemeParameterTemplate.v0.1.xlsx");
            }
            catch (Exception ex)
            {
                return this.StatusCode(500, "An error occured while processing request" + ex.Message);
            }
        }

        private ErrorViewModel ValidateCSV(IFormFile fileUpload)
        {
            ErrorViewModel validationErrors = CsvFileHelper.ValidateCSV(fileUpload);

            if (validationErrors.ErrorMessage != null)
            {
                this.TempData["Default_Parameter_Upload_Errors"] = JsonConvert.SerializeObject(validationErrors);
            }

            return validationErrors;
        }
    }
}
