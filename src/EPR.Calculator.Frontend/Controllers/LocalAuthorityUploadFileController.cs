using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers;

public class LocalAuthorityUploadFileController : BaseController
{
    public IActionResult Index()
    {
        return View(ViewNames.LocalAuthorityUploadFileIndex, new LapcapUploadViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile fileUpload)
    {
        return await ProcessUploadAsync(fileUpload);
    }

    public async Task<IActionResult> Upload()
    {
        try
        {
            var filePath = TempData["LapcapFilePath"]?.ToString();
            if (string.IsNullOrEmpty(filePath))
                return RedirectToError();

            using var stream = System.IO.File.OpenRead(filePath);
            var fileUpload = new FormFile(stream, 0, stream.Length, string.Empty, Path.GetFileName(stream.Name));
            return await ProcessUploadAsync(fileUpload);
        }
        catch (Exception)
        {
            return RedirectToError();
        }
    }

    private async Task<IActionResult> ProcessUploadAsync(IFormFile fileUpload)
    {
        try
        {
            var viewName = GetViewName(fileUpload);
            ModelState.Clear();
            if (viewName == ViewNames.LocalAuthorityUploadFileIndex)
            {
                var viewModel = CreateLapcapUploadViewModel();
                return View(viewName, viewModel);
            }
            else
            {
                var localAuthorityDisposalCosts = await CsvFileHelper.PrepareLapcapDataForUpload(fileUpload);
                var viewModel = new LapcapRefreshViewModel
                {
                    LapcapTemplateValue = localAuthorityDisposalCosts,
                    FileName = fileUpload.FileName
                };
                return View(viewName, viewModel);
            }
        }
        catch (Exception)
        {
            return RedirectToError();
        }
    }

    private ErrorViewModel ValidateCSV(IFormFile fileUpload)
    {
        var validationErrors = CsvFileHelper.ValidateCSV(fileUpload);

        if (validationErrors.ErrorMessage != null)
            TempData[UploadFileErrorIds.LocalAuthorityUploadErrors] = JsonConvert.SerializeObject(validationErrors);

        return validationErrors;
    }

    private string GetViewName(IFormFile fileUpload)
    {
        if (ValidateCSV(fileUpload).ErrorMessage is not null)
            return ViewNames.LocalAuthorityUploadFileIndex;

        return ViewNames.LocalAuthorityUploadFileRefresh;
    }

    private LapcapUploadViewModel CreateLapcapUploadViewModel()
    {
        var errors = TempData[UploadFileErrorIds.LocalAuthorityUploadErrors] != null
            ? JsonConvert.DeserializeObject<ErrorViewModel>(TempData[UploadFileErrorIds.LocalAuthorityUploadErrors]?.ToString() ?? "{}")
            : null;

        if (errors != null)
            errors.DOMElementId = ViewControlNames.FileUpload;

        return new LapcapUploadViewModel
        {
            Errors = new List<ErrorViewModel> { errors! }
        };
    }
}
