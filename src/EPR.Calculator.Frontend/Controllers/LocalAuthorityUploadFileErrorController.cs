using System.Text.Json;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

public class LocalAuthorityUploadFileErrorController : BaseController
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    public IActionResult Index()
    {
        var errorsInSession = HttpContext.Session.GetString(UploadFileErrorIds.LocalAuthorityUploadErrors);

        if(string.IsNullOrWhiteSpace(errorsInSession))
            return RedirectToError();

        var viewModel = new LapcapUploadViewModel();

        using var doc = JsonDocument.Parse(errorsInSession);
        var root = doc.RootElement;

        // API currently returns inconsistent error object types.
        // It needs refactoring to always return standardized ValidationProblemDetails.
        if (IsValidationProblemDetails(root))
        {
            var apiProblemDetails = root.Deserialize<ValidationProblemDetails>(JsonSerializerOptions)!;
            viewModel.ValidationErrors = apiProblemDetails.Errors
                .SelectMany(kv => kv.Value.Select(e => new ValidationErrorDto { ErrorMessage = e }))
                .ToList();
        }
        else if(root.ValueKind == JsonValueKind.Array)
        {
            var lapcapErrors = root.Deserialize<List<CreateLapcapDataErrorDto>>()!;
            viewModel.LapcapErrors = lapcapErrors;
            viewModel.ValidationErrors =
            [
                new ValidationErrorDto
                {
                    ErrorMessage = lapcapErrors!.Count > 1
                        ? $"The file contained {viewModel.LapcapErrors!.Count} errors."
                        : $"The file contained {viewModel.LapcapErrors!.Count} error."
                }
            ];
        }
        else
        {
            viewModel.ValidationErrors = [ new ValidationErrorDto { ErrorMessage = "The file contained an error." } ];
        }

        return View(ViewNames.LocalAuthorityUploadFileErrorIndex, viewModel);
    }

    private static bool IsValidationProblemDetails(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Object)
        {
            var type = root
                .EnumerateObject()
                .Where(kv =>
                    kv.Name.Equals("type", StringComparison.OrdinalIgnoreCase)
                    && kv.Value.ValueKind == JsonValueKind.String)
                .Select(kv => kv.Value.GetString())
                .FirstOrDefault() ?? "";

            return type.StartsWith("https://tools.ietf.org/html/rfc9110", StringComparison.OrdinalIgnoreCase);
        }

        return false;
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
        var lapcapFileErrors = CsvFileHelper.ValidateCSV(fileUpload);
        var lapcapUploadViewModel = new LapcapUploadViewModel();

        if (lapcapFileErrors.ErrorMessage is not null)
        {
            lapcapUploadViewModel.Errors = new List<ErrorViewModel> { lapcapFileErrors };
            return View(
                ViewNames.LocalAuthorityUploadFileErrorIndex,
                lapcapUploadViewModel);
        }

        var localAuthorityDisposalCostsValues = await CsvFileHelper.PrepareLapcapDataForUpload(fileUpload);

        return View(ViewNames.LocalAuthorityUploadFileRefresh, new LapcapRefreshViewModel { LapcapTemplateValue = localAuthorityDisposalCostsValues, FileName = fileUpload.FileName });
    }
}
