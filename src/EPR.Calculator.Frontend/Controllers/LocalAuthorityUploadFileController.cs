using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Helpers.Csv.Lapcap;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels.CsvUpload;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

public class LocalAuthorityUploadFileController(
    IConfiguration configuration,
    IEprCalculatorApiService eprCalculatorApiService
) : BaseController
{
    private const string ApiErrorsKey = "Local_Authority_Upload_Errors";

    private CsvUploadViewModel UploadTemplate => new()
    {
        Title = "Upload new local authority disposal costs",
        BackLinkUrl = Url.Action("Index", "LocalAuthorityDisposalCosts")!,
        UploadUrl = Url.Action("Upload", "LocalAuthorityUploadFile")!
    };

    [HttpGet]
    public IActionResult Index()
    {
        return View("Views/CsvUpload/Index", UploadTemplate);
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile? fileUpload, CancellationToken cancellationToken)
    {
        var result = await LapcapCsvFileHelper.Parse(fileUpload, cancellationToken);

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

        var processRequest = new SetLapcapDataRequest
        {
            Filename = fileUpload!.FileName,
            RelativeYear =  CommonUtil.GetRelativeYear(HttpContext.Session, CommonUtil.GetRelativeYearStartingMonth(configuration)),
            Values = result.Records
        };

        return View("Views/CsvUpload/Processing", new CsvUploadProcessingViewModel
        {
            ProcessingUrl = Url.Action("Process", "LocalAuthorityUploadFile")!,
            SuccessUrl = Url.Action("Index", "LocalAuthorityConfirmation")!,
            ErrorUrl = Url.Action("Errors", "LocalAuthorityUploadFile")!,
            Payload = processRequest
        });
    }

    [HttpPost]
    public async Task<IActionResult> Process([FromBody] SetLapcapDataRequest request, CancellationToken cancellationToken)
    {
        using var response = await eprCalculatorApiService.CallApi(
            HttpMethod.Put,
            "v1/lapcapData",
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
}
