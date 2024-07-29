using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using EPR.Calculator.Frontend.Constants;

namespace EPR.Calculator.Frontend.Controllers
{
    public class UploadCSVErrorController : Controller
    {
        public IActionResult Index()
        {
            var listErrorViewModel = new List<ErrorViewModel>
            {
                new() { DOMElementId = string.Empty, ErrorMessage = "Enter the scheme administrator operating costs for England" },
                new() { DOMElementId = string.Empty, ErrorMessage = "Enter the communication costs for steel" },
                new() { DOMElementId = string.Empty, ErrorMessage = "Communication costs for wood must be between £0 and £999,999,999.99" },
                new() { DOMElementId = string.Empty, ErrorMessage = "Scheme setup costs can only include numbers, commas and decimal points" },
            };

            ViewBag.Errors = listErrorViewModel;
            return View(ViewNames.Index);
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
