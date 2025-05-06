using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    public class ParameterUploadFileErrorController : Controller
    {
        private IActionResult RedirectToErrorPage => this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");

        public IActionResult Index()
        {
            try
            {
                var errors = this.HttpContext.Session.GetString(UploadFileErrorIds.DefaultParameterUploadErrors);

                if (string.IsNullOrEmpty(errors))
                {
                    return this.RedirectToErrorPage;
                }

                var validationErrors = JsonConvert.DeserializeObject<List<ValidationErrorDto>>(errors);

                var parameterUploadViewModel = new ParameterUploadViewModel()
                {
                    CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                };

                if (validationErrors?.Find(error => !string.IsNullOrEmpty(error.ErrorMessage)) != null)
                {
                    parameterUploadViewModel.ValidationErrors = validationErrors;
                }
                else
                {
                    parameterUploadViewModel.ParamterErrors = JsonConvert.DeserializeObject<List<CreateDefaultParameterSettingErrorDto>>(errors);
                }

                if (parameterUploadViewModel.ValidationErrors == null && parameterUploadViewModel.ParamterErrors != null)
                {
                    parameterUploadViewModel.ValidationErrors =
                    [
                        new ValidationErrorDto()
                            {
                                ErrorMessage = parameterUploadViewModel.ParamterErrors.Count > 1 ? $"The file contained {parameterUploadViewModel.ParamterErrors.Count} errors." : $"The file contained {parameterUploadViewModel.ParamterErrors.Count} error.",
                            },
                        ];
                }

                return this.View(
                    ViewNames.ParameterUploadFileErrorIndex,
                    parameterUploadViewModel);
            }
            catch (Exception)
            {
                return this.RedirectToErrorPage;
            }
        }

        [HttpPost]
        public IActionResult Index([FromBody] string errors)
        {
            this.HttpContext.Session.SetString(UploadFileErrorIds.DefaultParameterUploadErrors, errors);

            return this.Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile fileUpload)
        {
            var csvErrors = CsvFileHelper.ValidateCSV(fileUpload);
            var uploadViewModel = new ParameterUploadViewModel
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
            };
            if (csvErrors.ErrorMessage is not null)
            {
                uploadViewModel.Errors = new List<ErrorViewModel> { csvErrors! };
                return this.View(ViewNames.ParameterUploadFileErrorIndex, uploadViewModel);
            }

            var schemeTemplateParameterValues = await CsvFileHelper.PrepareSchemeParameterDataForUpload(fileUpload);

            return this.View(ViewNames.ParameterUploadFileRefresh, new ParameterRefreshViewModel { ParameterTemplateValues = schemeTemplateParameterValues, FileName = fileUpload.FileName });
        }
    }
}
