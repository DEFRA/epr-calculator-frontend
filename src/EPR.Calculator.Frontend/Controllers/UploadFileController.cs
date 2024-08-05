using CsvHelper;
using CsvHelper.Configuration;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EPR.Calculator.Frontend.Controllers
{
    public class UploadFileController : Controller
    {
        private const long MaxFileSize = 50 * 1024; // 50 KB

        public IActionResult Index()
        {
            return View(ViewNames.UploadFileIndex);
        }

        [HttpPost]
        public IActionResult Upload(IFormFile fileUpload)
        {
            try
            {
                if (ValidateCSV(fileUpload).Count > 0)
                {
                    if (TempData["Default_Parameter_Upload_Errors"] != null)
                    {
                        ViewBag.Errors = JsonConvert.DeserializeObject<List<ErrorViewModel>>(TempData["Default_Parameter_Upload_Errors"].ToString());
                        return View(ViewNames.UploadFileIndex);
                    }
                }

                var schemeTemplateParameterValues = PrepareDataForUpload(fileUpload);

                ViewData["schemeTemplateParameterValues"] = schemeTemplateParameterValues.ToArray();

                return View(ViewNames.UploadFileRefresh);
            }
            catch(Exception ex)
            {
                return RedirectToAction("Index", "StandardError");
            }
        }

        public IActionResult Upload()
        {
            try
            {
                if (TempData["FilePath"] != null)
                {
                    using var stream = System.IO.File.OpenRead(TempData["FilePath"].ToString());
                    var fileUpload = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name));

                    if (ValidateCSV(fileUpload).Count > 0)
                    {
                        if (TempData["Default_Parameter_Upload_Errors"] != null)
                        {
                            ViewBag.Errors = JsonConvert.DeserializeObject<List<ErrorViewModel>>(TempData["Default_Parameter_Upload_Errors"].ToString());
                            return View(ViewNames.UploadFileIndex);
                        }
                    }

                    var schemeTemplateParameterValues = PrepareDataForUpload(fileUpload);

                    ViewData["schemeTemplateParameterValues"] = schemeTemplateParameterValues.ToArray();

                    return View(ViewNames.UploadFileRefresh);
                }
                // TODO: Navigate to the standard error page once it is implemented
                // Code will reach this point if the uploaded file is not available
                return View(ViewNames.UploadFileRefresh);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "StandardError");
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

        private List<SchemeParameterTemplateValue> PrepareDataForUpload(IFormFile fileUpload)
        {
            var schemeTemplateParameterValues = new List<SchemeParameterTemplateValue>();

            using var memoryStream = new MemoryStream(new byte[fileUpload.Length]);
            fileUpload.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            using (var reader = new StreamReader(memoryStream))
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    PrepareHeaderForMatch = header => Regex.Replace(header.ToString(), @"\s", string.Empty),
                    ShouldSkipRecord = footer => footer.Row.GetField(0).Contains("upload version"),
                };
                using (var csv = new CsvReader(reader, config))
                {
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        schemeTemplateParameterValues.Add(
                            new SchemeParameterTemplateValue() { ParameterUniqueReferenceId = csv.GetField(0), ParameterValue = GetParameterValue(csv.GetField(5)) }
                        );
                    }
                }
            }

            return schemeTemplateParameterValues;
        }

        private decimal GetParameterValue(string parameterValue)
        {
            var parameterValueFormatted = parameterValue.Replace("£", "").Replace("%", "");
            return Decimal.Parse(parameterValueFormatted);
        }

        private List<ErrorViewModel> ValidateCSV(IFormFile fileUpload)
        {
            var validationErrors = new List<ErrorViewModel>();

            if (fileUpload == null || fileUpload.Length == 0)
            {
                validationErrors.Add(new ErrorViewModel() { DOMElementId = String.Empty, ErrorMessage = StaticHelpers.FileNotSelected });
            }

            if (fileUpload != null && !fileUpload.FileName.EndsWith(".csv"))
            {
                validationErrors.Add(new ErrorViewModel() { DOMElementId = String.Empty, ErrorMessage = StaticHelpers.FileMustBeCSV });
            }

            if (fileUpload != null && fileUpload.Length > MaxFileSize)
            {
                validationErrors.Add(new ErrorViewModel() { DOMElementId = String.Empty, ErrorMessage = StaticHelpers.FileNotExceed50KB });
            }

            if (validationErrors.Count > 0)
            {
                TempData["Default_Parameter_Upload_Errors"] = JsonConvert.SerializeObject(validationErrors);
            }

            return validationErrors;
        }
    }
}
