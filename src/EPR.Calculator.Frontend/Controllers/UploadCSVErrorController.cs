using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    public class UploadCSVErrorController : Controller
    {
        public IActionResult Index()
        {
            try
            {
                if (HttpContext.Session.GetString("Default_Parameter_Upload_Errors") != null)
                {
                    var errors = JsonConvert.DeserializeObject<List<ErrorDto>>(HttpContext.Session.GetString("Default_Parameter_Upload_Errors"));

                    var listErrorViewModel = new List<ErrorViewModel>();

                    errors.ForEach(error => listErrorViewModel.Add(new() { DOMElementId = string.Empty, ErrorMessage = error.Message }));

                    ViewBag.Errors = listErrorViewModel;
                    return View(ViewNames.UploadCSVErrorIndex);
                }
                else
                {
                    return RedirectToAction("Index", "StandardError");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "StandardError");
            }
        }

        [HttpPost]
        public IActionResult Index([FromBody]string errors)
        {
            HttpContext.Session.SetString("Default_Parameter_Upload_Errors", errors);

            return Ok();
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
