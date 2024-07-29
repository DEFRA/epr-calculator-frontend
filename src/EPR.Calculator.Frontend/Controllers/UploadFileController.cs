using System.Text.Json;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    public class UploadFileController : Controller
    {
        private const long MaxFileSize = 50 * 1024; // 50 KB
        private static bool _isTaskSuccessful = false;

        public IActionResult Index()
        {
            return View("Index");
        }

        [HttpPost]
        public IActionResult Upload(IFormFile fileUpload)
        {
            if (CsvFileValidation(fileUpload) != null && CsvFileValidation(fileUpload).Count > 0)
            {
                if (TempData["Errors"] != null)
                {
                    ViewBag.Errors = JsonSerializer.Deserialize<List<ErrorViewModel>>(TempData["Errors"].ToString());
                    return View("Index");
                }
            }

            return View("Refresh");
        }

        public IActionResult Upload()
        {
            if (TempData["FilePath"] != null)
            {
                using var stream = System.IO.File.OpenRead(TempData["FilePath"].ToString());
                var fileUpload = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name));
                CsvFileValidation(fileUpload);
            }

            return View("Refresh");
        }

        public IActionResult DownloadCsvTemplate()
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), StaticHelpers.Path);
                return PhysicalFile(filePath, "text/csv", "SchemeParameterTemplate.v0.1.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occured while processing request" + ex.Message);
            }
        }

        private List<ErrorViewModel> CsvFileValidation(IFormFile fileUpload)
        {
            var listErrorViewModel = new List<ErrorViewModel>();
            if (fileUpload == null || fileUpload.Length == 0)
            {
                listErrorViewModel.Add(new ErrorViewModel() { ErrorMessage = StaticHelpers.FileNotSelected });
            }

            if (fileUpload != null && !fileUpload.FileName.EndsWith(".csv"))
            {
                listErrorViewModel.Add(new ErrorViewModel() { ErrorMessage = StaticHelpers.FileMustBeCSV });
            }

            if (fileUpload != null && fileUpload.Length > MaxFileSize)
            {
                listErrorViewModel.Add(new ErrorViewModel() { ErrorMessage = StaticHelpers.FileNotExceed50KB });
            }

            if (listErrorViewModel != null && listErrorViewModel.Count > 0)
            {
                TempData["Errors"] = JsonSerializer.Serialize(listErrorViewModel);
            }

            return listErrorViewModel;
        }
    }
}
