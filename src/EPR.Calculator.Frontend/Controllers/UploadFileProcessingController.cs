using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    public class UploadFileProcessingController : Controller
    {
        public IActionResult Index(string abc)
        {
            // var abc = TempData["schemeTemplateParameterValues"];

            var bcd = System.Text.Json.JsonSerializer.Deserialize<List<SchemeParameterTemplateValue>>(abc);

            return View();
        }
    }
}
