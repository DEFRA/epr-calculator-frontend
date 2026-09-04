using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

public class ParameterConfirmationController : BaseController
{
    /// <summary>
    ///     Displays the parameter confirmation view.
    /// </summary>
    /// <returns>The parameter confirmation view.</returns>
    public IActionResult Index()
    {
        // Create a view model for parameter confirmation
        var parameterConfirmationViewModel = new ConfirmationViewModel
        {
            Title = ParameterConfirmation.Title,
            Body = ParameterConfirmation.Body,
            AdditionalParagraphs = ParameterConfirmation.AdditionalParagraphs.ToList(),
            RedirectController = ParameterConfirmation.RedirectController,
            SubmitText = ParameterConfirmation.SubmitText
        };

        // Return the view with the view model
        return View(ViewNames.ParameterConfirmationIndex, parameterConfirmationViewModel);
    }
}
