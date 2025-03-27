using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    [Authorize(Roles = "SASuperUser")]
    public class LocalAuthorityUploadFileErrorController : Controller
    {
        [Authorize(Roles = "SASuperUser")]
        public IActionResult Index()
        {
            try
            {
                var lapcapErrors = this.HttpContext.Session.GetString(UploadFileErrorIds.LocalAuthorityUploadErrors);

                if (!string.IsNullOrEmpty(lapcapErrors))
                {
                    var validationErrors = JsonConvert.DeserializeObject<List<ValidationErrorDto>>(lapcapErrors);

                    var lapcapUploadViewModel = new LapcapUploadViewModel()
                    {
                        CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                    };

                    if (validationErrors?.Find(error => !string.IsNullOrEmpty(error.ErrorMessage)) != null)
                    {
                        lapcapUploadViewModel.ValidationErrors = validationErrors;
                    }
                    else
                    {
                        lapcapUploadViewModel.LapcapErrors = JsonConvert.DeserializeObject<List<CreateLapcapDataErrorDto>>(lapcapErrors);
                    }

                    if (lapcapUploadViewModel.ValidationErrors is null && lapcapUploadViewModel.LapcapErrors is not null)
                    {
                        lapcapUploadViewModel.ValidationErrors =
                        [
                            new ValidationErrorDto()
                            {
                                ErrorMessage = lapcapUploadViewModel.LapcapErrors.Count > 1 ? $"The file contained {lapcapUploadViewModel.LapcapErrors.Count} errors." : $"The file contained {lapcapUploadViewModel.LapcapErrors.Count} error.",
                            },
                        ];
                    }

                    return this.View(
                        ViewNames.LocalAuthorityUploadFileErrorIndex,
                        lapcapUploadViewModel);
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
        public IActionResult Index([FromBody] string errors)
        {
            this.HttpContext.Session.SetString(UploadFileErrorIds.LocalAuthorityUploadErrors, errors);

            return this.Ok();
        }

        [HttpPost]
        [Authorize(Roles = "SASuperUser")]
        public async Task<IActionResult> Upload(IFormFile fileUpload)
        {
            var lapcapFileErrors = CsvFileHelper.ValidateCSV(fileUpload);
            var lapcapUploadViewModel = new LapcapUploadViewModel
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
            };

            if (lapcapFileErrors.ErrorMessage is not null)
            {
                lapcapUploadViewModel.Errors = lapcapFileErrors;
                return this.View(
                    ViewNames.LocalAuthorityUploadFileErrorIndex,
                    lapcapUploadViewModel);
            }

            var localAuthorityDisposalCostsValues = await CsvFileHelper.PrepareLapcapDataForUpload(fileUpload);

            return this.View(ViewNames.LocalAuthorityUploadFileRefresh, new LapcapRefreshViewModel { LapcapTemplateValue = localAuthorityDisposalCostsValues, FileName = fileUpload.FileName });
        }
    }
}