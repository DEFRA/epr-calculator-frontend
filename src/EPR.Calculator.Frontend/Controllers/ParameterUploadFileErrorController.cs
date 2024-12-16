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
        private IMemoryCache _memoryCache;

        public ParameterUploadFileErrorController(IMemoryCache memoryCache)
        {
            this._memoryCache = memoryCache;
        }

        [Authorize(Roles = "SASuperUser")]
        public IActionResult Index(string key)
        {
            try
            {
                string errors = string.Empty;
                if (this._memoryCache.TryGetValue(key, out string parameterErrors))
                {
                    errors = Encoding.UTF8.GetString(Convert.FromBase64String(parameterErrors));
                }

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
            var parameterErrors = Convert.ToBase64String(Encoding.UTF8.GetBytes(errors.Data));
            var key = Guid.NewGuid().ToString();

            using (var cacheEntry = this._memoryCache.CreateEntry(key))
            {
                cacheEntry.Value = parameterErrors;
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            }

            return this.Ok(new { redirectUrl = this.Url.Action("Index", new { key }) });
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
    }
}
