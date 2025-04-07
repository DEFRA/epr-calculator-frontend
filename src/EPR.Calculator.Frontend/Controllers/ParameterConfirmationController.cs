using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    [Authorize(Roles = "SASuperUser")]
    public class ParameterConfirmationController : Controller
    {
        [Authorize(Roles = "SASuperUser")]
        public IActionResult Index()
        {
            var parameterConfirmationViewModel = new ConfirmationViewModel
            {
                Title = ParameterConfirmation.Title,
                Body = ParameterConfirmation.Body,
                NextText = ParameterConfirmation.NextText,
                RedirectController = ParameterConfirmation.RedirectController,
                SubmitText = ParameterConfirmation.SubmitText,
            };
            return this.View(ViewNames.ParameterConfirmationIndex,parameterConfirmationViewModel);
        }
    }
}