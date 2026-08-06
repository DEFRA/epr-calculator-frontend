using System.Net;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Helpers.Csv;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

public class LocalAuthorityUploadFileController(
    IConfiguration configuration,
    IEprCalculatorApiService eprCalculatorApiService,
    ILogger<LocalAuthorityUploadFileController> logger
) : BaseController
{
    private const string ApiErrorsKey = "Local_Authority_Upload_Errors";

    private static readonly FileUploadErrorViewModel ErrorTemplate = new()
    {
        InputId = LapcapUploadViewModel.DomElements.InputId,
        DetailsId = LapcapUploadViewModel.DomElements.ErrorDetailsId,
        CallToActionId = LapcapUploadViewModel.DomElements.ErrorCallToActionId,
        FileErrors = [],
        ContentErrors = []
    };

    [HttpGet]
    public IActionResult Index()
    {
        return View("Index", new LapcapUploadViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile? fileUpload, CancellationToken cancellationToken)
    {
        try
        {
            var result = await LapcapCsvFileHelper.Parse(fileUpload, cancellationToken);

            if (!result.IsSuccess)
            {
                return View("Index", new LapcapUploadViewModel
                {
                    ErrorsViewModel = ErrorTemplate with
                    {
                        FileErrors = result.FileErrors,
                        ContentErrors = result.ContentErrors
                    }
                });
            }

            return View("Processing", new LapcapProcessingViewModel
            {
                Filename = fileUpload!.FileName,
                Values = result.Records
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Uncaught exception when handling CSV file upload");
            return RedirectToError();
        }
    }

    [HttpPost]
    public async Task<IActionResult> Process([FromBody] LapcapProcessingViewModel model, CancellationToken cancellationToken)
    {
        var request = new CreateLapcapDataRequest
        {
            Filename = model.Filename,
            RelativeYear =  CommonUtil.GetRelativeYear(HttpContext.Session, CommonUtil.GetRelativeYearStartingMonth(configuration)),
            Values = model.Values
        };

        using var response = await eprCalculatorApiService.CallApi(
            HttpMethod.Post,
            "v1/lapcapData",
            body: request,
            cancellationToken: cancellationToken);

        if (response is { IsSuccessStatusCode: true, StatusCode: HttpStatusCode.Created })
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
        return View("Index", new LapcapUploadViewModel
        {
            ErrorsViewModel = ErrorTemplate with
            {
                ContentErrors = [..problemDetails.Errors.SelectMany(kv => kv.Value)]
            }
        });
    }
}
