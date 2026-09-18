using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Helpers.Csv.DefaultParameters;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels.CsvUpload;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

public class DefaultParametersUploadFileController(
    IConfiguration configuration,
    IEprCalculatorApiService eprCalculatorApiService
) : BaseController
{
    private const string ApiErrorsKey = "Default_Parameters_Upload_Errors";

    private CsvUploadViewModel UploadTemplate => new()
    {
        Title = "Upload new default calculator parameters",
        BackLinkUrl = Url.Action("Index", "DefaultParameters")!,
        UploadUrl = Url.Action("Upload", "DefaultParametersUploadFile")!,
        DownloadTemplate = new CsvUploadViewModel.TemplateDownloadViewModel
        {
            Url = Url.Action("DownloadCsvTemplate", "DefaultParametersUploadFile")!,
            LinkText = "default parameters spreadsheet template"
        }
    };

    [HttpGet]
    public IActionResult Index()
    {
        return View("Views/CsvUpload/Index", UploadTemplate);
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile? fileUpload, CancellationToken cancellationToken)
    {
        var result = await DefaultParametersCsvFileHelper.Parse(fileUpload, cancellationToken);

        if (!result.IsSuccess)
        {
            return View("Views/CsvUpload/Index", UploadTemplate with
            {
                Errors = new CsvUploadViewModel.ErrorsViewModel
                {
                    FileErrors = result.FileErrors,
                    ContentErrors = result.ContentErrors
                }
            });
        }

        var processRequest = new SetDefaultParametersRequest
        {
            Filename = fileUpload!.FileName,
            RelativeYear =  CommonUtil.GetRelativeYear(HttpContext.Session, CommonUtil.GetRelativeYearStartingMonth(configuration)),
            Parameters = [..result.Records.Where(p => !p.Id.Contains("upload version", StringComparison.OrdinalIgnoreCase))]
        };

        return View("Views/CsvUpload/Processing", new CsvUploadProcessingViewModel
        {
            ProcessingUrl = Url.Action("Process", "DefaultParametersUploadFile")!,
            SuccessUrl = Url.Action("Index", "DefaultParametersConfirmation")!,
            ErrorUrl = Url.Action("Errors", "DefaultParametersUploadFile")!,
            Payload = processRequest
        });
    }

    [HttpPost]
    public async Task<IActionResult> Process([FromBody] SetDefaultParametersRequest request, CancellationToken cancellationToken)
    {
        using var response = await eprCalculatorApiService.CallApi(
            HttpMethod.Put,
            "v1/defaultParameterSetting",
            body: request,
            cancellationToken: cancellationToken);

        if (response is { IsSuccessStatusCode: true })
            return NoContent();

        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        HttpContext.Session.SetString(ApiErrorsKey, errorContent);

        return BadRequest();
    }

    [HttpGet]
    public IActionResult Errors()
    {
        var apiErrorsJson = HttpContext.Session.GetString(ApiErrorsKey);
        HttpContext.Session.Remove(ApiErrorsKey);

        if (!ApiValidationShim.TryParseAsProblemDetails(apiErrorsJson, out var problemDetails))
            return RedirectToError();

        // The API validates the contents of the file, so anything it rejects is a content error.
        return View("Views/CsvUpload/Index", UploadTemplate with
        {
            Errors = new CsvUploadViewModel.ErrorsViewModel
            {
                FileErrors = [],
                ContentErrors = [.. problemDetails.Errors.SelectMany(kv => kv.Value)]
            }
        });
    }

    [HttpGet]
    public IActionResult DownloadCsvTemplate()
    {
        // File() resolves the path against the web root (wwwroot); PhysicalFile() would need an
        // absolute path on disk and throws when given a virtual one.
        return File(
            "~/templates/DefaultParameterTemplate.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "DefaultParameterTemplate.xlsx");
    }
}
