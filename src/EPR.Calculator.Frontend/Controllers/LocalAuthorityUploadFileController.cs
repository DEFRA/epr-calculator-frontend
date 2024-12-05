using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    public class LocalAuthorityUploadFileController : Controller
    {
        private readonly TelemetryClient _telemetryClient;

        public LocalAuthorityUploadFileController(TelemetryClient telemetryClient)
        {
            this._telemetryClient = telemetryClient;
        }

        public IActionResult Index()
        {
            // Track a page visit event
            this._telemetryClient.TrackEvent("1.Visited LocalAuthorityUploadFileIndex");

            return this.View(ViewNames.LocalAuthorityUploadFileIndex);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile fileUpload)
        {
            try
            {
                this._telemetryClient.TrackEvent("2.FileUploadInitiated", new Dictionary<string, string>
                {
                    { "FileName", fileUpload.FileName },
                });

                // Track the file size as a metric
                this._telemetryClient.TrackMetric("UploadedFileSize", fileUpload.Length);

                var lapcapViewName = await this.GetViewName(fileUpload);

                this._telemetryClient.TrackEvent("4.FileUploadCompleted", new Dictionary<string, string>
                {
                    { "FileName", fileUpload.FileName },
                    { "ViewName", lapcapViewName },
                });
                return this.View(lapcapViewName);
            }
            catch (Exception ex)
            {
                this._telemetryClient.TrackException(ex, new Dictionary<string, string>
                 {
                     { "Action", "Upload (HttpPost)" },
                 });

                return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
        }

        public async Task<IActionResult> Upload()
        {
            try
            {
                var lapcapFilePath = this.TempData["LapcapFilePath"]?.ToString();
                this._telemetryClient.TrackTrace($"Retrieving TempData 1.{lapcapFilePath}");

                if (!string.IsNullOrEmpty(lapcapFilePath))
                {
                    using var stream = System.IO.File.OpenRead(lapcapFilePath);
                    var fileUpload = new FormFile(stream, 0, stream.Length, string.Empty, Path.GetFileName(stream.Name));

                    // Log telemetry for the automatic file re-upload
                    this._telemetryClient.TrackEvent("FileReUpload", new Dictionary<string, string>
                    {
                        { "FilePath", lapcapFilePath },
                        { "FileName", fileUpload.FileName },
                    });

                    var viewName = await this.GetViewName(fileUpload);
                    ModelState.Clear();
                    return this.View(viewName);
                }

                // Log a trace if the file path is missing
                this._telemetryClient.TrackTrace("FileReUploadFailed: LapcapFilePath is missing.");

                // Code will reach this point if the uploaded file is not available
                return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
            catch (Exception ex)
            {
                this._telemetryClient.TrackException(ex, new Dictionary<string, string>
                {
                    { "Action", "Upload (ReUpload)" },
                });
                return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
        }

        private async Task<string> GetViewName(IFormFile fileUpload)
        {
            // Track entry into the method
            this._telemetryClient.TrackTrace($"Entering GetViewName for file {fileUpload.FileName}");

            if (this.ValidateCSV(fileUpload).ErrorMessage is not null)
            {
                var uploadErrors = this.TempData[UploadFileErrorIds.LocalAuthorityUploadErrors]?.ToString();
                if (!string.IsNullOrEmpty(uploadErrors))
                {
                    var localAuthorityUploadErrors = JsonConvert.DeserializeObject<ErrorViewModel>(uploadErrors);
                    if (localAuthorityUploadErrors != null)
                    {
                        localAuthorityUploadErrors.DOMElementId = ViewControlNames.FileUpload;
                    }

                    this.ViewBag.Errors = localAuthorityUploadErrors;

                    // Log validation errors
                    this._telemetryClient.TrackEvent("3.2.ValidationErrorsDetected", new Dictionary<string, string>
                    {
                        { "FileName", fileUpload.FileName },
                        { "ErrorDetails", uploadErrors },
                    });
                    return ViewNames.LocalAuthorityUploadFileIndex;
                }
            }

            var localAuthorityDisposalCosts = await CsvFileHelper.PrepareLapcapDataForUpload(fileUpload);

            this.ViewData["localAuthorityDisposalCosts"] = localAuthorityDisposalCosts.ToArray();
            this.TempData["LapcapFileName"] = fileUpload.FileName;
            this._telemetryClient.TrackTrace($"Retrieving TempData 2.{this.TempData["LapcapFileName"]}");

            // Log successful preparation of data
            this._telemetryClient.TrackEvent("3.1.FileDataPrepared", new Dictionary<string, string>
            {
                 { "FileName", fileUpload.FileName },
                 { "DataCount", localAuthorityDisposalCosts.Count.ToString() },
            });

            return ViewNames.LocalAuthorityUploadFileRefresh;
        }

        private ErrorViewModel ValidateCSV(IFormFile fileUpload)
        {
            // Track entry into the method
            this._telemetryClient.TrackTrace($"Validating CSV file {fileUpload.FileName}");

            ErrorViewModel validationErrors = CsvFileHelper.ValidateCSV(fileUpload);

            if (validationErrors.ErrorMessage != null)
            {
                var serializedErrors = JsonConvert.SerializeObject(validationErrors);

                this.TempData[UploadFileErrorIds.LocalAuthorityUploadErrors] = JsonConvert.SerializeObject(validationErrors);
                this._telemetryClient.TrackTrace($"Retrieving TempData 3.{this.TempData[UploadFileErrorIds.LocalAuthorityUploadErrors]}");

                // Log validation errors as a metric and event
                this._telemetryClient.TrackMetric("CSVValidationErrors", 1);
                this._telemetryClient.TrackEvent("CSVValidationFailed", new Dictionary<string, string>
                {
                     { "FileName", fileUpload.FileName },
                     { "ErrorDetails", serializedErrors },
                });
            }

            return validationErrors;
        }
    }
}
