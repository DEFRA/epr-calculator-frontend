using CsvHelper;
using CsvHelper.Configuration;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EPR.Calculator.Frontend.Controllers
{
    public class LocalAuthorityUploadFileController : Controller
    {
        public IActionResult Index()
        {
            return View(ViewNames.LocalAuthorityUploadFileIndex);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile fileUpload)
        {
            try
            {
                if (this.ValidateCSV(fileUpload).ErrorMessage is not null)
                {
                    this.ViewBag.Errors = JsonConvert.DeserializeObject<ErrorViewModel>(TempData["Local_Authority_Upload_Errors"].ToString());
                    return this.View(ViewNames.LocalAuthorityUploadFileIndex);
                }

                var localAuthorityDisposalCosts = await PrepareDataForUpload(fileUpload);

                ViewData["localAuthorityDisposalCosts"] = localAuthorityDisposalCosts.ToArray();

                return View(ViewNames.LocalAuthorityUploadFileRefresh);
            }
            catch (Exception ex)
            {
                return RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
        }

        public async Task<IActionResult> Upload()
        {
            try
            {
                if (TempData["FilePath"] != null)
                {
                    using var stream = System.IO.File.OpenRead(TempData["FilePath"].ToString());
                    var fileUpload = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name));

                    if (ValidateCSV(fileUpload).ErrorMessage is not null)
                    {
                        ViewBag.Errors = JsonConvert.DeserializeObject<ErrorViewModel>(TempData["Local_Authority_Upload_Errors"].ToString());
                        return View(ViewNames.UploadFileIndex);
                    }

                    var localAuthorityDisposalCosts = await PrepareDataForUpload(fileUpload);

                    ViewData["localAuthorityDisposalCosts"] = localAuthorityDisposalCosts.ToArray();

                    return View(ViewNames.LocalAuthorityUploadFileRefresh);
                }

                // Code will reach this point if the uploaded file is not available
                return RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
            catch (Exception ex)
            {
                return RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
        }

        private async Task<List<LocalAuthorityDisposalCostDto>> PrepareDataForUpload(IFormFile fileUpload)
        {
            var localAuthorityDisposalCosts = new List<LocalAuthorityDisposalCostDto>();

            //using var memoryStream = new MemoryStream(new byte[fileUpload.Length]);
            //await fileUpload.CopyToAsync(memoryStream);
            //memoryStream.Position = 0;

            //using (var reader = new StreamReader(memoryStream))
            //{
            //    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            //    {
            //        PrepareHeaderForMatch = header => Regex.Replace(header.ToString(), @"\s", string.Empty, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            //        ShouldSkipRecord = args => args.Row.Parser.Record.All(string.IsNullOrWhiteSpace) || args.Row.GetField(0).Contains("upload version")
            //    };
            //    using (var csv = new CsvReader(reader, config))
            //    {
            //        csv.Read();
            //        while (csv.Read())
            //        {
            //            localAuthorityDisposalCosts.Add(
            //                new LocalAuthorityDisposalCostDto() { LapcapDataTemplateMasterUniqueRef = csv.GetField(0), TotalCost = csv.GetField(5) });
            //        }
            //    }
            //}

            return localAuthorityDisposalCosts;
        }

        private ErrorViewModel ValidateCSV(IFormFile fileUpload)
        {
            ErrorViewModel validationErrors = CsvFileHelper.ValidateCSV(fileUpload);

            if (validationErrors.ErrorMessage != null)
            {
                TempData["Local_Authority_Upload_Errors"] = JsonConvert.SerializeObject(validationErrors);
            }

            return validationErrors;
        }
    }
}
