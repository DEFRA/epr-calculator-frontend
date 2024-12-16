using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace EPR.Calculator.Frontend.Controllers
{
    [Authorize(Roles = "SASuperUser")]
    public class LocalAuthorityUploadFileErrorController : Controller
    {
        [Authorize(Roles = "SASuperUser")]
        public IActionResult Index(string laErrors)
        {
            try
            {
                var lapcapErrors = Encoding.UTF8.GetString(Convert.FromBase64String(laErrors));

                if (!string.IsNullOrEmpty(lapcapErrors))
                {
                    var validationErrors = JsonConvert.DeserializeObject<List<ValidationErrorDto>>(lapcapErrors);

                    if (validationErrors?.Find(error => !string.IsNullOrEmpty(error.ErrorMessage)) != null)
                    {
                        this.ViewBag.ValidationErrors = validationErrors;
                    }
                    else
                    {
                        this.ViewBag.Errors = JsonConvert.DeserializeObject<List<CreateLapcapDataErrorDto>>(lapcapErrors);
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
                        ViewNames.LocalAuthorityUploadFileErrorIndex,
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
            var laErrors = Convert.ToBase64String(Encoding.UTF8.GetBytes(errors.Data));
            return this.Ok(new { redirectUrl = this.Url.Action("Index", new { laErrors }) });
        }

        [HttpPost]
        [Authorize(Roles = "SASuperUser")]
        public async Task<IActionResult> Upload(IFormFile fileUpload)
        {
            var lapcapFileErrors = CsvFileHelper.ValidateCSV(fileUpload);
            if (lapcapFileErrors.ErrorMessage is not null)
            {
                this.ViewBag.DefaultError = lapcapFileErrors;
                return this.View(
                    ViewNames.LocalAuthorityUploadFileErrorIndex,
                    new ViewModelCommonData
                    {
                        CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                    });
            }

            var localAuthorityDisposalCostsValues = await CsvFileHelper.PrepareLapcapDataForUpload(fileUpload);

            this.ViewData["localAuthorityDisposalCosts"] = localAuthorityDisposalCostsValues.ToArray();
            var viewModel = new ViewModelCommonData
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                FileName = fileUpload?.FileName,
            };

            return this.View(ViewNames.LocalAuthorityUploadFileRefresh, viewModel);
        }
    }
}