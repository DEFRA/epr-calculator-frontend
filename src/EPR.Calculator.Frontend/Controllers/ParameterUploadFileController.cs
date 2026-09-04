using System.Net;
using System.Text.Json;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Helpers.Csv;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using EPR.Calculator.Frontend.ViewModels.CsvUpload;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

public class ParameterUploadFileController(
    IConfiguration configuration,
    IEprCalculatorApiService eprCalculatorApiService,
    ILogger<ParameterUploadFileController> logger
) : BaseController
{
    private const string ApiErrorsKey = "Default_Parameters_Upload_Errors";

    private CsvUploadViewModel UploadTemplate => new()
    {
        Title = "Upload new default calculator parameters",
        BackLinkUrl = Url.Action("Index", "DefaultParameters")!
    };

    private static readonly FileUploadErrorViewModel ErrorTemplate = new()
    {
        InputId = CsvUploadViewModel.DomElements.InputId,
        DetailsId = CsvUploadViewModel.DomElements.ErrorDetailsId,
        CallToActionId = CsvUploadViewModel.DomElements.ErrorCallToActionId,
        FileErrors = [],
        ContentErrors = []
    };

    [HttpGet]
    public IActionResult Index()
    {
        return View("Views/CsvUpload/Index", UploadTemplate);
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile? fileUpload, CancellationToken cancellationToken)
    {
        try
        {
            var result = await DefaultParametersCsvFileHelper.Parse(fileUpload, cancellationToken);

            if (!result.IsSuccess)
            {
                return View("Views/CsvUpload/Index", UploadTemplate with
                {
                    ErrorsViewModel = ErrorTemplate with
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
                Values = result.Records
            };

            return View("Views/CsvUpload/Processing", new CsvUploadProcessingViewModel
            {
                ProcessingUrl = Url.Action("Process", "ParameterUploadFile")!,
                SuccessUrl = Url.Action("Index", "ParameterConfirmation")!,
                ErrorUrl = Url.Action("Errors", "ParameterUploadFile")!,
                JsonPayload = JsonSerializer.Serialize(processRequest)
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Uncaught exception when handling CSV file upload");
            return RedirectToError();
        }
    }

    [HttpPost]
    public async Task<IActionResult> Process([FromBody] SetDefaultParametersRequest request, CancellationToken cancellationToken)
    {
        using var response = await eprCalculatorApiService.CallApi(
            HttpMethod.Post,
            "v1/defaultParameterSetting",
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
        return View("Views/CsvUpload/Index", UploadTemplate with
        {
            ErrorsViewModel = ErrorTemplate with
            {
                ContentErrors = [..problemDetails.Errors.SelectMany(kv => kv.Value)]
            }
        });
    }
}
