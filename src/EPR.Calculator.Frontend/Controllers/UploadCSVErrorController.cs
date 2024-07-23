using System.Text.Json;
using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    public class UploadCSVErrorController : Controller
    {
        public IActionResult Index()
        {
            var listErrorViewModel = new List<ErrorViewModel>
            {
                new() { DOMElementId = string.Empty, ErrorMessage = "Invalid Entry" },
                new() { DOMElementId = string.Empty, ErrorMessage = "Invalid Entry1" },
            };

            TempData["Errors"] = listErrorViewModel;
            ViewBag.Errors = TempData["Errors"];

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile fileUpload)
        {
            if (fileUpload != null)
            {
                var fileName = Path.GetFileName(fileUpload.FileName);
                var filePath = Path.Combine(Path.GetTempPath(), fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await fileUpload.CopyToAsync(stream);
                }

                TempData["FilePath"] = filePath;
            }

            return RedirectToAction("Upload", "UploadFile");
        }
    }
}
