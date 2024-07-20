using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    public class UploadCSVErrorController : Controller
    {
        public IActionResult Index()
        {
            if (TempData["Errors"] != null)
            {
                ViewBag.Errors = JsonSerializer.Deserialize<List<ErrorViewModel>>(TempData["Errors"].ToString());
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile fileUpload)
        {
            if (fileUpload != null)
            {
                var fileName = Path.GetFileName(fileUpload.FileName);
                var filePath = Path.Combine(Path.GetTempPath(), fileName);
                using (var stream = new FileStream(filePath,FileMode.Create))
                {
                    await fileUpload.CopyToAsync(stream);
                }

                TempData["FilePath"] = filePath;
            }
            return RedirectToAction("Upload", "UploadFile");
        }
    }
}
