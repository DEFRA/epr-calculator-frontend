using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    public class UploadCSVErrorController : Controller
    {
        public IActionResult Index()
        {
            var errors = new List<ErrorViewModel>();
            errors.Add(new ErrorViewModel { DOMElementId = "file-upload-1", ErrorMessage = "Invalid entry" });
            errors.Add(new ErrorViewModel { DOMElementId = "file-upload-1", ErrorMessage = "Invalid entry 1" });

            var viewModel = new UploadCSVErrorViewModel();
            viewModel.errorViewModels = errors;
            return View(viewModel);
        }
    }
}
