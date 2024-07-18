using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyModel.Resolution;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;

namespace EPR.Calculator.Frontend.Controllers
{
    public class UploadFileController : Controller
    {
        private static bool _isTaskSuccessful = false;
        private const long MaxFileSize = 50 * 1024 * 1024; //50 MB

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Upload(IFormFile fileUpload1)
        {
            var error = string.Empty;
            if (fileUpload1 == null || fileUpload1.Length == 0)
            {
                error = "Please select a file.";
            }
            if (fileUpload1 != null && !fileUpload1.FileName.EndsWith(".csv"))
            {
                error = "The file must be a csv.";
            }
            if (fileUpload1 != null && fileUpload1.Length > MaxFileSize)
            {
                error = "This file size must not exceed 50MB";
            }
            if (!string.IsNullOrEmpty(error))
            {
                var errorModel = new List<ErrorViewModel>
                {
                    new() { DOMElementId = "", ErrorMessage = error }
                };

                TempData["Errors"] = JsonSerializer.Serialize(errorModel);

                return RedirectToAction("Index", "UploadCSVError");
            }
            return View("Refresh");

        }

        [HttpPost]
        public async Task<IActionResult> ProcessData()
        {
            _isTaskSuccessful = false;

            await Task.Delay(5000); // To DO -  API call..

            if (_isTaskSuccessful)
            {
                return RedirectToAction("Index", "ParameterConfirmation");
            }
            else
            {
                return Json(new { newUrl = Url.Action("Index", "UploadCSVError") });
            }
        }

        public IActionResult DownloadCsvTemplate()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "test.csv");
            var mimeType = "text/csv";
            var fileName = "test.csv";
            return PhysicalFile(filePath, mimeType, fileName);

        }

    }
}
