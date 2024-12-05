using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    public class LocalAuthorityUploadFileErrorController : Controller
    {
        private readonly TelemetryClient _telemetryClient;

        public LocalAuthorityUploadFileErrorController(TelemetryClient telemetryClient)
        {
            this._telemetryClient = telemetryClient;
        }

        public IActionResult Index()
        {
            try
            {
                var lapcapErrors = this.HttpContext.Session.GetString(UploadFileErrorIds.LocalAuthorityUploadErrors);

                if (!string.IsNullOrEmpty(lapcapErrors))
                {
                    // Log the number of errors as a metric
                    this._telemetryClient.TrackMetric("UploadFileErrorCount", lapcapErrors.Length);

                    // Optionally log additional trace information
                    this._telemetryClient.TrackTrace($"Errors found in uploaded file: {lapcapErrors}");

                    var validationErrors = JsonConvert.DeserializeObject<List<ValidationErrorDto>>(lapcapErrors);

                    if (validationErrors?.Find(error => !string.IsNullOrEmpty(error.ErrorMessage)) != null)
                    {
                        this.ViewBag.ValidationErrors = validationErrors;
                    }
                    else
                    {
                        this.ViewBag.Errors = JsonConvert.DeserializeObject<List<CreateLapcapDataErrorDto>>(lapcapErrors);
                    }

                    if (this.ViewBag.ValidationErrors is null && this.ViewBag.Errors is not null)
                    {
                        this.ViewBag.ValidationErrors = new List<ValidationErrorDto>()
                        {
                            new ValidationErrorDto()
                            {
                                ErrorMessage = this.ViewBag.Errors.Count > 1 ? $"The file contained {this.ViewBag.Errors.Count} errors." : $"The file contained {this.ViewBag.Errors.Count} error.",
                            },
                        };
                    }

                    return this.View(ViewNames.LocalAuthorityUploadFileErrorIndex);
                }
                else
                {
                    // Log trace if no errors are found
                    this._telemetryClient.TrackTrace("No errors found in the uploaded file.");

                    return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
                }
            }
            catch (Exception ex)
            {
                // Track exception in Application Insights
                this._telemetryClient.TrackException(ex);

                return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
        }

        [HttpPost]
        public IActionResult Index([FromBody] string errors)
        {
            this.HttpContext.Session.SetString(UploadFileErrorIds.LocalAuthorityUploadErrors, errors);

            return this.Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile fileUpload)
        {
            var lapcapFileErrors = CsvFileHelper.ValidateCSV(fileUpload);
            if (lapcapFileErrors.ErrorMessage is not null)
            {
                this.ViewBag.DefaultError = lapcapFileErrors;
                return this.View(ViewNames.LocalAuthorityUploadFileErrorIndex);
            }

            var localAuthorityDisposalCostsValues = await CsvFileHelper.PrepareLapcapDataForUpload(fileUpload);

            this.ViewData["localAuthorityDisposalCosts"] = localAuthorityDisposalCostsValues.ToArray();
            this.TempData["LapcapFileName"] = fileUpload.FileName;
            this._telemetryClient.TrackTrace($"Retrieving TempData 4.{this.TempData["LapcapFileName"]}");

            return this.View(ViewNames.LocalAuthorityUploadFileRefresh);
        }
    }
}