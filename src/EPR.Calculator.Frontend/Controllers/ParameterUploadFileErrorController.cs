using System.IO.Compression;
using System.Text;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    [Authorize(Roles = "SASuperUser")]
    public class ParameterUploadFileErrorController : Controller
    {
        [Authorize(Roles = "SASuperUser")]
        public IActionResult Index(string parameterErrors)
        {
            try
            {
                var errors = DecompressString(parameterErrors);

                if (!string.IsNullOrEmpty(errors))
                {
                    var validationErrors = JsonConvert.DeserializeObject<List<ValidationErrorDto>>(errors);

                    if (validationErrors?.Find(error => !string.IsNullOrEmpty(error.ErrorMessage)) != null)
                    {
                        this.ViewBag.ValidationErrors = validationErrors;
                    }
                    else
                    {
                        this.ViewBag.Errors = JsonConvert.DeserializeObject<List<CreateDefaultParameterSettingErrorDto>>(errors);
                    }

                    if (this.ViewBag.ValidationErrors is null && this.ViewBag.Errors is not null)
                    {
                        this.ViewBag.ValidationErrors = new List<ValidationErrorDto>()
                        {
                            new ValidationErrorDto()
                            {
                                ErrorMessage = this.ViewBag.Errors.Count > 1 ? $"The file contained {this.ViewBag.Errors.Count} errors." : $"The file contained {this.ViewBag.Errors.Count} error.",
                            },
                        };
                    }

                    return this.View(
                        ViewNames.ParameterUploadFileErrorIndex,
                        new ViewModelCommonData
                        {
                            CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                        });
                }
                else
                {
                    return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
                }
            }
            catch (Exception)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
        }

        [HttpPost]
        [Authorize(Roles = "SASuperUser")]
        public IActionResult Index([FromBody] DataRequest errors)
        {
            var parameterErrors = CompressString(errors.Data);
            return this.Ok(new { redirectUrl = this.Url.Action("Index", new { parameterErrors }) });
        }

        [HttpPost]
        [Authorize(Roles = "SASuperUser")]
        public async Task<IActionResult> Upload(IFormFile fileUpload)
        {
            var csvErrors = CsvFileHelper.ValidateCSV(fileUpload);
            if (csvErrors.ErrorMessage is not null)
            {
                this.ViewBag.DefaultError = csvErrors;
                return this.View(
                    ViewNames.ParameterUploadFileErrorIndex,
                    new ViewModelCommonData
                    {
                        CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                    });
            }

            var schemeTemplateParameterValues = await CsvFileHelper.PrepareSchemeParameterDataForUpload(fileUpload);

            this.ViewData["schemeTemplateParameterValues"] = schemeTemplateParameterValues.ToArray();
            var viewModel = new ViewModelCommonData
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                FileName = fileUpload?.FileName,
            };

            return this.View(ViewNames.ParameterUploadFileRefresh, viewModel);
        }

        public static string CompressString(string text)
        {
            using var outputStream = new MemoryStream();
            using (var zipStream = new GZipStream(outputStream, CompressionMode.Compress))
            using (var writer = new StreamWriter(zipStream, Encoding.UTF8))
            {
                writer.Write(text);
            }

            return Convert.ToBase64String(outputStream.ToArray());
        }

        public static string DecompressString(string compressedText)
        {
            var inputBytes = Convert.FromBase64String(compressedText);
            using var inputStream = new MemoryStream(inputBytes);
            using var zipStream = new GZipStream(inputStream, CompressionMode.Decompress);
            using var reader = new StreamReader(zipStream, Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }
}
