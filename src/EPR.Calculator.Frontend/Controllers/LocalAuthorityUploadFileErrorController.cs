using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

public class LocalAuthorityUploadFileErrorController : BaseController
{
    public IActionResult Index()
    {
        var errorsInSession = HttpContext.Session.GetString(UploadFileErrorIds.LocalAuthorityUploadErrors);

        if (string.IsNullOrWhiteSpace(errorsInSession))
            return RedirectToError();

        var (lapcapErrors, validationErrors) = ApiValidationShim.Parse<CreateLapcapDataErrorDto>(errorsInSession);

        if (lapcapErrors.Length > 0 && validationErrors.Length == 0)
        {
            validationErrors =
            [
                new ValidationErrorDto { ErrorMessage = $"The file contained {lapcapErrors.Length} error{(lapcapErrors.Length > 1 ? "s" : "")}." }
            ];
        }

        var viewModel = new LapcapUploadViewModel
        {
            LapcapErrors = lapcapErrors.Length > 0 ? lapcapErrors : null,
            ValidationErrors = validationErrors.Length > 0 ? validationErrors : null
        };

        return View(ViewNames.LocalAuthorityUploadFileErrorIndex, viewModel);
    }

    [HttpPost]
    public IActionResult Index([FromBody] string errors)
    {
        HttpContext.Session.SetString(UploadFileErrorIds.LocalAuthorityUploadErrors, errors);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile fileUpload)
    {
        var lapcapFileErrors = ObsoleteCsvFileHelper.ValidateCSV(fileUpload);
        var lapcapUploadViewModel = new LapcapUploadViewModel();

        if (lapcapFileErrors.ErrorMessage is not null)
        {
            lapcapUploadViewModel.Errors = new List<ErrorViewModel> { lapcapFileErrors };
            return View(
                ViewNames.LocalAuthorityUploadFileErrorIndex,
                lapcapUploadViewModel);
        }

        var localAuthorityDisposalCostsValues = await ObsoleteCsvFileHelper.PrepareLapcapDataForUpload(fileUpload);

        return View(ViewNames.LocalAuthorityUploadFileRefresh, new LapcapRefreshViewModel { LapcapTemplateValue = localAuthorityDisposalCostsValues, FileName = fileUpload.FileName });
    }
}
