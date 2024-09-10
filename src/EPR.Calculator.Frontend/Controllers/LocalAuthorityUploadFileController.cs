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
            if (this.ValidateUploadedCSV(fileUpload).ErrorMessage is not null)
            {
                this.ViewBag.Errors = JsonConvert.DeserializeObject<ErrorViewModel>(TempData["Local_Authority_Upload_Errors"].ToString());
                return this.View(ViewNames.LocalAuthorityUploadFileIndex);
            }

            var localAuthorityDisposalCosts = await PrepareFileDataForUpload(fileUpload);

            ViewData["localAuthorityDisposalCosts"] = localAuthorityDisposalCosts.ToArray();

            return View(ViewNames.LocalAuthorityUploadFileRefresh);
        }

        public async Task<IActionResult> Upload()
        {
            try
            {
                if (TempData["FilePath"] != null)
                {
                    using var stream = System.IO.File.OpenRead(TempData["FilePath"].ToString());
                    var fileUpload = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name));

                    if (ValidateUploadedCSV(fileUpload).ErrorMessage is not null)
                    {
                        ViewBag.Errors = JsonConvert.DeserializeObject<ErrorViewModel>(TempData["Local_Authority_Upload_Errors"].ToString());
                        return View(ViewNames.UploadFileIndex);
                    }

                    var localAuthorityDisposalCosts = await PrepareFileDataForUpload(fileUpload);

                    ViewData["localAuthorityDisposalCosts"] = localAuthorityDisposalCosts.ToArray();

                    return View(ViewNames.LocalAuthorityUploadFileRefresh);
                }

                // Code will reach this point if the uploaded file is not available
                return RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
            catch (Exception)
            {
                return RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
        }

        private async Task<List<LocalAuthorityDisposalCostDto>> PrepareFileDataForUpload(IFormFile fileUpload)
        {
            return new List<LocalAuthorityDisposalCostDto>();
        }

        private ErrorViewModel ValidateUploadedCSV(IFormFile fileUpload)
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
