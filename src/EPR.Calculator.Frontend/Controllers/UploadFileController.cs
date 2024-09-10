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
            return View(ViewNames.UploadFileIndex);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile fileUpload)
        {
            try
            {
                if (ValidateCSV(fileUpload).ErrorMessage is not null)
                {
                    ViewBag.Errors = JsonConvert.DeserializeObject<ErrorViewModel>(TempData["Default_Parameter_Upload_Errors"].ToString());
                    return View(ViewNames.UploadFileIndex);
                }

                var schemeTemplateParameterValues = await CsvFileHelper.PrepareSchemeParameterDataForUpload(fileUpload);

                ViewData["schemeTemplateParameterValues"] = schemeTemplateParameterValues.ToArray();

                return View(ViewNames.UploadFileRefresh);
            }
            catch(Exception ex)
            {
                return RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
        }

        public async Task<IActionResult> Upload()
        {
            try
            {
                if (TempData["FilePath"] != null)
                {
                    using var stream = System.IO.File.OpenRead(TempData["FilePath"].ToString());
                    var fileUpload = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name));

                    if (ValidateCSV(fileUpload).ErrorMessage is not null)
                    {
                            ViewBag.Errors = JsonConvert.DeserializeObject<ErrorViewModel>(TempData["Default_Parameter_Upload_Errors"].ToString());
                            return View(ViewNames.UploadFileIndex);
                    }

                    var schemeTemplateParameterValues = await CsvFileHelper.PrepareSchemeParameterDataForUpload(fileUpload);

                    ViewData["schemeTemplateParameterValues"] = schemeTemplateParameterValues.ToArray();

                    return View(ViewNames.UploadFileRefresh);
                }

                // Code will reach this point if the uploaded file is not available
                return RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
            catch (Exception ex)
            {
                return RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
        }

        public IActionResult DownloadCsvTemplate()
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), StaticHelpers.Path);
                return PhysicalFile(filePath, "text/csv", "SchemeParameterTemplate.v0.1.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occured while processing request" + ex.Message);
            }
        }

        private ErrorViewModel ValidateCSV(IFormFile fileUpload)
        {
            ErrorViewModel validationErrors = CsvFileHelper.ValidateCSV(fileUpload);

            if (validationErrors.ErrorMessage != null)
            {
                TempData["Default_Parameter_Upload_Errors"] = JsonConvert.SerializeObject(validationErrors);
            }

            return validationErrors;
        }
    }
}
