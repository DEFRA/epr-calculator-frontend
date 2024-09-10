using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    public class UploadCSVErrorController : Controller
    {
        public IActionResult Index()
        {
            try
            {
                if (!string.IsNullOrEmpty(HttpContext.Session.GetString("Default_Parameter_Upload_Errors")))
                {
                    var errors = HttpContext.Session.GetString("Default_Parameter_Upload_Errors");

                    var validationErrors = JsonConvert.DeserializeObject<List<ValidationErrorDto>>(errors);

                    if (validationErrors.Any() && validationErrors.FirstOrDefault(error => !string.IsNullOrEmpty(error.ErrorMessage)) != null)
                    {
                        ViewBag.ValidationErrors = validationErrors;
                    }
                    else
                    {
                        ViewBag.Errors = JsonConvert.DeserializeObject<List<CreateDefaultParameterSettingErrorDto>>(errors);
                    }

                    if (ViewBag.ValidationErrors is null && ViewBag.Errors is not null)
                    {
                        ViewBag.ValidationErrors = new List<ValidationErrorDto>() { new ValidationErrorDto() { ErrorMessage = ViewBag.Errors.Count > 1 ? $"The file contained {ViewBag.Errors.Count} errors." : $"The file contained {ViewBag.Errors.Count} error." } };
                    }

                    return View(ViewNames.UploadCSVErrorIndex);
                }
                else
                {
                    return RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
        }

        [HttpPost]
        public IActionResult Index([FromBody]string errors)
        {
            HttpContext.Session.SetString("Default_Parameter_Upload_Errors", errors);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile fileUpload)
        {
                var csvErrors = CsvFileHelper.ValidateCSV(fileUpload);
                if (csvErrors.ErrorMessage is not null)
                {
                    ViewBag.DefaultError = csvErrors;
                    return View(ViewNames.UploadCSVErrorIndex);
                }

                var schemeTemplateParameterValues = await CsvFileHelper.PrepareSchemeParameterDataForUpload(fileUpload);

                ViewData["schemeTemplateParameterValues"] = schemeTemplateParameterValues.ToArray();

                return View(ViewNames.UploadFileRefresh);
        }
    }
}
