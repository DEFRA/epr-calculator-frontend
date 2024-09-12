using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    public class LocalAuthorityUploadFileController : Controller
    {
        public IActionResult Index()
        {
            return this.View(ViewNames.LocalAuthorityUploadFileIndex);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile fileUpload)
        {
            try
            {
                if (this.ValidateCSV(fileUpload).ErrorMessage is not null)
                {
                    this.ViewBag.Errors = JsonConvert.DeserializeObject<ErrorViewModel>(this.TempData["Local_Authority_Upload_Errors"].ToString());
                    return this.View(ViewNames.LocalAuthorityUploadFileIndex);
                }

                var localAuthorityDisposalCosts = await CsvFileHelper.PrepareLapcapDataForUpload(fileUpload);

                this.ViewData["localAuthorityDisposalCosts"] = localAuthorityDisposalCosts.ToArray();

                return this.View(ViewNames.LocalAuthorityUploadFileRefresh);
            }
            catch (Exception)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
        }

        public async Task<IActionResult> Upload()
        {
            try
            {
                if (this.TempData["FilePath"] != null)
                {
                    using var stream = System.IO.File.OpenRead(this.TempData["FilePath"].ToString());
                    var fileUpload = new FormFile(stream, 0, stream.Length, string.Empty, Path.GetFileName(stream.Name));

                    if (this.ValidateCSV(fileUpload).ErrorMessage is not null)
                    {
                        this.ViewBag.Errors = JsonConvert.DeserializeObject<ErrorViewModel>(this.TempData["Local_Authority_Upload_Errors"].ToString());
                        return this.View(ViewNames.LocalAuthorityUploadFileIndex);
                    }

                    var localAuthorityDisposalCosts = await CsvFileHelper.PrepareLapcapDataForUpload(fileUpload);

                    this.ViewData["localAuthorityDisposalCosts"] = localAuthorityDisposalCosts.ToArray();

                    return this.View(ViewNames.LocalAuthorityUploadFileRefresh);
                }

                // Code will reach this point if the uploaded file is not available
                return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
            catch (Exception)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
        }

        private ErrorViewModel ValidateCSV(IFormFile fileUpload)
        {
            ErrorViewModel validationErrors = CsvFileHelper.ValidateCSV(fileUpload);

            if (validationErrors.ErrorMessage != null)
            {
                this.TempData["Local_Authority_Upload_Errors"] = JsonConvert.SerializeObject(validationErrors);
            }

            return validationErrors;
        }
    }
}
