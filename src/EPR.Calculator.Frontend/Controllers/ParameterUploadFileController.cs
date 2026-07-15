using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers;

public class ParameterUploadFileController : BaseController
{
    public IActionResult Index()
    {
        return View(ViewNames.ParameterUploadFileIndex, new ParameterUploadViewModel());
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
            var filePath = TempData["FilePath"]?.ToString();
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

    public async Task<IActionResult> DownloadCsvTemplate()
    {
        try
        {
            using (var client = new HttpClient())
            {
                var fileBytes = await client.GetByteArrayAsync(StaticHelpers.CsvTemplatePath);
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", StaticHelpers.CsvTemplateFileName);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occured while processing request" + ex.Message);
        }
    }

    private async Task<IActionResult> ProcessUploadAsync(IFormFile fileUpload)
    {
        try
        {
            var viewName = GetViewName(fileUpload);
            if (viewName == ViewNames.ParameterUploadFileIndex)
            {
                var viewModel = CreateParameterUploadViewModel();
                return View(viewName, viewModel);
            }
            else
            {
                var schemeTemplateParameterValues = await CsvFileHelper.PrepareSchemeParameterDataForUpload(fileUpload);
                var viewModel = new ParameterRefreshViewModel
                {
                    ParameterTemplateValues = schemeTemplateParameterValues,
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

    private ParameterUploadViewModel CreateParameterUploadViewModel()
    {
        var errors = TempData[UploadFileErrorIds.DefaultParameterUploadErrors] != null
            ? JsonConvert.DeserializeObject<ErrorViewModel>(TempData[UploadFileErrorIds.DefaultParameterUploadErrors]?.ToString() ?? string.Empty)
            : null;

        if (errors != null)
            errors.DOMElementId = ViewControlNames.FileUpload;

        ModelState.Clear();
        return new ParameterUploadViewModel
        {
            Errors = errors
        };
    }

    private string GetViewName(IFormFile fileUpload)
    {
        if (ValidateCSV(fileUpload).ErrorMessage is not null)
            return ViewNames.ParameterUploadFileIndex;

        return ViewNames.ParameterUploadFileRefresh;
    }

    private ErrorViewModel ValidateCSV(IFormFile fileUpload)
    {
        var validationErrors = CsvFileHelper.ValidateCSV(fileUpload);

        if (validationErrors.ErrorMessage != null)
            TempData[UploadFileErrorIds.DefaultParameterUploadErrors] = JsonConvert.SerializeObject(validationErrors);

        return validationErrors;
    }
}
