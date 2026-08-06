using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Helpers.Csv;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

public class ParameterUploadFileErrorController : BaseController
{
    public IActionResult Index()
    {
        var errorsInSession = HttpContext.Session.GetString(UploadFileErrorIds.DefaultParameterUploadErrors);

        if (string.IsNullOrWhiteSpace(errorsInSession))
            return RedirectToError();

        var (parameterErrors, validationErrors) = ApiValidationShim.Parse<CreateDefaultParameterSettingErrorDto>(errorsInSession);

        if (parameterErrors.Length > 0 && validationErrors.Length == 0)
        {
            validationErrors =
            [
                new ValidationErrorDto { ErrorMessage = $"The file contained {parameterErrors.Length} error{(parameterErrors.Length > 1 ? "s" : "")}." }
            ];
        }

        var viewModel = new ParameterUploadViewModel
        {
            ParamterErrors = parameterErrors.Length > 0 ? parameterErrors : null,
            ValidationErrors = validationErrors.Length > 0 ? validationErrors : null
        };

        return View(ViewNames.ParameterUploadFileErrorIndex, viewModel);
    }

    [HttpPost]
    public IActionResult Index([FromBody] string errors)
    {
        HttpContext.Session.SetString(UploadFileErrorIds.DefaultParameterUploadErrors, errors);

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile fileUpload)
    {
        var uploadViewModel = new ParameterUploadViewModel();

        if (!CsvFileHelper.TryValidateFile(fileUpload, out var errors))
        {
            uploadViewModel.Errors = new ErrorViewModel { DOMElementId = ViewControlNames.FileUpload, ErrorMessage = errors.First() };
            return View(ViewNames.ParameterUploadFileErrorIndex, uploadViewModel);
        }

        var schemeTemplateParameterValues = await ParametersCsvFileHelper.Parse(fileUpload);

        return View(ViewNames.ParameterUploadFileRefresh, new ParameterRefreshViewModel { ParameterTemplateValues = schemeTemplateParameterValues, FileName = fileUpload.FileName });
    }
}
